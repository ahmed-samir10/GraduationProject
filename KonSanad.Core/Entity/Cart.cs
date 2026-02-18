using KonSanad.Core.Entity.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity
{
   
    public class Cart : BaseEntity<int>
    {
        public int UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}
