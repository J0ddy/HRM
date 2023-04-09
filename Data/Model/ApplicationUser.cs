using Microsoft.AspNetCore.Identity;
using System;

namespace Data.Model
{
    public class ApplicationUser : IdentityUser<string>
    {
        public ApplicationUser() => Id = Guid.NewGuid().ToString();

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdult { get; set; }

        public virtual EmployeeData EmployeeData { get; set; }
    }
}
