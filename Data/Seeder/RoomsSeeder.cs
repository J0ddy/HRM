using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using HotelReservationsManager.Model;

namespace Data.Seeders
{
    public class RoomsSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, ILogger logger)
        {
            if (dbContext.Rooms.Any()) return;
            
            await dbContext.Rooms.AddRangeAsync(new Room[]
           {
                new Room
                {
                    Id="ExampleRoom1",
                    AdultPrice=50,
                    ChildrenPrice=30,
                    Capacity=5,
                    Number=105,
                    Type=Room.RoomType.Studio,
                    ImageUrl="https://i.ibb.co/S6W1qvR/bedroom.jpg",
                },
                new Room
                {
                    Id="ExampleRoom2",
                    AdultPrice=30,
                    ChildrenPrice=10,
                    Capacity=3,
                    Number=205,
                    Type=Room.RoomType.DoubleBed,
                    ImageUrl="https://i.ibb.co/c1Dkh6n/bedroom2.jpg",
                }
           });

            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Finished executing {nameof(RoomsSeeder)}");
        }
    }
}
