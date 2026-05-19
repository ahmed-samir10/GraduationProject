using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class DonorReportItem
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public decimal Total { get; set; }

        public int Count { get; set; }
    }
}
