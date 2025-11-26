
using System.Collections.Generic;

namespace KonSanad.Core.Dtos
{
    public class BeneficiaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Pssword { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int NationalId { get; set; }

        // Related collections
        public IList<HelpOrderDto> HelpOrders { get; set; } = new List<HelpOrderDto>();

        public IList<SupplyMovementDto> SupplyMovements { get; set; } = new List<SupplyMovementDto>();
    }
}
