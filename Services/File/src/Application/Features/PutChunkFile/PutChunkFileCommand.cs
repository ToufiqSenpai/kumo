using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace File.Application.Features.PutChunkFile;

public sealed record PutChunkFileCommand(string UploadId, Stream FileStream, PutChunkFileHeader Header) : IRequest<IActionResult>;