using System.Net;

namespace Shared.Common.Exceptions;

public class HttpResponseException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public override string Message { get; }
    public object Errors { get; }  
    
    public HttpResponseException(HttpStatusCode statusCode, string message, object? errors = null) : base(message)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors ?? new object();
    }
}