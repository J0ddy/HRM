using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Web.Common;
using System.Threading.Tasks;
using Web.Models.Settings;
using Services.Data;

namespace Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class SettingsController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISettingService _settingService;

        public SettingsController(IMemoryCache memoryCache, ISettingService settingService)
        {
            _memoryCache = memoryCache;
            _settingService = settingService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SettingsIndexModel
            {
                AllInclusivePrice = await _memoryCache.GetAllInclusivePrice(_settingService),
                BreakfastPrice = await _memoryCache.GetBreakfastPrice(_settingService),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SettingsIndexModel model)
        {
            await _settingService.UpdateAsync("AllInclusivePrice", 
                                             model.AllInclusivePrice.ToString(),
                                             typeof(double).ToString());
            await _settingService.UpdateAsync("BreakfastPrice", 
                                             model.BreakfastPrice.ToString(),
                                             typeof(double).ToString());

            _memoryCache.ClearPriceCache();

            TempData["Success"] = true;

            return RedirectToAction(nameof(Index));
        }

    }
}
