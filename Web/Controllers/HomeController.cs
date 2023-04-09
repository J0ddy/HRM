using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Services.Common;
using Services.Data;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Web.Common;
using Web.Models;
using Web.Models.Rooms;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly ISettingService _settingService;
        private readonly IReservationService _reservationService;
        private readonly IRoomService _roomService;

        public HomeController(IRoomService roomService,
                              IMemoryCache cache,
                              ISettingService settingService,
                              IReservationService reservationService)
        {
            _roomService = roomService;
            _cache = cache;
            _settingService = settingService;
            _reservationService = reservationService;
        }

        public async Task<IActionResult> Index(int id = 1, int pageSize = 10)
        {
            if (pageSize <= 0) pageSize = 10;
            var pageCount = (int)Math.Ceiling((double)_roomService.CountAllRooms() / pageSize);
            if (id > pageCount || id < 1) id = 1;

                HomePageViewModel viewModel = new()
            {
                PagesCount = pageCount,
                CurrentPage = id,
                Rooms = await _roomService.GetAllFreeRoomsAtPresent<RoomViewModel>().GetPageItems<RoomViewModel>(id, pageSize),
                Controller = "Home",
                Action = nameof(Index),
                BreakfastPrice = await _cache.GetBreakfastPrice(_settingService),
                AllInclusivePrice = await _cache.GetAllInclusivePrice(_settingService),
                TotalReservationsMade = await _reservationService.CountAllReservations(),
                MinPrice = await _roomService.GetMinPrice(),
                MaxPrice = await _roomService.GetMaxPrice(),
            };

            return View(viewModel);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
