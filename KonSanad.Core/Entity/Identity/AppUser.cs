using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity.Identity
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;

        public int NationalId { get; set; }




    }
}
