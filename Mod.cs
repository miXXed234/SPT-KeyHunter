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
        public override Version Version { get; init; } = new("1.0.2");
        public override string? Url { get; init; } = "https://github.com/miXXed234/SPT-KeyHunter";
        public override Range SptVersion { get; init; } = new("~4.0");
        public override bool? IsBundleMod { get; init; } = false;
        public override string License { get; init; } = "MIT";
        public override List<string>? Contributors { get; init; } = new List<string>();
        public override List<string>? Incompatibilities { get; init; } = new List<string>();
        public override Dictionary<string, Range>? ModDependencies { get; init; } = new Dictionary<string, Range>();
    }

    public static class KeyHunterLoadPriority
    {
        public const int KeyHunterPriorityOffset = 6;
    }

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + KeyHunterLoadPriority.KeyHunterPriorityOffset)]
    public class Mod : IOnLoad
    {
        private readonly DatabaseService _databaseService;
        private readonly LazyLoadHandlerService _lazyLoadHandlerService;
        private readonly ConfigService _configService;
        private readonly ILogger<Mod> _logger;

        public Mod(
            DatabaseService databaseService,
            LazyLoadHandlerService lazyLoadHandlerService,
            ConfigService configService,
            ILogger<Mod> logger
        )
        {
            _databaseService = databaseService;
            _lazyLoadHandlerService = lazyLoadHandlerService;
            _configService = configService;
            _logger = logger;
        }

        public Task OnLoad()
        {
            var config = _configService.Load();
            
            if (!config.Enabled)
            {
                return Task.CompletedTask;
            }

            _lazyLoadHandlerService.OnPostDBLoad();

            _logger.LogInformation("[KeyHunter] Successfully initialized!");
        return Task.CompletedTask;
        }
    }
}
