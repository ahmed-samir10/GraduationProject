using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class HelpOrderDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;

        public int? BeneficiaryId { get; set; }
    }
}
