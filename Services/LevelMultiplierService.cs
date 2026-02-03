using KeyHunter.Config;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;

namespace KeyHunter.Services
{
    [Injectable(InjectionType.Singleton)]
    public class LevelMultiplierService
    {
        private readonly PlayerLevelService _playerLevelService;
        private readonly ConfigService _configService;
        private readonly ILogger<LevelMultiplierService> _logger;

        public LevelMultiplierService(
            PlayerLevelService playerLevelService,
            ConfigService configService,
            ILogger<LevelMultiplierService> logger)
        {
            _playerLevelService = playerLevelService;
            _configService = configService;
            _logger = logger;
        }

        public float GetMultiplierForRarity(RarityTier rarity, bool isKeycard)
        {
            var config = _configService.Load();
            if (!config.EnableProgressiveSystem)
            {
                return 1.0f;
            }

            var level = _playerLevelService.GetPlayerLevel();
            var thresholds = config.LevelThresholds;
            RarityMultipliers multipliers;

            if (level <= thresholds.RookieMaxLevel)
            {
                multipliers = thresholds.Rookie;
                _logger.LogDebug("[KeyHunter] Player level {Level} -> Rookie tier", level);
            }
            else if (level <= thresholds.SurvivorMaxLevel)
            {
                multipliers = thresholds.Survivor;
                _logger.LogDebug("[KeyHunter] Player level {Level} -> Survivor tier", level);
            }
            else if (level <= thresholds.VeteranMaxLevel)
            {
                multipliers = thresholds.Veteran;
                _logger.LogDebug("[KeyHunter] Player level {Level} -> Veteran tier", level);
            }
            else
            {
                multipliers = thresholds.Elite;
                _logger.LogDebug("[KeyHunter] Player level {Level} -> Elite tier", level);
            }

            if (isKeycard)
            {
                return multipliers.Keycard;
            }

            return rarity switch
            {
                RarityTier.Common => multipliers.Common,
                RarityTier.Rare => multipliers.Rare,
                RarityTier.SuperRare => multipliers.SuperRare,
                _ => 1.0f
            };
        }

        public float CalculateFinalChance(
            float baseChance,
            RarityTier rarity,
            bool isKeycard,
            bool isHomeMap)
        {
            var config = _configService.Load();
            
            float finalChance = baseChance;

            float levelMultiplier = GetMultiplierForRarity(rarity, isKeycard);
            finalChance *= levelMultiplier;

            if (config.EnableKeyRouting)
            {
                float mapMultiplier = isHomeMap ? config.HomeMapMultiplier : config.OtherMapMultiplier;
                finalChance *= mapMultiplier;
            }

            return finalChance;
        }
    }
}
