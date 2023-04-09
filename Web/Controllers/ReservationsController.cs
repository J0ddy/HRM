using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Services.Data;
using Services.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Model;
using Services.Common;
using Web.Common;
using Web.Models.Reservations;
using Web.Models.Rooms;

namespace Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService reservationService;
        private readonly IUserService userService;
        private readonly IRoomService roomService;
        private readonly ISettingService settingService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMemoryCache memoryCache;

        public ReservationsController(IReservationService reservationService,
                                      IUserService userService,
                                      IRoomService roomService,
                                      ISettingService settingService,
                                      UserManager<ApplicationUser> userManager,
                                      IMemoryCache memoryCache)
        {
            this.reservationService = reservationService;
            this.userService = userService;
            this.roomService = roomService;
            this.settingService = settingService;
            this.userManager = userManager;
            this.memoryCache = memoryCache;
        }

        public async Task<IActionResult> Index(int id = 1, int pageSize = 10)
        {
            var user = await userManager.GetUserAsync(User);
            var reservations = await reservationService.GetReservationsForUser<ReservationViewModel>(user.Id);

            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var pageCount = (int)Math.Ceiling((double)reservations.Count() / pageSize);
            if (id > pageCount || id < 1) id = 1;

                var viewModel = new ReservationsIndexViewModel
            {
                CurrentPage = id,
                PagesCount = pageCount,
                Reservations = reservations.GetPageItems(id, pageSize),
                Controller = "Reservations",
                Action = nameof(Index),
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await userManager.GetUserAsync(User);
            var viewModel = await reservationService.GetReservation<ReservationViewModel>(id);
            if (viewModel == null || 
                !(user.Id == viewModel.UserId || User.IsInRole("Admin") || User.IsInRole("Employee")))
            {
                return NotFound();
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Create(string id)
        {
            var room = await roomService.GetRoom<RoomViewModel>(id);
            if (room == null || (room?.IsTaken ?? true))
            {
                return NotFound();
            }

            var inputModel = await FillRoomData(new ReservationInputModel(), room);

            return View(inputModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string id, ReservationInputModel inputModel)
        {
            var room = await roomService.GetRoom<RoomViewModel>(id);
            if (room == null || (room?.IsTaken ?? true))
            {
                return NotFound();
            }

            var roomIsEmpty = await reservationService.AreDatesAcceptable(room.Id, 
                                                                          inputModel.AccommodationDate, 
                                                                          inputModel.ReleaseDate);

            if (!roomIsEmpty)
            {
                ModelState.AddModelError(nameof(inputModel.AccommodationDate), 
                                              "Room is already reserved at that time");
            }

            if (!ModelState.IsValid)
            {
                return View(await FillRoomData(inputModel, room));
            }

            var clients = new List<ClientData>();
            foreach (var client in inputModel.Clients)
            {
                clients.Add(await userService.CreateClient(client.Email, client.FullName, client.IsAdult));
            }

            var user = await userManager.GetUserAsync(User);

            var reservation = await reservationService.AddReservation(
                room.Id,
                inputModel.AccommodationDate,
                inputModel.ReleaseDate,
                inputModel.AllInclusive,
                inputModel.Breakfast,
                clients,
                user);

            return RedirectToAction(nameof(Details), new { id = reservation.Id });
        }

        public async Task<IActionResult> Update(string id)
        {
            var user = await userManager.GetUserAsync(User);

            var reservation = await reservationService.GetReservation<ReservationInputModel>(id);
            if (reservation == null || !(user.Id == reservation.UserId || User.IsInRole("Admin")))
            {
                return NotFound();
            }

            var room = await roomService.GetRoom<RoomViewModel>(reservation.RoomId);
            reservation = await FillRoomData(reservation, room);

            if (reservation.Reservations?.Any() ?? false)
            {
                reservation.Reservations = reservation.Reservations.
                                                      Where(x => !(x.AccommodationDate == reservation.AccommodationDate
                                                                   && x.ReleaseDate == reservation.ReleaseDate));
            }

            return View(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, ReservationInputModel inputModel)
        {
            var user = await userManager.GetUserAsync(User);
            var reservation = await reservationService.GetReservation<ReservationInputModel>(id);
            if (reservation == null || 
                !(user.Id == reservation.UserId || User.IsInRole("Admin")) || reservation.ReleaseDate < DateTime.Today)
            {
                return NotFound();
            }

            var room = await roomService.GetRoom<RoomViewModel>(reservation.RoomId);
            reservation = await FillRoomData(reservation, room);

            if (reservation.Reservations?.Any() ?? false)
            {
                reservation.Reservations = reservation.Reservations.
                                                       Where(x => !(x.AccommodationDate == reservation.AccommodationDate 
                                                                    && x.ReleaseDate == reservation.ReleaseDate));
            }

            var roomIsEmpty = await reservationService.AreDatesAcceptable(room.Id,
                                                                          inputModel.AccommodationDate, 
                                                                          inputModel.
                                                                          ReleaseDate,id);

            if (!roomIsEmpty)
            {
                ModelState.AddModelError(nameof(inputModel.AccommodationDate), 
                                              "Room is already reserved at that time");
            }

            if (!ModelState.IsValid)
            {
                inputModel = await FillRoomData(inputModel, room);
                return View(inputModel);
            }

            var cls = inputModel.Clients.Select(x => new ClientData
            {
                IsAdult = x.IsAdult,
                Email = x.Email,
                FullName = x.FullName,
                Id = x.Id,
            }).ToList();

            var clients = await reservationService.UpdateClientsForReservation(reservation.Id, cls);

            var success = await reservationService.UpdateReservation(
                reservation.Id,
                inputModel.AccommodationDate,
                inputModel.ReleaseDate,
                inputModel.AllInclusive,
                inputModel.Breakfast,
                clients,
                user);

            if (!success)
            {
                ModelState.AddModelError("", "Error updating reservation");
                inputModel = await FillRoomData(inputModel, room);

                return View(inputModel);
            }

            return RedirectToAction(nameof(Details), new { id = reservation.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var reservation = await reservationService.GetReservation<ReservationInputModel>(id);

            if (reservation == null || 
                !(reservation.UserId == reservation.UserId || User.IsInRole("Admin")) || reservation.ReleaseDate<DateTime.Today)
            {
                NotFound();
            }

            await reservationService.DeleteReservation(id);

            return RedirectToAction(nameof(Index));
        }

        private async Task<ReservationInputModel> FillRoomData(ReservationInputModel inputModel, RoomViewModel room)
        {
            inputModel.RoomId = room.Id;
            inputModel.Reservations = room.Reservations.AsQueryable().ProjectTo<ReservationPeriod>().ToList();
            inputModel.RoomCapacity = room.Capacity;
            inputModel.AllInclusivePrice = await memoryCache.GetAllInclusivePrice(settingService);
            inputModel.RoomAdultPrice = room.AdultPrice;
            inputModel.BreakfastPrice = await memoryCache.GetBreakfastPrice(settingService);
            inputModel.RoomChildrenPrice = room.ChildrenPrice;
            inputModel.RoomType = room.Type;
            inputModel.RoomImageUrl = room.ImageUrl;

            return inputModel;
        }

        [Authorize(Roles ="Admin, Employee")]
        public async Task<IActionResult> All(int id = 1, int pageSize = 10)
        {
            var reservations = await reservationService.GetAll<ReservationViewModel>();

            if (pageSize <= 0) pageSize = 10;
            
            var pageCount = (int)Math.Ceiling((double)reservations.Count() / pageSize);
            if (id > pageCount || id < 1)
            {
                id = 1;
            }

            var viewModel = new ReservationsIndexViewModel
            {
                CurrentPage = id,
                PagesCount = pageCount,
                Reservations = reservations.GetPageItems(id, pageSize),
                Controller = "Reservations",
                Action = nameof(All),
            };

            return View(viewModel);
        }
    }
}