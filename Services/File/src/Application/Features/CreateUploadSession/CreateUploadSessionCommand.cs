using GrpcService;
using MediatR;

namespace File.Application.Features.CreateUploadSession;

public record CreateUploadSessionCommand(string FileId) : IRequest<CreateUploadSessionResponse>;