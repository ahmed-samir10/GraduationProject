
namespace KonSanad.Services.Services
{
    
    public class DonationService : IDonationService
    {
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DonationService( IUnitOfWork unitOfWork , IMapper mapper)
        {
     
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper;
        }

        // delegate creation to the existing order service (keeps single place for creation logic)



        public async Task<IEnumerable<OrderItemDto>> GetAllDonationItemsAsync(int pagenum , int pagesize)
        {
            // Get all OrderItems
            var orderItems = await _unitOfWork.Repository<OrderItem, int>().GetAllAsync();

            // Fill DTO
            var result = orderItems
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    CustomSupplyName = i.CustomSupplyName,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Category = i.Category,
                    ImageUrl = i.ImageUrl,
                    //DonationOrderId = i.DonationOrderId,
                    //SupplyId = i.SupplyId,
                    CreatedAt = i.CreatedAt,
                    // UpdatedAt = i.UpdatedAt,
                    // Status = i.Status
                })
                 .Skip((pagenum - 1) * pagesize)
                 .Take(pagesize)
                .ToList();
            var dtos = _mapper.Map<List<OrderItemDto>>(result);
            return result;
        }
        public async Task<OrderItemDto?> GetOrderItemByIdAsync(int id)
        {
            var item = await _unitOfWork.Repository<OrderItem, int>().GetByIdAsync(id);


            if (item == null) return null;

            var dtos = _mapper.Map<OrderItemDto>(item);
            return dtos;
        }






    }
}
    