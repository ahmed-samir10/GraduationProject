using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class PendingRequestReportItem
    {
        public string UserName { get; set; } = null!;

        public string Status { get; set; } = null!;

        public int Count { get; set; }
    }
}
