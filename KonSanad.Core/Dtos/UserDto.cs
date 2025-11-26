using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class UserDto
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Token { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
