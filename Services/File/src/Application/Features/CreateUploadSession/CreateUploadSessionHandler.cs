using File.Application.Interfaces;
using File.Domain.Entities;
using GrpcService;
using MediatR;

namespace File.Application.Features.CreateUploadSession;

public sealed class CreateUploadSessionHandler(
    IUploadSessionService uploadSessionService,
    IFileStorageService fileStorageService
    ) : IRequestHandler<CreateUploadSessionCommand, CreateUploadSessionResponse>
{
    public async Task<CreateUploadSessionResponse> Handle(CreateUploadSessionCommand request, CancellationToken cancellationToken)
    {
        var uploadId = await fileStorageService.InitiateMultiPartUploadAsync(request.FileId, cancellationToken);
        var expires = DateTime.UtcNow.AddHours(1);
        var uploadSession = new UploadSession
        {
            UploadId = uploadId,
            FileId = request.FileId,
            Expires = expires
        };

        await uploadSessionService.SetUploadSessionAsync(uploadId, uploadSession);

        return new CreateUploadSessionResponse
        {
            UploadId = uploadId,
            Expires = expires.ToString("O")
        };
    }
}