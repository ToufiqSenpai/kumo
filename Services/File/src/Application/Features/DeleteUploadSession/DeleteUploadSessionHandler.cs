using File.Application.Interfaces;
using MediatR;

namespace File.Application.Features.DeleteUploadSession;

public sealed class DeleteUploadSessionHandler(IUploadSessionService uploadSessionService) : IRequestHandler<DeleteUploadSessionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUploadSessionCommand request, CancellationToken cancellationToken)
    {
        await uploadSessionService.DeleteUploadSessionAsync(request.UploadId);
        
        return Unit.Value;
    }
}