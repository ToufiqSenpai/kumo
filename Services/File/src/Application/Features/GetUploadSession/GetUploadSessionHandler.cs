using System.Net;
using File.Application.Interfaces;
using MediatR;
using Shared.Common.Exceptions;

namespace File.Application.Features.GetUploadSession;

public sealed class GetUploadSessionHandler(IUploadSessionService uploadSessionService) : IRequestHandler<GetUploadSessionQuery, GetUploadSessionResponseDto>
{
    public async Task<GetUploadSessionResponseDto> Handle(GetUploadSessionQuery request, CancellationToken cancellationToken)
    {
        var uploadSession = await uploadSessionService.GetUploadSessionAsync(request.UploadId);
        
        if (uploadSession is null)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "Upload session not found.");
        }
        
        long nextStartRange = 0;

        if (uploadSession.FileParts.Count > 0)
        {
            nextStartRange = uploadSession.FileParts.Last().End + 1;
        }

        return new GetUploadSessionResponseDto
        {
            Expires = uploadSession.Expires,
            NextStartRange = nextStartRange
        };
    }
}