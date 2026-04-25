using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    public class UpdateSupplyDto
    {

        public int? StockQuantity { get; set; }
        public int? MinimumLevel { get; set; }
    }
}
