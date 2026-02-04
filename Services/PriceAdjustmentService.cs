using System.Linq;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using Microsoft.Extensions.Logging;
using KeyHunter.Config;

namespace KeyHunter.Services
{
    [Injectable(InjectionType.Singleton)]
    public class PriceAdjustmentService
    {
        private readonly DatabaseService _databaseService;
        private readonly ConfigService _configService;
        private readonly ILogger<PriceAdjustmentService> _logger;
        private bool _pricesAdjusted = false;

        public PriceAdjustmentService(
            DatabaseService databaseService,
            ConfigService configService,
            ILogger<PriceAdjustmentService> logger)
        {
            _databaseService = databaseService;
            _configService = configService;
            _logger = logger;
        }

        public void AdjustPrices()
        {
            var config = _configService.Load();
            if (!config.EnablePriceAdjustment)
            {
                if (config.DebugLogging)
                    _logger.LogDebug("[KeyHunter] Price adjustment disabled");
                return;
            }

            if (_pricesAdjusted)
            {
                if (config.DebugLogging)
                    _logger.LogDebug("[KeyHunter] Prices already adjusted");
                return;
            }

            try
            {
                var items = _databaseService.GetItems();
                var handbook = _databaseService.GetHandbook();

                int adjustedCount = 0;

                foreach (var kvp in items)
                {
                    var item = kvp.Value;
                    if (IsKeyOrKeycard(item))
                    {
                        AdjustFleaPrice(item, config.FleaPriceMultiplier);
                        AdjustTraderPrices(kvp.Key.ToString(), config.TraderPriceMultiplier, handbook);
                        adjustedCount++;
                    }
                }

                _pricesAdjusted = true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "[KeyHunter] Error adjusting prices");
            }
        }

        private bool IsKeyOrKeycard(TemplateItem item)
        {
            return item.Parent == "543be5e94bdc2df1348b4568" ||
                   item.Parent == "5c99f98d86f7745c314214b3";
        }

        private void AdjustFleaPrice(TemplateItem item, float multiplier)
        {
            try
            {
                if (item.Properties?.CreditsPrice.HasValue == true)
                {
                    double originalPrice = item.Properties.CreditsPrice.Value;
                    double newPrice = originalPrice * multiplier;
                    if (newPrice < 100) newPrice = 100;
                    
                    item.Properties.CreditsPrice = newPrice;
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private void AdjustTraderPrices(string itemId, float multiplier, HandbookBase handbook)
        {
            try
            {
                var handbookItem = handbook?.Items?.FirstOrDefault(h => h.Id == itemId);
                if (handbookItem != null && handbookItem.Price.HasValue)
                {
                    int originalPrice = (int)handbookItem.Price.Value;
                    int newPrice = (int)(originalPrice * multiplier);
                    if (newPrice < 100) newPrice = 100;
                    
                    handbookItem.Price = newPrice;
                }
            }
            catch
            {
                // Silent fail
            }
        }
    }
}
