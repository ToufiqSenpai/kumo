using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Common.Filters;

public class ModelStateFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, List<string>>();

            foreach (var kvp in context.ModelState)
            {
                var key = char.ToLowerInvariant(kvp.Key[0]) + kvp.Key.Substring(1);
                errors[key] = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList();
            }

            var formattedErrors = errors.ToDictionary(
                kvp =>
                {
                    if (kvp.Key.StartsWith("$."))
                    {
                        return kvp.Key.Substring(2);
                    }

                    return kvp.Key;
                },
                kvp => kvp.Value.ToArray()
            );

            formattedErrors.Remove("request");

            context.Result = new JsonResult(new { Message = "Bad Request", Errors = formattedErrors }) 
                { StatusCode = 400 };
        }
    }
    
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}