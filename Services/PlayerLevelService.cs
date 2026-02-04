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
        private string? _cachedProfileId = null;

        public PlayerLevelService(
            ConfigService configService,
            ILogger<PlayerLevelService> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public int GetPlayerLevel()
        {
            var config = _configService.Load();
            
            // Check if cache is still valid (profile hasn't changed)
            var currentProfileId = GetCurrentActiveProfileId();
            if (_cachedLevel.HasValue && _cachedProfileId == currentProfileId)
            {
                return _cachedLevel.Value;
            }
            
            // Cache invalid or profile changed - clear and re-detect
            if (_cachedLevel.HasValue && _cachedProfileId != currentProfileId)
            {
                if (config.DebugLogging)
                    _logger.LogDebug("[KeyHunter] Active profile changed from {Old} to {New}, re-detecting level", _cachedProfileId ?? "none", currentProfileId ?? "none");
                _cachedLevel = null;
                _cachedProfileId = null;
            }

            // Priority 1: Forced level from config
            if (config.ForcedPlayerLevel.HasValue && config.ForcedPlayerLevel.Value > 0)
            {
                _cachedLevel = config.ForcedPlayerLevel.Value;
                _cachedProfileId = "forced";
                _logger.LogInformation("[KeyHunter] Player level: {Level} (source: forced)", config.ForcedPlayerLevel.Value);
                return config.ForcedPlayerLevel.Value;
            }

            // Priority 2: Auto-detect from profile files
            var detectedLevel = TryGetPlayerLevelFromProfileFiles(out var profileSource, out var detectedProfileId);
            if (detectedLevel.HasValue)
            {
                _cachedLevel = detectedLevel.Value;
                _cachedProfileId = detectedProfileId;
                _logger.LogInformation("[KeyHunter] Player level: {Level} (source: {Source})", detectedLevel.Value, profileSource);
                return detectedLevel.Value;
            }

            // Fallback: Default level
            var defaultLevel = 15;
            _cachedLevel = defaultLevel;
            _cachedProfileId = "default";
            _logger.LogInformation("[KeyHunter] Player level: {Level} (source: default)", defaultLevel);
            return defaultLevel;
        }

        private string? GetCurrentActiveProfileId()
        {
            try
            {
                var profilesPath = FindProfilesDirectory();
                if (string.IsNullOrEmpty(profilesPath))
                    return null;
                    
                return TryGetActiveProfileId(profilesPath);
            }
            catch
            {
                return null;
            }
        }

        private int? TryGetPlayerLevelFromProfileFiles(out string source, out string? profileId)
        {
            source = "unknown";
            profileId = null;
            try
            {
                var debugConfig = _configService.Load();
                
                // Find the profiles directory relative to the mod location
                var profilesPath = FindProfilesDirectory();
                if (string.IsNullOrEmpty(profilesPath) || !Directory.Exists(profilesPath))
                {
                    if (debugConfig.DebugLogging)
                        _logger.LogDebug("[KeyHunter] Profiles directory not found");
                    return null;
                }

                // Try to get active profile ID from launcher config
                var activeProfileId = TryGetActiveProfileId(profilesPath);
                
                if (!string.IsNullOrEmpty(activeProfileId))
                {
                    // Try to load the active profile
                    var activeProfilePath = Path.Combine(profilesPath, $"{activeProfileId}.json");
                    if (File.Exists(activeProfilePath))
                    {
                        var level = ExtractLevelFromProfileFile(activeProfilePath);
                        if (level.HasValue)
                        {
                            source = $"activeProfile:{activeProfileId}";
                            profileId = activeProfileId;
                            if (debugConfig.DebugLogging)
                                _logger.LogDebug("[KeyHunter] Found active profile level: {Level}", level.Value);
                            return level.Value;
                        }
                    }
                }

                // Fallback: If no active profile found, use the first valid profile
                var profileFiles = Directory.GetFiles(profilesPath, "*.json")
                    .Where(f => !Path.GetFileName(f).Equals("activeMods.json", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (profileFiles.Count == 0)
                {
                    if (debugConfig.DebugLogging)
                        _logger.LogDebug("[KeyHunter] No profile files found");
                    return null;
                }

                foreach (var profileFile in profileFiles)
                {
                    try
                    {
                        var level = ExtractLevelFromProfileFile(profileFile);
                        if (level.HasValue)
                        {
                            var detectedId = Path.GetFileNameWithoutExtension(profileFile);
                            profileId = detectedId;
                            source = $"firstProfile:{detectedId}";
                            if (debugConfig.DebugLogging)
                                _logger.LogDebug("[KeyHunter] Using first available profile with level: {Level}", level.Value);
                            return level.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debugConfig.DebugLogging)
                            _logger.LogDebug("[KeyHunter] Failed to read profile {File}: {Error}", Path.GetFileName(profileFile), ex.Message);
                    }
                }

                if (debugConfig.DebugLogging)
                    _logger.LogDebug("[KeyHunter] No valid player levels found in profile files");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[KeyHunter] Failed to auto-detect player level: {Error}", ex.Message);
                return null;
            }
        }

        private string? TryGetActiveProfileId(string profilesPath)
        {
            try
            {
                // Try to find launcher config in parent directory
                var launcherConfigPath = Path.Combine(Path.GetDirectoryName(profilesPath) ?? "", "launcher", "config.json");
                if (File.Exists(launcherConfigPath))
                {
                    var json = File.ReadAllText(launcherConfigPath);
                    using var doc = JsonDocument.Parse(json);
                    
                    if (doc.RootElement.TryGetProperty("activeProfileId", out var profileId))
                    {
                        return profileId.GetString();
                    }
                }

                // Alternative: Try to find most recently modified profile file
                var profileFiles = Directory.GetFiles(profilesPath, "*.json")
                    .Where(f => !Path.GetFileName(f).Equals("activeMods.json", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (profileFiles.Count > 0)
                {
                    var mostRecent = profileFiles
                        .Select(f => new { Path = f, LastWrite = File.GetLastWriteTime(f) })
                        .OrderByDescending(x => x.LastWrite)
                        .First();
                    
                    return Path.GetFileNameWithoutExtension(mostRecent.Path);
                }
            }
            catch (Exception ex)
            {
                var debugConfig = _configService.Load();
                if (debugConfig.DebugLogging)
                    _logger.LogDebug("[KeyHunter] Failed to get active profile ID: {Error}", ex.Message);
            }

            return null;
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
                    var debugConfig = _configService.Load();
                    if (debugConfig.DebugLogging)
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
            _cachedProfileId = null;
        }
    }
}
