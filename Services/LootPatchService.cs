using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KeyHunter.Config;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Services;
using Microsoft.Extensions.Logging;

namespace KeyHunter.Services
{
    [Injectable(InjectionType.Singleton)]
    public class LootPatchService
    {
        private readonly DatabaseService _databaseService;
        private readonly ConfigService _configService;
        private readonly KeyDataService _keyDataService;
        private readonly PlayerLevelService _playerLevelService;
        private readonly LevelMultiplierService _levelMultiplierService;
        private readonly ILogger<LootPatchService> _logger;

        public LootPatchService(
            DatabaseService databaseService,
            ConfigService configService,
            KeyDataService keyDataService,
            PlayerLevelService playerLevelService,
            LevelMultiplierService levelMultiplierService,
            ILogger<LootPatchService> logger)
        {
            _databaseService = databaseService;
            _configService = configService;
            _keyDataService = keyDataService;
            _playerLevelService = playerLevelService;
            _levelMultiplierService = levelMultiplierService;
            _logger = logger;
        }

        public void OnPostDBLoad()
        {
            var config = _configService.Load();
            
            if (!config.Enabled)
            {
                _logger.LogInformation("[KeyHunter] Mod disabled in config");
                return;
            }

            _keyDataService.Initialize();

            var locations = _databaseService.GetLocations().GetDictionary();

            foreach ((string locationId, Location location) in locations)
            {
                if (location.StaticLoot is not null)
                {
                    string currentLocation = locationId;
                    
                    location.StaticLoot.AddTransformer(lazyloadedStaticLootData =>
                    {
                        HandleStaticLootLazyLoad(currentLocation, lazyloadedStaticLootData);
                        return lazyloadedStaticLootData;
                    });
                }
            }

            _logger.LogInformation("[KeyHunter] Loot transformers registered for {Count} locations", locations.Count);
        }

        private void HandleStaticLootLazyLoad(string locationId, Dictionary<MongoId, StaticLootDetails>? staticLootData)
        {
            if (staticLootData is null) return;

            Stopwatch sw = Stopwatch.StartNew();
            int containersPatched = 0;

            foreach ((MongoId containerId, StaticLootDetails lootDetails) in staticLootData)
            {
                var config = _configService.Load();
                if (config.TargetContainers.Contains(containerId.ToString()))
                {
                    PatchContainer(locationId, containerId, lootDetails);
                    containersPatched++;
                }
            }

            sw.Stop();
            _logger.LogDebug("[KeyHunter] Patched {Containers} containers in {Location} in {Ms}ms", containersPatched, locationId, sw.ElapsedMilliseconds);
        }

        private void PatchContainer(string locationId, MongoId containerId, StaticLootDetails containerDetails)
        {
            var config = _configService.Load();
            
            if (containerDetails.ItemDistribution == null) return;

            var itemDistributionList = containerDetails.ItemDistribution.ToList();
            var allKeys = _keyDataService.GetAllKeyIds().ToList();
            int keysAdded = 0;

            foreach (var keyId in allKeys)
            {
                var keyInfo = _keyDataService.GetKeyInfo(keyId);
                if (keyInfo == null) continue;

                if (itemDistributionList.Any(x => x.Tpl == keyId))
                {
                    continue;
                }

                bool isHomeMap = config.EnableKeyRouting ? IsHomeMapForKey(locationId, keyInfo) : true;
                float finalChance = _levelMultiplierService.CalculateFinalChance(
                    config.BaseKeySpawnChance,
                    keyInfo.Rarity,
                    keyInfo.IsKeycard,
                    isHomeMap
                );

                if (finalChance >= 1)
                {
                    itemDistributionList.Add(new ItemDistribution
                    {
                        Tpl = keyId,
                        RelativeProbability = finalChance
                    });
                    keysAdded++;
                }
            }

            containerDetails.ItemDistribution = itemDistributionList;

            _logger.LogDebug(
                "[KeyHunter] Container {Container}: Added {Added} keys",
                containerId,
                keysAdded
            );
        }

        private bool IsHomeMapForKey(string currentLocation, KeyInfo keyInfo)
        {
            if (keyInfo.HomeMap == "generic") return true;
            if (keyInfo.HomeMap == "factory4_day" || keyInfo.HomeMap == "factory4_night")
                return currentLocation == "factory4_day" || currentLocation == "factory4_night";
            if (keyInfo.HomeMap == "sandbox" || keyInfo.HomeMap == "sandbox_high")
                return currentLocation == "sandbox" || currentLocation == "sandbox_high";
            return currentLocation == keyInfo.HomeMap;
        }
    }
}
