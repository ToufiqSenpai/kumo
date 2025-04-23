using MediatR;

namespace File.Application.Features.GetUploadSession;

public record GetUploadSessionQuery(string UploadId) : IRequest<GetUploadSessionResponseDto>;