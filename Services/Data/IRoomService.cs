﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HotelReservationsManager.Model;

namespace Services.Data
{
    public interface IRoomService
    {
        public Task AddRoom(Room room);
        public Task<IEnumerable<T>> GetAllByCapacity<T>(int capacity);
        public Task<IEnumerable<T>> GetAllByType<T>(Room.RoomType type);
        public Task<IEnumerable<T>> GetAllFreeRoomsAtPresent<T>();
        public Task<int> CountFreeRoomsAtPresent();
        public Task<IEnumerable<T>> GetAll<T>();

        public Task<IEnumerable<T>> GetSearchResults<T>(bool availableOnly = false,
            Room.RoomType[] types = null,
            int? minCapacity = null);

        public Task DeleteRoom(string id);
        public Task UpdateRoom(string id, Room room);
        public Task<T> GetRoom<T>(string id);
        public int CountAllRooms();
        public Task<double> GetMinPrice();
        public Task<double> GetMaxPrice();
        public Task<bool> IsRoomNumberFree(int number, string idRoom = null);
        public Task<int> GetMaxCapacity();
    }
}