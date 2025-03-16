using System.Text.Json;
using Shared.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.Common.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (HttpResponseException ex)
        {
            context.Response.StatusCode = (int) ex.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new { message = ex.Message, errors = ex.Errors };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            logger.LogError(ex, ex.Message);
            logger.LogTrace(ex.StackTrace);

            var response = new { message = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}