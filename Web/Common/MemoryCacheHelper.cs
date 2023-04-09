using Microsoft.Extensions.Caching.Memory;
using Services.Data;
using System.Threading.Tasks;

namespace Web.Common
{
    public static class MemoryCacheHelper
    {
        public static async Task<double> GetBreakfastPrice(this IMemoryCache cahce, ISettingService settingService)
        {
            return await cahce.GetPrice("BreakfastPrice", settingService);
        }

        public static async Task<double> GetAllInclusivePrice(this IMemoryCache cache, ISettingService settingService)
        {
            return await cache.GetPrice("AllInclusivePrice", settingService);
        }

        public static async Task<double> GetPrice(this IMemoryCache cache, string key, ISettingService settingService)
        {
            if (cache.TryGetValue(key, out double price)) return price;
            price = double.Parse((await settingService.GetAsync(key)).Value);
            cache.Set(key, price);
            return price;
        }

        public static void ClearPriceCache(this IMemoryCache cache)
        {
            cache.Remove("AllInclusivePrice");
            cache.Remove("BreakfastPrice");
        }
    }
}
