using System.Collections;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Common.Extensions;

public static class ServiceExtensions
{
    public static void AddEnvironmentConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            string envVarName = entry.Key.ToString()!;
            string envVarValue = entry.Value?.ToString()!;
            
            string? configKey = ConvertEnvVarToConfigKey(envVarName);

            if (configKey != null && !string.IsNullOrEmpty(envVarValue))
            {
                configuration[configKey] = envVarValue;
            }
        }
    }
    
    private static string ToPascalCase(string str)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }
    
    private static string? ConvertEnvVarToConfigKey(string envVarName)
    {
        string[] parts = envVarName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return null;
        }
        
        string section = ToPascalCase(parts[0]);
        
        string key = string.Join("", parts.Skip(1).Select(ToPascalCase));

        return $"{section}:{key}";
    }
}