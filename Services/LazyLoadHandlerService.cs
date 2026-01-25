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
    public class LazyLoadHandlerService(
        DatabaseService databaseService,
        ConfigService configService,
        ILogger<LazyLoadHandlerService> logger
    )
    {
        private readonly DatabaseService _databaseService = databaseService;
        private readonly ConfigService _configService = configService;
        private readonly ILogger<LazyLoadHandlerService> _logger = logger;

        public void OnPostDBLoad()
        {
            var locations = _databaseService.GetLocations().GetDictionary();

            foreach ((string locationId, Location location) in locations)
            {
                if (location.StaticLoot is not null)
                {
                    
                    location.StaticLoot.AddTransformer(lazyloadedStaticLootData =>
                    {
                        HandleStaticLootLazyLoad(locationId, lazyloadedStaticLootData);
                        return lazyloadedStaticLootData;
                    });
                }
            }
            
        }

        private void HandleStaticLootLazyLoad(string locationId, Dictionary<MongoId, StaticLootDetails>? staticLootData)
        {
            if (staticLootData is null)
            {
                return;
            }

            var config = _configService.Load();
            
            Stopwatch sw = Stopwatch.StartNew();
            foreach ((MongoId containerId, StaticLootDetails lootDetails) in staticLootData)
            {
                // Check if this is one of our target containers
                if (config.TargetContainers.Contains(containerId.ToString()))
                {
                                        
                    // Add keys to this container
                    AddKeysToContainer(containerId, lootDetails, config);
                }
            }

            sw.Stop();
                    }

        private void AddKeysToContainer(MongoId containerId, StaticLootDetails containerDetails, ModConfig config)
        {
            if (containerDetails.ItemDistribution == null)
            {
                                return;
            }

            if (config.DebugLogging)
            {
                            }

            // Convert to list to modify
            var itemDistributionList = containerDetails.ItemDistribution.ToList();
            int keysAdded = 0;
            int keysUpdated = 0;

            // ALLE Keys mit gleicher Wahrscheinlichkeit hinzufügen!
            // SPT wählt dann zufällig aus diesem Pool für jeden Container
            foreach (var keyId in config.KeysToAdd)
            {
                var existingKey = itemDistributionList.FirstOrDefault(x => x.Tpl == keyId);
                if (existingKey != null)
                {
                    existingKey.RelativeProbability = config.KeySpawnChance;
                    keysUpdated++;
                }
                else
                {
                    itemDistributionList.Add(new ItemDistribution
                    {
                        Tpl = keyId,
                        RelativeProbability = config.KeySpawnChance
                    });
                    keysAdded++;
                }
            }
            
                        
            // Update the container with modified list
            containerDetails.ItemDistribution = itemDistributionList;
        }
    }
}
