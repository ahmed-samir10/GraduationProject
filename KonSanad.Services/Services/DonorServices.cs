using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class DonorServices : IDonorServices
    {
       // private readonly IDonorServices _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DonorServices(
         
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
         
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        // ---------------------------------------------------------------------------
        // 2) Get All Low-Stock Supplies
        // ---------------------------------------------------------------------------
        public async Task<List<SupplyDto>> GetLowStockSuppliesAsync()
        {

            
            var supplies = await _unitOfWork.Repository<Supply, int>()
                .FindAllAsync(s => s.StockQuantity <= s.MinimumLevel);

            return supplies.Select(s => new SupplyDto
            {
                Id = s.Id,
                Name = s.Name,
               Quantity = s.StockQuantity ?? 0,
                Unit = s.Unit
            }).ToList();
        }

        // ---------------------------------------------------------------------------
        // 3) Create Donation (Cash + Items)
        // ---------------------------------------------------------------------------
        public async Task<DonationResponseItemDto> CreateDonationItemAsync(
              CreateDonationItemRequestDto request, int donorId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Create donation order
            var donationOrder = new DonationOrder
            {
                DonorId = donorId,
                CampaignId = null,
                ManagerId = 1,
                CreatedAt = DateTime.Now,
                OrderDate = DateTime.Now,
                DonationCategoryStatus = "معلقة",
                Status = "قيد المراجعة",
            };

            await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
            await _unitOfWork.Complete();

            // ensure upload folder exists
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);




            // Load supply to copy its Name and Unit into OrderItem
            var supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(request.SupplyId);



            // Handle single donated object (CreateDonationItemRequestDto represents one item)
            string? imageUrl = null;
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }

                imageUrl = "/uploads/" + fileName;
            }

            var donationItem = new OrderItem
            {
                DonationOrderId = donationOrder.Id,
                SupplyId = request.SupplyId,
                Quantity = request.Quantity,
                ImageUrl = imageUrl,

                // copy supply values into order item
                CustomSupplyName = supply?.Name,
                Unit = supply?.Unit ?? string.Empty,

                Status = string.IsNullOrWhiteSpace(request.Status) ? "قيد المراجعة" : request.Status,
                Description = request.Description
            };

            await _unitOfWork.Repository<OrderItem, int>().AddAsync(donationItem);

            // Update stock
            supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(request.SupplyId);
            if (supply != null)
            {
                supply.StockQuantity = supply.StockQuantity + request.Quantity;
                _unitOfWork.Repository<Supply, int>().Update(supply);
            }

            await _unitOfWork.Complete();

            return new DonationResponseItemDto
            {
                QuantityItems = request.Quantity,
                Items = new List<DonationItemDto>
        {
            new DonationItemDto
            {
                SupplyId = request.SupplyId,
                ImageFile = imageUrl,
                Quantity = request.Quantity,
                Status = donationItem.Status,
                Description = donationItem.Description
            }
        },
                Message = "تم تسجيل التبرع بالمنتج ✔"
            };
        }

        public async Task<DonationResponseCashDto> CreateDonationCashAsync(
           CreateDonationCashRequestDto request, int donorId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var donationOrder = new DonationOrder
            {
                DonorId = donorId,
                CampaignId = request.CampaignId,
                ManagerId = 1, // Default manager assignment
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                DonationCategoryStatus = "معلقه",
                Status = "قيد المراجعة",


            };

            await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
            await _unitOfWork.Complete();


            int? cashReceiptId = null;
           

            // Step 2: Cash donation
            if (request.CashAmount.HasValue && request.CashAmount > 0)
            {
                donationOrder.CashAmount = request.CashAmount.Value;
                _unitOfWork.Repository<DonationOrder, int>().Update(donationOrder);
                await _unitOfWork.Complete();

                var cash = new CashDonation
                {

                    DonationOrderId = donationOrder.Id,
                    Amount = request.CashAmount.Value
                };

                await _unitOfWork.Repository<CashDonation, int>().AddAsync(cash);
                await _unitOfWork.Complete();
                var receipt = new Receipt
                {
                    DonationOrderId = donationOrder.Id,
                    CashDonationId = cash.Id,

                    DonorId = donorId,
                    ManagerId = 1
                };

                // Save
                await _unitOfWork.Repository<Receipt, int>().AddAsync(receipt);

                await _unitOfWork.Complete();

                cashReceiptId = receipt.Id;
            }

            
            await _unitOfWork.Complete();

            // Step 4: Response

            
                return new DonationResponseCashDto
                {

                    CashAmount = request.CashAmount,

                    CashReceiptId = cashReceiptId,
                    Message = "تم تسجيل التبرع بنجاح ✔"
                };

           



        }

        

    }
}

