using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Shared.Common.Validators;

public class OptionsDataAnnotationsValidator<T> : IValidateOptions<T> where T : class
{
    public ValidateOptionsResult Validate(string? name, T options)
    {
        if (options is null) return ValidateOptionsResult.Fail($"{typeof(T).Name} options are required.");
        
        var validationContext = new ValidationContext(options);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true);
        
        if (isValid)
        {
            return ValidateOptionsResult.Success;
        }
        
        var errors = validationResults.Select(r => r.ErrorMessage).ToArray();
        return ValidateOptionsResult.Fail(errors);
    }
}