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
        public string FullName
        {
            get
            {
                var first = FirstName?.Trim();
                var last = LastName?.Trim();

                if (string.IsNullOrEmpty(first) && string.IsNullOrEmpty(last))
                    return string.Empty;

                if (string.IsNullOrEmpty(first))
                    return last!; // first null -> return last only (حسب طلبك)

                if (string.IsNullOrEmpty(last))
                    return first;

                return $"{first} {last}";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    FirstName = null;
                    LastName = null;
                    return;
                }

                var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1)
                {
                    // لو اسم واحد فقط — نخزنها في LastName ونجعل FirstName = null (حسب طلبك)
                    FirstName = null;
                    LastName = parts[0];
                }
                else
                {
                    FirstName = parts[0];
                    LastName = string.Join(' ', parts.Skip(1));
                }
            }
        }
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;

        public string NationalId { get; set; }




    }
}
