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
                CampaignId = request.CampaignId,
                ManagerId = 1,
                CreatedAt = DateTime.Now,
                OrderDate = DateTime.Now,
                DonationCategoryStatus = "معلقة",
                Status = "قيد المراجعة",
            };

            await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
            await _unitOfWork.Complete();

            // Add existing items
            foreach (var item in request.Items)
            {
                var donationItem = new OrderItem
                {
                    CustomSupplyName = item.CustomName,
                    DonationOrderId = donationOrder.Id,
                    SupplyId = item.SupplyId,
                    Quantity = item.Quantity,
                    ImageUrl = item.ImageFile,
                };

                await _unitOfWork.Repository<OrderItem, int>().AddAsync(donationItem);

                // Update stock
                var supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(item.SupplyId);
                if (supply != null)
                {
                    supply.StockQuantity += item.Quantity;
                    _unitOfWork.Repository<Supply, int>().Update(supply);
                }
            }

            await _unitOfWork.Complete();

            return new DonationResponseItemDto
            {
                TotalItems = request.Items.Sum(i => i.Quantity),
                Items = request.Items,
                Message = "تم تسجيل التبرع بالمنتجات الموجودة ✔"
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

