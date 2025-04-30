using File.Application.Features.CreateUploadSession;
using Grpc.Core;
using GrpcService;
using MediatR;

namespace File.Presentation.Grpc;

public class FileService(IMediator mediator) : GrpcService.FileService.FileServiceBase
{
    public override async Task<CreateUploadSessionResponse> CreateUploadSession(CreateUploadSessionRequest request, ServerCallContext context)
    {
        var response = await mediator.Send(new CreateUploadSessionCommand(request.FileId));

        return response;
    }
}