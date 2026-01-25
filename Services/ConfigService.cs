using System.IO;
using System.Reflection;
using KeyHunter.Config;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;

namespace KeyHunter.Services
{
    [Injectable(InjectionType.Singleton)]
    public class ConfigService
    {
        private readonly string _configPath;
        private readonly ILogger<ConfigService> _logger;

        public ConfigService(ILogger<ConfigService> logger)
        {
            _logger = logger;
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string? modDir = Path.GetDirectoryName(assemblyLocation);
            
            // Try explicit Config folder first
            string? configPath = Path.Combine(modDir!, "Config", "config.json");
            if (!File.Exists(configPath))
            {
                // Try root
                configPath = Path.Combine(modDir!, "config.json");
            }
            // Try ../Config/config.json (if dll is in bin)
            if (!File.Exists(configPath))
            {
                 var parent = Directory.GetParent(modDir!);
                 if (parent != null)
                    configPath = Path.Combine(parent.FullName, "Config", "config.json");
            }

            _configPath = configPath ?? "config.json";
        }

        public ModConfig Load()
        {
            if (!File.Exists(_configPath))
            {
                                return new ModConfig();
            }

            try
            {
                string json = File.ReadAllText(_configPath);
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<ModConfig>(json, options) ?? new ModConfig();
            }
            catch (System.Exception)
            {
                                return new ModConfig();
            }
        }
    }
}
