using File.Application.Interfaces;
using File.Domain.ValueObjects;
using FluentValidation;

namespace File.Application.Features.PutChunkFile;

public class PutChunkFileHeaderValidator : AbstractValidator<PutChunkFileHeader>
{
    private const int MaxRangeSize = 30 * 1024; // 30 MB

    public PutChunkFileHeaderValidator(IUploadSessionService uploadSessionService, IHttpContextAccessor contextAccessor)
    {
        
        var uploadId = contextAccessor.HttpContext?.Request.Query["uploadId"];

        RuleFor(x => x.ContentRange)
            .MustAsync(async (contentRange, _) =>
            {
                var uploadSession = await uploadSessionService.GetUploadSessionAsync(uploadId ?? string.Empty);
                var latestChunk = uploadSession?.FileParts.Last();

                return latestChunk is not null && contentRange.RangeStart != latestChunk.End + 1;
            }).WithMessage("The start of the range must be equal to the end of the last chunk + 1.")
            .MustAsync(async (contentRange, _) =>
            {
                var uploadSession = await uploadSessionService.GetUploadSessionAsync(uploadId ?? string.Empty);

                return uploadSession?.Size is null || contentRange.Size == uploadSession.Size;
            }).WithMessage("The size of the content range must be equal to the size of the upload session.")
            .Must(contentRange => contentRange.RangeEnd - contentRange.RangeStart < MaxRangeSize)
            .WithMessage($"Range size cannot exceed {MaxRangeSize} bytes.");
        
        RuleFor(x => x.ContentLength)
            .Must(contentLength => contentLength > 0)
            .WithMessage("Content length must be greater than 0.")
            .Must(contentLength => contentLength < MaxRangeSize)
            .WithMessage($"Content length cannot exceed {MaxRangeSize} bytes.");
    }
}