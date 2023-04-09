using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using HotelReservationsManager.Model;

namespace Data.Seeders
{
    public class ReservationsSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, ILogger logger)
        {
            if (dbContext.Reservations.Any()) return;

                await dbContext.Reservations.AddRangeAsync(new Reservation[]
                {
                    new Reservation
                    {
                        Id="ExampleReservation1",
                        AccommodationDate=DateTime.Today.AddDays(3),
                        ReleaseDate=DateTime.Today.AddDays(7),
                        IsAllInclusive=true,
                        IncludesBreakfast=false,
                        Price=150,
                        Room=dbContext.Rooms.FirstOrDefault(x=>x.Id=="ExampleRoom1"),
                        User=dbContext.Users.FirstOrDefault(x=>x.Id=="Admin"),
                    },
                    new Reservation
                    {
                        Id="ExampleReservation2",
                        AccommodationDate=DateTime.Today.AddDays(9),
                        ReleaseDate=DateTime.Today.AddDays(14),
                        IsAllInclusive=false,
                        IncludesBreakfast=true,
                        Price=120,
                        Room=dbContext.Rooms.FirstOrDefault(x=>x.Id=="ExampleRoom2"),
                        User=dbContext.Users.FirstOrDefault(x=>x.Id=="Admin"),
                    },
                }) ;

            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Finished executing {nameof(ReservationsSeeder)}");
        }
    }
}
