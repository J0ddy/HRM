using Data.Model;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services.Mapping;
using Microsoft.EntityFrameworkCore;
using Services.Common;

namespace Services.Data
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public UserService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(EmployeeData user)
        {
            await _applicationDbContext.EmployeeData.AddAsync(user);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<T> GetEmployeeAsync<T>(string id) => await _applicationDbContext.EmployeeData.Where(x => x.UserId == id).ProjectTo<T>().FirstOrDefaultAsync();

        public async Task<T> GetUserAsync<T>(string id) => await _applicationDbContext.Users.Where(x => x.Id == id).ProjectTo<T>().FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetAllEmployees<T>() => await _applicationDbContext.EmployeeData.AsQueryable().ProjectTo<T>().ToListAsync();

        public async Task<IEnumerable<T>> GetAllUsers<T>() => await _applicationDbContext.Users.AsQueryable().ProjectTo<T>().ToListAsync();

        public async Task<IEnumerable<string>> GetAllBySearch(string searchString) => await _applicationDbContext.Users.Where(x => x.Email.ToUpper().Contains(searchString.ToUpper()) ||
                                                                                                                                x.FirstName.ToUpper().Contains(searchString.ToUpper()) ||
                                                                                                                                x.LastName.ToUpper().Contains(searchString.ToUpper()) ||
                                                                                                                                x.UserName.ToUpper().Contains(searchString.ToUpper())).
                                             Select(x => x.Id).ToListAsync();

        public async Task<IEnumerable<string>> GetAllBySecondName(string searchString) => await _applicationDbContext.EmployeeData.Where(x => x.SecondName.ToUpper().Contains(searchString.ToUpper())).
                                                    Select(x => x.UserId).ToListAsync();

        public async Task<IEnumerable<T>> GetEmployeePageItems<T>(int page, int usersOnPage) => await GetAllEmployees<T>().GetPageItems(page, usersOnPage);

        public async Task<IEnumerable<T>> GetUserPageItems<T>(int page, int usersOnPage) => await GetAllUsers<T>().GetPageItems(page, usersOnPage);

        private async Task<List<string>> GetSearchResults(string searchString)
        {
            var result = new List<string>();

            var emailResults = await GetAllBySecondName(searchString);
            var familyNameResults = await GetAllBySearch(searchString);

            if (emailResults != null)
            {
                result.AddRange(emailResults);
            }

            if (familyNameResults != null)
            {
                result.AddRange(familyNameResults);
            }

            return result.Distinct().ToList();
        }

        public async Task<IEnumerable<T>> GetEmployeesSearchResults<T>(string searchString)
        {
            var result = await GetSearchResults(searchString);

            return await _applicationDbContext.EmployeeData.Where(x => result.Contains(x.UserId)).ProjectTo<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetUsersSearchResults<T>(string searchString)
        {
            var result = await GetSearchResults(searchString);

            return await _applicationDbContext.Users.Where(x => result.Contains(x.Id)).ProjectTo<T>().ToListAsync();
        }

        public async Task UpdateAsync(EmployeeData user)
        {
            var userInContext = await _applicationDbContext.EmployeeData.FindAsync(user.UserId);

            if (userInContext != null)
            {
                _applicationDbContext.Entry(userInContext).CurrentValues.SetValues(user);
                await _applicationDbContext.SaveChangesAsync();
            }
            else
            {
                await AddAsync(user);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var userInContext = await _applicationDbContext.EmployeeData.FindAsync(id);

            if (userInContext != null)
            {
                userInContext.DateOfResignation = DateTime.UtcNow;
                userInContext.IsActive = false;
                _applicationDbContext.EmployeeData.Update(userInContext);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public int CountAllEmployees() => _applicationDbContext.EmployeeData.Count();

        public int CountAllUsers() => _applicationDbContext.Users.Count();

        public bool IsAlreadyAdded(string email) => _applicationDbContext.Users.Any(x => x.Email.ToLower().Equals(email.ToLower()));

        public async Task<ClientData> CreateClient(string email, string name, bool adult)
        {
            var client = new ClientData
            {
                Email = email,
                FullName = name,
                IsAdult = adult,
            };

            _applicationDbContext.ClientData.Add(client);
            await _applicationDbContext.SaveChangesAsync();

            return client;
        }

        public async Task<ClientData> UpdateClient(string id, string email, string name, bool adult)
        {
            var client = new ClientData
            {
                Id = id,
                Email = email,
                FullName = name,
                IsAdult = adult,
            };

            var clientInContext = await _applicationDbContext.ClientData.FindAsync(id);
            _applicationDbContext.Entry<ClientData>(clientInContext).CurrentValues.SetValues(client);
            await _applicationDbContext.SaveChangesAsync();

            return client;
        }

        public async Task DeleteClient(string id)
        {
            var client = await _applicationDbContext.ClientData.FindAsync(id);

            if (client != null)
            {
                _applicationDbContext.ClientData.Remove(client);
                await _applicationDbContext.SaveChangesAsync();
            }
        }
    }
}
