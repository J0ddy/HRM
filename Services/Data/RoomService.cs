using Data;
using HotelReservationsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using static HotelReservationsManager.Model.Room;
using System.Threading.Tasks;
using Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Services.Data
{
    public class RoomServices : IRoomService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public RoomServices(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddRoom(Room room)
        {
            await _applicationDbContext.Rooms.AddAsync(room);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllByCapacity<T>(int capacity) => await _applicationDbContext.Rooms.Where(x => x.Capacity == capacity).ProjectTo<T>().ToListAsync();

        public async Task<IEnumerable<T>> GetAllByType<T>(RoomType type) => await _applicationDbContext.Rooms.Where(x => x.Type == type).ProjectTo<T>().ToListAsync();

        public async Task<IEnumerable<T>> GetAllFreeRoomsAtPresent<T>() => await _applicationDbContext.Rooms.
                                 Where(x => !x.Reservations.Any(r => r.AccommodationDate <= DateTime.Today &&
                                                                     r.ReleaseDate > DateTime.Today)).
                                 OrderBy(x => x.Number).
                                 ProjectTo<T>().
                                 ToListAsync();

        public async Task<int> CountFreeRoomsAtPresent() => await _applicationDbContext.Rooms.
                                 Where(x => !x.Reservations.Any(r => r.AccommodationDate <= DateTime.Today
                                                                  && r.ReleaseDate > DateTime.Today)).
                                 CountAsync();

        public async Task<IEnumerable<T>> GetAll<T>() => await _applicationDbContext.Rooms.AsQueryable().ProjectTo<T>().ToListAsync();

        public async Task<IEnumerable<T>> GetSearchResults<T>(bool availableOnly = false,
                                                              RoomType[] types = null,
                                                              int? minCapacity = null)
        {
            IQueryable<Room> result = _applicationDbContext.Rooms;

            if (availableOnly)
            {
                result = result.Where(x => !x.Reservations.Any(r => r.AccommodationDate <= DateTime.Today
                                                                 && r.ReleaseDate > DateTime.Today));
            }

            if (types != null && (types?.Count() ?? 0) > 0)
            {
                result = result.Where(x => types.Contains(x.Type));
            }

            if (minCapacity != null && minCapacity > 0)
            {
                result = result.Where(x => x.Capacity > minCapacity);
            }

            return await result.OrderBy(x => x.Number).ProjectTo<T>().ToListAsync();
        }

        public async Task DeleteRoom(string id)
        {
            var room = await _applicationDbContext.Rooms.Include(x => x.Reservations).FirstOrDefaultAsync(x => x.Id == id);
            if (room != null)
            {
                //Feature: Send an email for room cancel forced
                if (room.Reservations != null)
                {
                    _applicationDbContext.Reservations.RemoveRange(room.Reservations);
                    await _applicationDbContext.SaveChangesAsync();
                }

                _applicationDbContext.Rooms.Remove(room);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateRoom(string id, Room room)
        {
            room.Id = id;
            var roomToChange = await _applicationDbContext.Rooms.AsNoTracking().Include(x => x.Reservations).FirstOrDefaultAsync(x => x.Id == id);
            if (roomToChange != null)
            {
                if (roomToChange.Reservations != null)
                {
                    foreach (var reservation in roomToChange.Reservations)
                    {
                        if (roomToChange.Capacity < room.Capacity)
                        {
                            //Feature: Send an email for change & not cancel till confirmation
                            _applicationDbContext.Reservations.Remove(reservation);
                        }
                    }
                }

                _applicationDbContext.Rooms.Update(room);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task<T> GetRoom<T>(string id) => await _applicationDbContext.Rooms.Where(x => x.Id == id).ProjectTo<T>().FirstOrDefaultAsync();

        ///<returns>The count of all rooms in the database</returns>
        public int CountAllRooms() => _applicationDbContext.Rooms.Count();

        public async Task<double> GetMinPrice() => await _applicationDbContext.Rooms.OrderBy(x => x.AdultPrice).Select(X => X.AdultPrice).FirstOrDefaultAsync();

        public async Task<double> GetMaxPrice() => await _applicationDbContext.Rooms.
                                      OrderByDescending(x => x.AdultPrice).
                                      Select(X => X.AdultPrice).
                                      FirstOrDefaultAsync();

        public async Task<bool> IsRoomNumberFree(int number, string idRoom = null) => !await _applicationDbContext.Rooms.AsNoTracking().Where(x => x.Id != idRoom).AnyAsync(x => x.Number == number);

        public async Task<int> GetMaxCapacity() => await _applicationDbContext.Rooms.AsNoTracking().
                                       OrderByDescending(x => x.Capacity).
                                       Select(x => x.Capacity).
                                       FirstOrDefaultAsync();
    }
}
