using Data.Model;
using Data;
using System.Threading.Tasks;

namespace Services.Data
{
    public class SettingService : ISettingService
    {
        private readonly ApplicationDbContext _dbContext;

        public SettingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(string key, string value, string type)
        {
            await _dbContext.Settings.AddAsync(
                new Setting
                {
                    Key = key,
                    Value = value,
                    Type = type,
                });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<(string Value, string Type)> GetAsync(string key)
        {
            var res = await _dbContext.Settings.FindAsync(key);

            return res == null ? (null, null) : (res.Value, res.Type);
        }

        public async Task UpdateAsync(string key, string value, string type)
        {
            var res = await _dbContext.Settings.FindAsync(key);

            if (res != null)
            {
                res.Value = value;
                res.Type = type;
                _dbContext.Settings.Update(res);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
