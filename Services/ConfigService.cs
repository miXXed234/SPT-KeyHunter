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
        private ModConfig? _cachedConfig;

        public ConfigService(ILogger<ConfigService> logger)
        {
            _logger = logger;
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string? modDir = Path.GetDirectoryName(assemblyLocation);
            
            string? configPath = Path.Combine(modDir!, "Config", "config.json");
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(modDir!, "config.json");
            }
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
            if (_cachedConfig != null)
            {
                return _cachedConfig;
            }

            if (!File.Exists(_configPath))
            {
                _logger.LogWarning("[KeyHunter] Config not found at {Path}, using defaults", _configPath);
                _cachedConfig = new ModConfig();
                return _cachedConfig;
            }

            try
            {
                string json = File.ReadAllText(_configPath);
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var config = JsonSerializer.Deserialize<ModConfig>(json, options);
                _cachedConfig = config ?? new ModConfig();
                
                return _cachedConfig;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "[KeyHunter] Error loading config, using defaults");
                _cachedConfig = new ModConfig();
                return _cachedConfig;
            }
        }

        public void Reload()
        {
            _cachedConfig = null;
        }

        public bool IsDebugLoggingEnabled()
        {
            return Load().DebugLogging;
        }
    }
}
