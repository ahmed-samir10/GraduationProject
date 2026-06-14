using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class ReportSubmittedDto
    {
        public IFormFile? File { get; set; }
        public int MissionId { get; set; }
        // optional: change mission status or additional notes
        public string? Notes { get; set; }
    }

}
