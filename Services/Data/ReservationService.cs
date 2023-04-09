using Data.Model;
using Data;
using HotelReservationsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Services.Common;
using Services.Mapping;

namespace Services.Data
{
    public class ReservationsService : IReservationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISettingService _settingService;

        public ReservationsService(ApplicationDbContext dbContext, ISettingService settingService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
        }


        public async Task<bool> AreDatesAcceptable(string roomId,
                                                    DateTime accommodationDate,
                                                    DateTime releaseDate,
                                                    string idReservation = null)
        {
            if (accommodationDate >= releaseDate || accommodationDate < DateTime.Today) return false;

            var reservationPeriods = await _dbContext.
                                           Reservations.
                                           AsNoTracking().
                                           Where(x => x.Room.Id == roomId).
                                           Select(x => new Tuple<DateTime, DateTime>
                                                        (x.AccommodationDate, x.ReleaseDate).
                                                        ToValueTuple()).
                                          ToListAsync();

            if (!string.IsNullOrWhiteSpace(idReservation))
            {
                var reservation = await _dbContext.Reservations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == idReservation);
                reservationPeriods = reservationPeriods.Where(x => x.Item1 != reservation.AccommodationDate &&
                                                              x.Item2 != reservation.ReleaseDate).ToList();
            }

            return !reservationPeriods.Any(x =>
                (x.Item1 >= accommodationDate && x.Item1 <= releaseDate) ||
                (x.Item2 > accommodationDate && x.Item2 <= releaseDate) ||
                (x.Item1 >= accommodationDate && x.Item2 <= releaseDate) ||
                (x.Item1 <= accommodationDate && x.Item2 >= releaseDate));
        }


        private async Task<double> CalculatePriceForNight(Room room, IEnumerable<ClientData> clients, bool allInclusive, bool breakfast)
        {
            clients = clients.ToList().Where(x => x.FullName != null);
            var price =
                clients.Count(x => x.IsAdult) * room.AdultPrice +
                clients.Count(x => !x.IsAdult) * room.ChildrenPrice +
                room.AdultPrice;

            if (allInclusive)
            {
                price += double.Parse((await _settingService
                                                .GetAsync($"{nameof(Reservation.IsAllInclusive)}Price")).Value);
            }
            else if (breakfast)
            {
                price += double.Parse((await _settingService
                                               .GetAsync($"{nameof(Reservation.IncludesBreakfast)}Price")).Value);
            }

            return price;
        }


        public async Task<Reservation> AddReservation(string roomId,
                                                      DateTime accommodationDate,
                                                      DateTime releaseDate,
                                                      bool allInclusive,
                                                      bool breakfast,
                                                      IEnumerable<ClientData> clients,
                                                      ApplicationUser user)
        {
            var room = await _dbContext.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return null;
            }

            if (!await AreDatesAcceptable(roomId, accommodationDate, releaseDate))
            {
                return null;
            }

            if (clients.Count() + 1 > room.Capacity)
            {
                return null;
            }

            var price = await CalculatePriceForNight(room, clients, allInclusive, breakfast) * (releaseDate - accommodationDate).TotalDays;

            var reservation = new Reservation
            {
                AccommodationDate = accommodationDate,
                IsAllInclusive = allInclusive,
                IncludesBreakfast = breakfast,
                Price = price,
                Room = (Room) room,
                ReleaseDate = releaseDate,
                Clients = clients,
                User = user,
            };

            _dbContext.Reservations.Add(reservation);
            await _dbContext.SaveChangesAsync();

            return reservation;
        }


        public async Task<bool> UpdateReservation(string id,
                                                  DateTime accommodationDate,
                                                  DateTime releaseDate,
                                                  bool allInclusive,
                                                  bool breakfast,
                                                  IEnumerable<ClientData> clients,
                                                  ApplicationUser user)
        {
            var reservation = await _dbContext.Reservations.AsNoTracking().Include(x => x.User).Include(x => x.Room).FirstOrDefaultAsync(x => x.Id == id);

            var room = await _dbContext.Rooms.FirstOrDefaultAsync(x => x.Reservations.Any(y => y.Id == id));

            var areDateAcceptable = await AreDatesAcceptable(room.Id, accommodationDate, releaseDate, id);
            var isCapacityInRange = clients.Count() + 1 <= room.Capacity;
            var isUserAuthorizedToUpdate = reservation.User.Id == user.Id ||
                                             _dbContext.UserRoles.Any(x => x.UserId == user.Id &&
                                              x.RoleId != _dbContext.Roles.First(a => a.Name == "User").Id);

            if (!isUserAuthorizedToUpdate || !isCapacityInRange || !areDateAcceptable)
            {
                return false;
            }

            var price = await CalculatePriceForNight(room, clients, allInclusive, breakfast) * (releaseDate - accommodationDate).TotalDays;

            var newReservation = new Reservation
            {
                Id = id,
                AccommodationDate = accommodationDate,
                IsAllInclusive = allInclusive,
                IncludesBreakfast = breakfast,
                Price = price,
                ReleaseDate = releaseDate,
                Room = room,
                Clients = clients,
                User = user
            };

            _dbContext.Reservations.Update(newReservation);
            await _dbContext.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteReservation(string id)
        {
            var reservation = await _dbContext.Reservations.FindAsync(id);

            if (reservation != null)
            {
                _dbContext.ClientData.RemoveRange(_dbContext.ClientData
                                                     .Where(x => x.Reservation.Id == reservation.Id));

                _dbContext.Reservations.Remove(reservation);
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }


        public async Task<T> GetReservation<T>(string id) => await _dbContext.Reservations.AsNoTracking().Where(x => x.Id == id).ProjectTo<T>().FirstOrDefaultAsync();


        public async Task<IEnumerable<T>> GetReservationsForUser<T>(string userId) => await _dbContext.Reservations.AsNoTracking()
                                                    .Where(x => x.User.Id == userId)
                                                    .OrderByDescending(x => x.AccommodationDate)
                                                    .ProjectTo<T>().ToListAsync();


        public async Task<IEnumerable<T>> GetForUserOnPage<T>(string userId, int page, int elementsOnPage) => 
            await GetReservationsForUser<T>(userId).GetPageItems(page, elementsOnPage);


        public async Task<IEnumerable<ClientData>> UpdateClientsForReservation(string reservationId,
                                                                               IEnumerable<ClientData> clients)
        {
            var reservation = await _dbContext.Reservations.AsNoTracking()
                                                          .Include(x => x.Room)
                                                          .FirstOrDefaultAsync(x => x.Id == reservationId);
            var initialClients = await _dbContext.ClientData.Where(x => x.Reservation.Id == reservationId)
                                                           .ToListAsync();


            var deletedClients = initialClients.Where(x => !clients.Select(u => u.Id).Contains(x.Id)).ToList();

            if (deletedClients?.Any() ?? false)
            {
                _dbContext.ClientData.RemoveRange(deletedClients);
            }

            var newClients = clients.Where(x => !initialClients.Select(u => u.Id)
                                                               .Contains(x.Id))
                                                               .ToList();

            if (newClients?.Any() ?? false)
            {
                foreach (var cl in newClients)
                {
                    cl.ReservationId = reservation.Id;
                    if (string.IsNullOrWhiteSpace(cl.Id))
                    {
                        cl.Id = Guid.NewGuid().ToString();
                    }
                }

                _dbContext.ClientData.AddRange(newClients);
            }

            var clientsToUpdate = clients.Where(x => !newClients.Select(u => u.Id).Contains(x.Id) && x.Id != null).ToList();

            if (clientsToUpdate?.Any() ?? false)
            {
                foreach (var cl in newClients)
                {
                    cl.ReservationId = reservation.Id;
                }
                _dbContext.ClientData.UpdateRange(clientsToUpdate);
            }

            await _dbContext.SaveChangesAsync();

            return clients;
        }

        public async Task<IEnumerable<T>> GetAll<T>() => await _dbContext.Reservations.AsNoTracking().OrderBy(x => x.ReleaseDate).ProjectTo<T>().ToListAsync();

        public async Task<int> CountAllReservations() => await _dbContext.Reservations.AsNoTracking().CountAsync();
    }
}
