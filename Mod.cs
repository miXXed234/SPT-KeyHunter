using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using KeyHunter.Config;
using KeyHunter.Services;
using SemanticVersioning;
using Version = SemanticVersioning.Version;
using Range = SemanticVersioning.Range;

namespace KeyHunter
{
    public record KeyHunterModMetadata : AbstractModMetadata
    {
        public override string ModGuid { get; init; }  = "com.keyhunter.spt40";
        public override string Name { get; init; } = "KeyHunter";
        public override string Author { get; init; } = "miXXed";
        public override Version Version { get; init; } = new("2.0.0");
        public override string? Url { get; init; } = "https://github.com/miXXed234/SPT-KeyHunter";
        public override Range SptVersion { get; init; } = new("~4.0");
        public override bool? IsBundleMod { get; init; } = false;
        public override string License { get; init; } = "MIT";
        public override List<string>? Contributors { get; init; } = new() { "MusicManiac (Original Mod)" };
        public override List<string>? Incompatibilities { get; init; } = new();
        public override Dictionary<string, Range>? ModDependencies { get; init; } = new();
    }

    public static class KeyHunterLoadPriority
    {
        public const int KeyHunterPriorityOffset = 1000;
    }

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + KeyHunterLoadPriority.KeyHunterPriorityOffset)]
    public class Mod : IOnLoad
    {
        private readonly ConfigService _configService;
        private readonly LootPatchService _lootPatchService;
        private readonly PriceAdjustmentService _priceAdjustmentService;
        private readonly ILogger<Mod> _logger;

        public Mod(
            ConfigService configService,
            LootPatchService lootPatchService,
            PriceAdjustmentService priceAdjustmentService,
            ILogger<Mod> logger
        )
        {
            _configService = configService;
            _lootPatchService = lootPatchService;
            _priceAdjustmentService = priceAdjustmentService;
            _logger = logger;
        }

        public Task OnLoad()
        {
            try
            {
                var config = _configService.Load();
                
                if (!config.Enabled)
                {
                    _logger.LogInformation("[KeyHunter] Mod is disabled in config");
                    return Task.CompletedTask;
                }

                _priceAdjustmentService.AdjustPrices();
                _lootPatchService.OnPostDBLoad();

                _logger.LogInformation("[KeyHunter] v2.0.0 loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KeyHunter] Error during initialization");
            }

            return Task.CompletedTask;
        }
    }
}
