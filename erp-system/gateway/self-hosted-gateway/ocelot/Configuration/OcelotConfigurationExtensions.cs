using Microsoft.Extensions.Configuration;

namespace Gateway.Ocelot.Configuration
{
    public static class OcelotConfigurationExtensions
    {
        public static IConfigurationBuilder AddOcelotConfiguration(this IConfigurationBuilder builder, IWebHostEnvironment environment)
        {
            // Add base configuration first
            builder.AddJsonFile("ocelot.json", optional: true, reloadOnChange: true);
            
            // Add environment-specific configuration
            var environmentName = environment.EnvironmentName;
            builder.AddJsonFile($"ocelot.{environmentName}.json", optional: false, reloadOnChange: true);
            
            // Add optional local overrides (for developers)
            builder.AddJsonFile("ocelot.local.json", optional: true, reloadOnChange: true);
            
            return builder;
        }
    }
    
    public class EnvironmentSettings
    {
        public string Environment { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public bool EnableDetailedErrors { get; set; }
        public bool EnableSwagger { get; set; } = true;
        public LoggingSettings Logging { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
    }
    
    public class LoggingSettings
    {
        public bool EnableRequestLogging { get; set; }
        public bool EnableResponseLogging { get; set; }
        public bool EnablePerformanceLogging { get; set; }
        public string LogLevel { get; set; } = "Information";
    }
    
    public class SecuritySettings
    {
        public bool EnableCors { get; set; } = true;
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public bool RequireHttps { get; set; }
        public bool EnableSecurityHeaders { get; set; } = true;
    }
}
