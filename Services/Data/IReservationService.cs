﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model;
using HotelReservationsManager.Model;

namespace Services.Data
{
    public interface IReservationService
    {
        public Task<Reservation> AddReservation(string roomId,
                                                DateTime accommodationDate,
                                                DateTime releaseDate,
                                                bool isAllInclusive,
                                                bool includesBreakfast,
                                                IEnumerable<ClientData> clients,
                                                ApplicationUser user);
        public Task<bool> UpdateReservation(string id,
                                            DateTime accommodationDate,
                                            DateTime releaseDate,
                                            bool allInclusive,
                                            bool breakfast,
                                            IEnumerable<ClientData> clients,
                                            ApplicationUser user);
        public Task<bool> DeleteReservation(string id);
        public Task<T> GetReservation<T>(string id);
        public Task<IEnumerable<T>> GetReservationsForUser<T>(string userId);
        public Task<IEnumerable<T>> GetForUserOnPage<T>(string userId, int page, int elementsOnPage);
        public Task<IEnumerable<ClientData>> UpdateClientsForReservation(string reservationId, IEnumerable<ClientData> clients);
        public Task<IEnumerable<T>> GetAll<T>();
        public Task<int> CountAllReservations();
        public Task<bool> AreDatesAcceptable(string roomId,
                                             DateTime accommodationDate,
                                             DateTime releaseDate,
                                             string idReservation = null);
    }
}
