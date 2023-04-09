using System;
using System.Collections.Generic;
using Data.Model;

namespace HotelReservationsManager.Model
{
    public class Reservation
    {
        public Reservation() => Id = Guid.NewGuid().ToString();
        public string Id { get; set; }
        public ApplicationUser User { get; set; }
        public IEnumerable<ClientData> Clients { get; set; }
        public DateTime AccommodationDate { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool IncludesBreakfast { get; set; }
        public bool IsAllInclusive { get; set; }
        public double Price { get; set; }
        public virtual Room Room { get; set; }
    }
}