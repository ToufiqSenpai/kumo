using System.Collections;
using System.Net;
using Amazon.S3.Model;
using File.Application.Interfaces;
using File.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Exceptions;
using Shared.Events.Item;

namespace File.Application.Features.PutChunkFile;

public sealed class PutChunkFileHandler(
    IUploadSessionService uploadSessionService, 
    IFileStorageService fileStorageService,
    IFileMetadataExtractor fileMetadataExtractor,
    IPublishEndpoint publishEndpoint
    ) : IRequestHandler<PutChunkFileCommand, IActionResult>
{
    public async Task<IActionResult> Handle(PutChunkFileCommand request, CancellationToken cancellationToken)
    {
        var uploadSession = await GetUploadSessionAsync(request);
        
        await UploadPartAsync(uploadSession, request, cancellationToken);
        
        // Check if all parts are uploaded
        if (uploadSession.FileParts.Last().End + 1 == uploadSession.Size)
        {
            return await FinishUploadAsync(uploadSession, cancellationToken);
        }
        else
        {
            return await ContinueUploadAsync(uploadSession);
        }
    }

    private async Task<UploadSession> GetUploadSessionAsync(PutChunkFileCommand request)
    {
        var uploadSession = await uploadSessionService.GetUploadSessionAsync(request.UploadId);
        
        if (uploadSession is null)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "Upload session not found.");
        }
        
        // Set upload size on initial upload
        if (uploadSession.Size == 0)
        {
            uploadSession.Size = request.Header.ContentRange.Size ?? 0;
        }
        
        return uploadSession;
    }
    
    private async Task UploadPartAsync(UploadSession uploadSession, PutChunkFileCommand request, CancellationToken cancellationToken)
    {
        var filePart = await fileStorageService.UploadPartAsync(
            uploadSession.FileId, 
            uploadSession.UploadId, 
            uploadSession.FileParts.Count + 1, 
            request.FileStream, 
            cancellationToken);
        
        filePart.Start = request.Header.ContentRange.RangeStart;
        filePart.End = request.Header.ContentRange.RangeEnd;
        
        uploadSession.FileParts.Add(filePart);
    }
    
    private async Task<IActionResult> ContinueUploadAsync(UploadSession uploadSession)
    {
        // Update upload session
        // Extend session exipres
        var newExpires = DateTime.UtcNow.AddHours(1);
        uploadSession.Expires = newExpires;
            
        await uploadSessionService.SetUploadSessionAsync(uploadSession.UploadId, uploadSession);
            
        return new AcceptedResult("", new PutChunkFileContinueUploadResponseDto
        {
            Expires = newExpires,
            NextStartRange = uploadSession.FileParts.Last().End + 1
        });
    }

    private async Task<IActionResult> FinishUploadAsync(UploadSession uploadSession, CancellationToken cancellationToken)
    {
        // Complete upload
        await fileStorageService.CompleteMultiPartUploadAsync(
            uploadSession.FileId, 
            uploadSession.UploadId, 
            uploadSession.FileParts, 
            cancellationToken);
            
        // Remove upload session
        await uploadSessionService.DeleteUploadSessionAsync(uploadSession.UploadId);

        var mimeType = await fileMetadataExtractor.GetFileMimeTypeAsync(uploadSession.FileId, cancellationToken);
            
        fileMetadataExtractor.EnqueueFileMetadataExtractionAsync(uploadSession.FileId, mimeType, cancellationToken);
        await publishEndpoint.Publish(new FinishUpload(uploadSession.Size, mimeType));
            
        return new OkObjectResult(new PutChunkFileFinishUploadResponseDto
        {
            FileId = uploadSession.FileId
        });
    }
}