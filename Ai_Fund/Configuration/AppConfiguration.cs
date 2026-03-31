namespace Ai_Fund.Configuration;

public static class AppConfiguration
{
    public static string? GetValue(IConfiguration configuration, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value) && !LooksLikePlaceholder(value))
            {
                return value;
            }
        }

        return null;
    }

    public static string GetRequiredValue(IConfiguration configuration, string errorMessage, params string[] keys)
    {
        return GetValue(configuration, keys) ?? throw new InvalidOperationException(errorMessage);
    }

    public static string GetRequiredConnectionString(IConfiguration configuration)
    {
        return GetRequiredValue(
            configuration,
            "Database connection string is missing. Set ConnectionStrings__DefaultConnection or DefaultConnection in the environment.",
            "ConnectionStrings:DefaultConnection",
            "DefaultConnection",
            "ConnectionStrings__DefaultConnection");
    }

    public static bool HasConfiguredValue(IConfiguration configuration, params string[] keys)
    {
        return GetValue(configuration, keys) is not null;
    }

    private static bool LooksLikePlaceholder(string value)
    {
        return value.Contains("YOUR_", StringComparison.OrdinalIgnoreCase)
            || value.Contains("_HERE", StringComparison.OrdinalIgnoreCase)
            || value.Contains("placeholder", StringComparison.OrdinalIgnoreCase);
    }
}
