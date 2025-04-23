using MediatR;

namespace File.Application.Features.DeleteUploadSession;

public sealed record DeleteUploadSessionCommand(string UploadId) : IRequest<Unit>;