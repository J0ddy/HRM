using System;
using Microsoft.AspNetCore.Identity;

namespace Data.Model
{
    public class ApplicationRole : IdentityRole<string>
    {
        public ApplicationRole() => Id = Guid.NewGuid().ToString();
    }
}
