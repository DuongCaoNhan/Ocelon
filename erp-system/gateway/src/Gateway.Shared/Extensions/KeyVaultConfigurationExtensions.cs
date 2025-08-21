using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Gateway.Shared.Extensions;

/// <summary>
/// Extension methods for configuring Azure Key Vault
/// </summary>
public static class KeyVaultConfigurationExtensions
{
    /// <summary>
    /// Adds Azure Key Vault configuration provider for non-development environments
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddKeyVaultIfConfigured(
        this IConfigurationBuilder builder, 
        IHostEnvironment environment)
    {
        // Only add Key Vault in non-development environments
        if (environment.IsDevelopment())
        {
            return builder;
        }

        // Build temporary configuration to get Key Vault URL
        var tempConfig = builder.Build();
        var keyVaultUrl = tempConfig["Azure:KeyVault:VaultUrl"];

        if (!string.IsNullOrEmpty(keyVaultUrl))
        {
            try
            {
                builder.AddAzureKeyVault(
                    new Uri(keyVaultUrl),
                    new DefaultAzureCredential(),
                    new AzureKeyVaultConfigurationOptions
                    {
                        ReloadInterval = TimeSpan.FromMinutes(30),
                        Manager = new KeyVaultSecretManager()
                    });
            }
            catch (Exception ex)
            {
                // Log the error but don't fail startup
                Console.WriteLine($"Warning: Failed to configure Key Vault: {ex.Message}");
            }
        }

        return builder;
    }

    /// <summary>
    /// Gets a connection string with fallback to user secrets in development
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="name">The connection string name</param>
    /// <returns>The connection string value</returns>
    public static string? GetConnectionStringWithFallback(
        this IConfiguration configuration, 
        string name)
    {
        // Try connection strings section first
        var connectionString = configuration.GetConnectionString(name);
        
        // Fallback to direct configuration path for flexibility
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration[$"ConnectionStrings:{name}"];
        }

        return connectionString;
    }

    /// <summary>
    /// Gets a configuration value with Key Vault secret name transformation
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="key">The configuration key</param>
    /// <returns>The configuration value</returns>
    public static string? GetValueWithKeyVault(
        this IConfiguration configuration, 
        string key)
    {
        // Try the original key first
        var value = configuration[key];
        
        // If not found, try Key Vault naming convention (replace : with --)
        if (string.IsNullOrEmpty(value))
        {
            var keyVaultKey = key.Replace(":", "--");
            value = configuration[keyVaultKey];
        }

        return value;
    }
}