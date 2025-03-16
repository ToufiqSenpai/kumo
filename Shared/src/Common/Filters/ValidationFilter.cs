using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Common.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Skip validation if the action has no arguments
        if (context.ActionArguments.Count == 0)
        {
            await next();
            return;
        }

        // Validate all action arguments
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            var modelType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(modelType);

            // Get validator from DI
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;
            if (validator == null) continue;

            // Create ValidationContext<T> dynamically
            var validationContextType = typeof(ValidationContext<>).MakeGenericType(modelType);
            var validationContext = Activator.CreateInstance(validationContextType, argument) as IValidationContext;

            // Validate asynchronously
            var validationResult = await validator.ValidateAsync(validationContext);

            if (!validationResult.IsValid)
            {
                var errors = new Dictionary<string, List<string>>();
                
                foreach (var error in validationResult.Errors)
                {
                    // Konversi property name ke camelCase
                    var key = char.ToLowerInvariant(error.PropertyName[0]) + error.PropertyName.Substring(1);

                    if (!errors.ContainsKey(key))
                    {
                        errors[key] = new List<string>();
                    }
                    
                    errors[key].Add(error.ErrorMessage);
                }

                var formattedErrors = errors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToArray()
                );
                
                context.Result = new JsonResult(new { Message = "Bad Request", Errors = formattedErrors }) 
                    { StatusCode = 400 };
                return;
            }
        }

        await next(); // Proceed if validation passes
    }
}