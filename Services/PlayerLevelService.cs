using System.Text.Json;
using SPTarkov.DI.Annotations;
using Microsoft.Extensions.Logging;
using KeyHunter.Config;

namespace KeyHunter.Services
{
    [Injectable(InjectionType.Singleton)]
    public class PlayerLevelService
    {
        private readonly ConfigService _configService;
        private readonly ILogger<PlayerLevelService> _logger;
        private int? _cachedLevel = null;

        public PlayerLevelService(
            ConfigService configService,
            ILogger<PlayerLevelService> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public int GetPlayerLevel()
        {
            if (_cachedLevel.HasValue)
            {
                return _cachedLevel.Value;
            }

            var config = _configService.Load();

            // Priority 1: Forced level from config
            if (config.ForcedPlayerLevel.HasValue && config.ForcedPlayerLevel.Value > 0)
            {
                _cachedLevel = config.ForcedPlayerLevel.Value;
                _logger.LogDebug("[KeyHunter] Using forced player level: {Level}", config.ForcedPlayerLevel.Value);
                return config.ForcedPlayerLevel.Value;
            }

            // Priority 2: Auto-detect from profile files
            var detectedLevel = TryGetPlayerLevelFromProfileFiles();
            if (detectedLevel.HasValue)
            {
                _cachedLevel = detectedLevel.Value;
                _logger.LogInformation("[KeyHunter] Auto-detected player level: {Level}", detectedLevel.Value);
                return detectedLevel.Value;
            }

            // Fallback: Default level
            var defaultLevel = 15;
            _cachedLevel = defaultLevel;
            _logger.LogDebug("[KeyHunter] Using default player level: {Level}", defaultLevel);
            return defaultLevel;
        }

        private int? TryGetPlayerLevelFromProfileFiles()
        {
            try
            {
                // Find the profiles directory relative to the mod location
                var profilesPath = FindProfilesDirectory();
                if (string.IsNullOrEmpty(profilesPath) || !Directory.Exists(profilesPath))
                {
                    _logger.LogDebug("[KeyHunter] Profiles directory not found");
                    return null;
                }

                var profileFiles = Directory.GetFiles(profilesPath, "*.json")
                    .Where(f => !Path.GetFileName(f).Equals("activeMods.json", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (profileFiles.Count == 0)
                {
                    _logger.LogDebug("[KeyHunter] No profile files found");
                    return null;
                }

                int maxLevel = 0;
                foreach (var profileFile in profileFiles)
                {
                    try
                    {
                        var level = ExtractLevelFromProfileFile(profileFile);
                        if (level.HasValue && level.Value > maxLevel)
                        {
                            maxLevel = level.Value;
                            _logger.LogDebug("[KeyHunter] Found profile with level: {Level} in {File}", level.Value, Path.GetFileName(profileFile));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("[KeyHunter] Failed to read profile {File}: {Error}", Path.GetFileName(profileFile), ex.Message);
                    }
                }

                if (maxLevel > 0)
                {
                    return maxLevel;
                }

                _logger.LogDebug("[KeyHunter] No valid player levels found in profile files");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[KeyHunter] Failed to auto-detect player level: {Error}", ex.Message);
                return null;
            }
        }

        private string? FindProfilesDirectory()
        {
            // Try to find profiles directory by walking up from current directory
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Try common paths relative to SPT installation
            var possiblePaths = new[]
            {
                Path.Combine(currentDir, "user", "profiles"),
                Path.Combine(currentDir, "..", "user", "profiles"),
                Path.Combine(currentDir, "..", "..", "user", "profiles"),
                Path.Combine(currentDir, "..", "..", "..", "user", "profiles"),
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    _logger.LogDebug("[KeyHunter] Found profiles directory: {Path}", fullPath);
                    return fullPath;
                }
            }

            return null;
        }

        private int? ExtractLevelFromProfileFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            
            // Navigate: characters -> pmc -> Info -> Level
            if (doc.RootElement.TryGetProperty("characters", out var characters) &&
                characters.TryGetProperty("pmc", out var pmc) &&
                pmc.TryGetProperty("Info", out var info) &&
                info.TryGetProperty("Level", out var level))
            {
                return level.GetInt32();
            }

            return null;
        }

        public void ClearCache()
        {
            _cachedLevel = null;
        }
    }
}
