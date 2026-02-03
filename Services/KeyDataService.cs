using System.Collections.Generic;
using System.Linq;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Common;

namespace KeyHunter.Services
{
    [Injectable(InjectionType.Singleton)]
    public class KeyDataService
    {
        private readonly DatabaseService _databaseService;
        private Dictionary<string, KeyInfo> _keyCache = new();
        private bool _isInitialized = false;

        public KeyDataService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            var items = _databaseService.GetItems();
            foreach (var kvp in items)
            {
                var item = kvp.Value;
                if (IsKey(item) || IsKeycard(item))
                {
                    var rarity = GetRarityFromItem(item);
                    var homeMap = DetermineHomeMap(item);
                    var itemId = kvp.Key.ToString();
                    
                    _keyCache[itemId] = new KeyInfo
                    {
                        Id = itemId,
                        Name = item.Name ?? "Unknown",
                        IsKeycard = IsKeycard(item),
                        Rarity = rarity,
                        HomeMap = homeMap
                    };
                }
            }

            _isInitialized = true;
        }

        public KeyInfo? GetKeyInfo(string itemId)
        {
            _keyCache.TryGetValue(itemId, out var info);
            return info;
        }

        public bool IsKeyOrKeycard(string itemId)
        {
            return _keyCache.ContainsKey(itemId);
        }

        public IEnumerable<string> GetAllKeyIds()
        {
            return _keyCache.Keys;
        }

        private bool IsKey(TemplateItem item)
        {
            return item.Parent == "543be5e94bdc2df1348b4568" ||
                   item.Parent == "5c99f98d86f7745c314214b3";
        }

        private bool IsKeycard(TemplateItem item)
        {
            return item.Parent == "5c99f98d86f7745c314214b3";
        }

        private RarityTier GetRarityFromItem(TemplateItem item)
        {
            var rarityPvE = item.Properties?.RarityPvE?.ToString()?.ToLower() ?? "common";
            
            return rarityPvE switch
            {
                "not_exist" => RarityTier.Common,
                "common" => RarityTier.Common,
                "rare" => RarityTier.Rare,
                "superrare" => RarityTier.SuperRare,
                _ => RarityTier.Common
            };
        }

        private string DetermineHomeMap(TemplateItem item)
        {
            var name = item.Name?.ToLower() ?? "";

            if (name.Contains("dorm") || name.Contains("gas station") || name.Contains("customs") || name.Contains("trailer"))
                return "bigmap";
            if (name.Contains("rb-") || name.Contains("reserve") || name.Contains("barrack") || name.Contains("kpp"))
                return "rezervbase";
            if (name.Contains("sanatorium") || name.Contains("resort") || name.Contains("cottage") || name.Contains("weather") || name.Contains("coast") || name.Contains("health resort"))
                return "shoreline";
            if (name.Contains("kiba") || name.Contains("goshan") || name.Contains("oli") || name.Contains("idea") || name.Contains("ultra") || name.Contains("interchange"))
                return "interchange";
            if (name.Contains("labs") || name.Contains("laboratory") || name.Contains("terragroup") || name.Contains("keycard"))
                return "laboratory";
            if (name.Contains("lighthouse") || name.Contains("rogue") || name.Contains("water treatment") || name.Contains("merin"))
                return "lighthouse";
            if (name.Contains("concordia") || name.Contains("pinewood") || name.Contains("city") || name.Contains("tarbank") || name.Contains("streets") || name.Contains("zmeisky") || name.Contains("primorsky"))
                return "tarkovstreets";
            if (name.Contains("factory") && !name.Contains("abandoned"))
                return "factory4_day";
            if (name.Contains("zb-014") || name.Contains("shturman") || name.Contains("woods"))
                return "woods";
            if (name.Contains("ground zero") || name.Contains("unity credit"))
                return "sandbox";

            return "generic";
        }
    }

    public class KeyInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsKeycard { get; set; }
        public RarityTier Rarity { get; set; }
        public string HomeMap { get; set; } = "generic";
    }

    public enum RarityTier
    {
        Common,
        Rare,
        SuperRare
    }
}
