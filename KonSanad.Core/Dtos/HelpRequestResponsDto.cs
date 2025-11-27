using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    // Response returned after creating a help request
    public class HelpRequestResponsDto
    {
        public int HelpOrderId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
