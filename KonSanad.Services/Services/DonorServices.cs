
using Microsoft.EntityFrameworkCore;

namespace KonSanad.Services.Services
{
    public class DonorServices : IDonorServices
    {
       // private readonly IDonorServices _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public DonorServices(
         
            IUnitOfWork unitOfWork,
            IMapper mapper, UserManager<AppUser> userManager)
        {
         
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;

        }

        // Helper to map DonationOrder.Status string to DonationRequestStatus enum
        private Core.Entity.DonationRequestStatus MapDonationOrderStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return Core.Entity.DonationRequestStatus.Pending;
            var s = status.Trim().ToLower();
            if (s.Contains("قيد") || s.Contains("مراجعة") || s.Contains("pending")) return Core.Entity.DonationRequestStatus.Pending;
            if (s.Contains("مكتمل") || s.Contains("completed") || s.Contains("complete")) return Core.Entity.DonationRequestStatus.Completed;
            if (s.Contains("مرفوض") || s.Contains("rejected")) return Core.Entity.DonationRequestStatus.Rejected;
            return Core.Entity.DonationRequestStatus.Pending;
        }


        // ---------------------------------------------------------------------------
        // 2) Get All Low-Stock Supplies
        // ---------------------------------------------------------------------------

        //  new : pagenation
        public async Task<List<SupplyDto>> GetLowStockSuppliesAsync(int pageNumber, int pageSize)
        {
            var supplies = await _unitOfWork.Repository<Supply, int>()
                .GetQueryable()
                .Where(s => s.StockQuantity <= s.MinimumLevel)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return supplies.Select(s => new SupplyDto
            {
                Id = s.Id,
                Name = s.Name,
                Quantity = s.StockQuantity ?? 0,
                imageurl = s.ImageUrl,
                Unit = s.Unit,
                title = s.Title
            }).ToList();
        }

        // ---------------------------------------------------------------------------
        // 3) Create Donation (Cash + Items)
        // ---------------------------------------------------------------------------
        /*   public async Task<DonationResponseItemDto> CreateDonationItemAsync(
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
                   //QuantityItems = request.Quantity,
                   Items = new List<object>
           {
               new DonationItemDto
               {
                   SupplyId = request.SupplyId,
                   //ImageFile = imageUrl,
                   Quantity = request.Quantity,
                   Status = donationItem.Status,
                   Description = donationItem.Description
               }
           },
                   Message = "تم تسجيل التبرع بالمنتج ✔"
               };
           }
   */

        /*    public async Task<DonationResponseItemDto> CreateCustomItemDonationAsync(
                CreateCustomDonationItemRequestDto request, int donorId)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Validate custom item name
                if (string.IsNullOrWhiteSpace(request.CustomItemName))
                    throw new ArgumentException("اسم العنصر المخصص مطلوب", nameof(request.CustomItemName));

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

                // Ensure upload folder exists
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Handle image upload
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

                // Create donation item for CUSTOM item (SupplyId = null)
                var donationItem = new OrderItem
                {
                    DonationOrderId = donationOrder.Id,
                    SupplyId = null, 
                    Quantity = request.Quantity,
                    ImageUrl = imageUrl,
                    CustomSupplyName = request.CustomItemName, 
                    Unit = request.Unit ?? "قطعة", //
                    Status = string.IsNullOrWhiteSpace(request.Status) ? "قيد المراجعة" : request.Status,
                    Description = request.Description
                };

                await _unitOfWork.Repository<OrderItem, int>().AddAsync(donationItem);
                await _unitOfWork.Complete();

                return new DonationResponseItemDto
                {
                    QuantityItems = request.Quantity,
                    Items = new List<object>
            {
                new DonationNewItemDto
                {
                    //SupplyId = 0, // No supply ID for custom items
                    CustomName = request.CustomItemName,
                    Unit = donationItem.Unit,
                    Quantity = request.Quantity,
                    Status = donationItem.Status,
                    Description = donationItem.Description,
                    ImageFile = imageUrl
                }
            },
                    Message = "تم تسجيل التبرع بعنصر مخصص ✔"
                };
            }
    */
        /*      public async Task<DonationResponseCashDto> CreateDonationCashAsync(CreateDonationCashRequestDto request, int donorId)
              {
                  // 1. Validate request
                  if (request is null)
                      throw new ArgumentNullException(nameof(request));


                  // 2. Create donation order with cash amount
                  var donationOrder = new DonationOrder
                  {
                      DonorId = donorId,
                      HelpOrderid = request.HelpOrderId,
                      CampaignId = request.CampaignId,
                      ManagerId = 1, // Default manager assignment
                      OrderDate = DateTime.Now,
                      CreatedAt = DateTime.Now,
                      DonationCategoryStatus = "معلقه",
                      Status = "قيد المراجعة",
                      CashAmount = request.CashAmount
                  };


                  // 3. Save donation order (cash donation)
                  await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
                  await _unitOfWork.Complete();


                  // 4. Initialize cashReceiptId to null; it will be set if a cash donation record is created
                  int? cashReceiptId = null;


                  // 5. Create cash donation record if cash amount is provided
                  if (request.CashAmount.HasValue && request.CashAmount > 0)
                  {
                      // 5.1 Update the DonationOrder with the cash amount (if not already set)
                      donationOrder.CashAmount = request.CashAmount.Value;
                      _unitOfWork.Repository<DonationOrder, int>().Update(donationOrder);
                      await _unitOfWork.Complete();

                      // 5.2 Create CashDonation record linked to the DonationOrder
                      var cash = new CashDonation
                      {
                          DonationOrderId = donationOrder.Id,
                          Amount = request.CashAmount.Value
                      };

                      // 5.3 Save CashDonation record
                      await _unitOfWork.Repository<CashDonation, int>().AddAsync(cash);
                      await _unitOfWork.Complete();

                      // 5.4 Create Receipt record linked to the DonationOrder and CashDonation
                      var receipt = new Receipt
                      {
                          DonationOrderId = donationOrder.Id,
                          CashDonationId = cash.Id,
                          DonorId = donorId,
                          ManagerId = 1
                      };

                      // 5.5 Save Receipt record
                      await _unitOfWork.Repository<Receipt, int>().AddAsync(receipt);
                      await _unitOfWork.Complete();

                      // 5.6 Set cashReceiptId to the ID of the created receipt
                      cashReceiptId = receipt.Id;
                  }


                  // 6. return response with cash amount and receipt ID (if created)
                  return new DonationResponseCashDto
                  {
                      CashAmount = request.CashAmount,
                      CashReceiptId = cashReceiptId,
                      Message = "تم تسجيل التبرع بنجاح ✔"
                  };
              }
      */

        // ---------------------------------------------------------------------------
        // New: Get all DonationOrders made by the donor (cash and/or items)
        // ---------------------------------------------------------------------------


        // Get all cash donations made by the donor, including related receipts
        public async Task<IReadOnlyList<CashDonationDto>> GetMyCashDonationsAsyncc(string userId)
        {
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);
            if (donor is null)
                throw new Exception($"Donor not found for userId: {userId}");

            // جيب كل الـ CashDonations
            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
       .FindAllAsync(
           cd => cd.DonationOrderId > 0,  // بدل true
           include: q => q
               .Include(cd => cd.DonationOrder)
               .Include(cd => cd.Receipt)
       );
            // فلتر في الميموري زي الـ SQL
            var result = cashDonations
                .Where(cd => cd.DonationOrder != null && cd.DonationOrder.DonorId == donor.Id)
                .Select(d => new CashDonationDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    Status = d.Status,
                    CashAmount = d.Amount,
                    //new status
                    cashDonationStatus = d.cashDonationStatus,
                    
                    ReceiptIds = d.Receipt != null
                        ? new List<int> { d.Receipt.Id }
                        : new List<int>()
                }).ToList();

            return result;
        }
        //-----------------------------------------------------------------------------------
        public async Task<IEnumerable<ItemDonationDto>> GetMyItemDonationsAsync(string userId)
        {
            // 1) Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");

            // 2) Query DonationOrders that have Items only
            var itemDonations = await _unitOfWork.Repository<DonationOrder, int>()
                .FindAllAsync(
                    d => d.DonorId == donor.Id && d.OrderItems.Any(),
                    include: q => q
                        .Include(d => d.OrderItems)
                            .ThenInclude(oi => oi.Supply)
                        .Include(d => d.Receipts)
                );

            // 3) Map to DTO
            var result = itemDonations.Select(d => new ItemDonationDto
            {
                Id = d.Id,
                OrderDate = d.OrderDate,
                CreatedAt = d.CreatedAt,
                Status = d.Status,
                Items = d.OrderItems?.Select(oi => new DonationItemDetailDto
                {
                    SupplyId = oi.SupplyId ?? 0,
                    Quantity = oi.Quantity,
                    CustomName = oi.CustomSupplyName,
                    ImageFile = oi.ImageUrl,
                    Unit = oi.Unit
                }).ToList() ?? new List<DonationItemDetailDto>(),
                ReceiptIds = d.Receipts?.Select(r => r.Id).ToList() ?? new List<int>()
            }).ToList();

            return result;
        }

        // Get all cash donations made by the donor, including related receipts
       

        #region GetMyItemDonationsAsync
        //-----------------------------------------------------------------------------------

        public async Task<IEnumerable<MyOrderItemDto>> GetMyDonatedOrderItemsAsync(string userId)
        {
            // 1️⃣ هات المتبرع
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            //لو ملقاش المتبرع ال فالتوكين بيساوي ال فجدول ال appUserId
            if (donor == null)
                throw new Exception("Donor not found");


            // 2️⃣ هات OrderItems باستخدام Repository
            // جلب الـ OrderItems الخاصة بالمتبرع

            var orderItems = await _unitOfWork.Repository<OrderItem, int>()
                .FindAllAsync(
                    oi => oi.DonationOrder != null &&
                          oi.DonationOrder.DonorId == donor.Id,
                    include: q => q.Include(oi => oi.DonationOrder)
                );

            // 3️⃣ Mapping آمن يمنع Null crash
            // ?? string.Empty دي علشان لو القيمه ب null في الداتا بيز الاسواجر بيضرب

            var result = orderItems.Select(oi => new MyOrderItemDto
            {
                Id = oi.Id,
                DonationOrderId = oi.DonationOrderId,
                SupplyId = oi.SupplyId,
                CustomSupplyName = oi.CustomSupplyName ?? string.Empty,
                Quantity = oi.Quantity,
                Category = oi.Category ?? string.Empty,
                Unit = oi.Unit ?? string.Empty,
                CreatedAt = oi.CreatedAt,
                Status = oi.Status ?? string.Empty,
                ImageUrl = oi.ImageUrl ?? string.Empty,
                Description = oi.Description ?? string.Empty,

                orderItemStatus = oi.orderItemStatus

            }).ToList();

            return result;
        }
        //-------------------------------------------------------------------------------
        #endregion
        // Get all campaigns that the donor has donated From it .
        // -----------------------------------------------------------
        public async Task<IEnumerable<CampaignDto>> GetMyDonationsInCampaignRequestsAsync(string userId)
        {
            // 1. Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            // 2. check if donor exists
            if (donor is null)
                throw new Exception($"Donor not found for userId: {userId}");

            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
     .FindAllAsync(
         cd => cd.CampaignId != null && cd.CampaignId != 0,  // ✅ أضف != 0
         include: q => q
             .Include(cd => cd.DonationOrder)
             .Include(cd => cd.Campaign)
                 .ThenInclude(c => c.Category)
     );

            var campaigns = cashDonations
                .Where(cd => cd.DonationOrder != null && cd.DonationOrder.DonorId == donor.Id)
                .Select(cd => cd.Campaign)
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            // 4. Map to CampaignDto and compute a few helpful aggregates
            var result = campaigns.Select(c =>
            {
                var totalCash = cashDonations
       .Where(cd => cd.CampaignId == c.Id)
       .Sum(cd => cd.Amount);
                var donorCount = cashDonations
         .Where(cd => cd.CampaignId == c.Id && cd.DonationOrder?.DonorId != null)
         .Select(cd => cd.DonationOrder.DonorId)
         .Distinct()
         .Count();

                int progressPercentage = 0;
                if (c.GoalCash.HasValue && c.GoalCash > 0)
                {
                    progressPercentage = (int)Math.Min(100,
                        Math.Round((double)(totalCash / c.GoalCash.Value) * 100));
                }

                return new CampaignDto
                {
                    Id = c.Id,

                    Title = c.Title ?? string.Empty,

                    ShortDescription = string.IsNullOrEmpty(c.Description)
                        ? string.Empty
                        : (c.Description.Length > 150 ? c.Description.Substring(0, 150) : c.Description),

                    PictureUrl = c.PictureUrl ?? string.Empty,

                    CategoryName = c.Category?.Name ?? string.Empty,

                    Status = DonationRequestStatus.Pending.ToString(), 

                    ProgressPercentage = progressPercentage,

                    DonorCount = donorCount,
                };
            }).ToList();

            // 5. Return result
            return result;

        }
        // -------------------------------------------------------------------

        public async Task<bool> DeleteCashDonationAsync(int cashDonationId)
        {
            var repo = _unitOfWork.Repository<CashDonation, int>();

            var cashDonation = await repo.FirstOrDefaultAsync(
                cd => cd.Id == cashDonationId,
                include: q => q
                    .Include(cd => cd.DonationOrder)
                    .Include(cd => cd.Receipt)
            );

            if (cashDonation == null)
                return false;

            // حذف الكاش
            repo.Delete(cashDonation);

            // حذف الاوردر
            if (cashDonation.DonationOrder != null)
            {
                _unitOfWork.Repository<DonationOrder, int>()
                    .Delete(cashDonation.DonationOrder);
            }

            await _unitOfWork.Complete();

            return true;
        }
        public async Task<bool> UpdateCashDonationAmountAsync(int cashDonationId, decimal amount)
        {
            var repo = _unitOfWork.Repository<CashDonation, int>();

            var cashDonation = await repo.GetByIdAsync(cashDonationId);

            if (cashDonation == null)
                throw new Exception("CashDonation not found");

            cashDonation.Amount = amount;

            repo.Update(cashDonation);

            await _unitOfWork.Complete();

            return true;
        }

        //del
        public async Task<IEnumerable<CampaignDto>> GetCampaignsDonatedByAsync(string userId)
        {
            // 1) Get donor
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");
            var allOrders = await _unitOfWork.Repository<DonationOrder, int>()
    .FindAllAsync(d => d.DonorId == donor.Id);
            Console.WriteLine($"Total orders for donor {donor.Id}: {allOrders.Count()}");
            // جيب الـ CashDonations مع include للـ DonationOrder
            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
                .FindAllAsync(
                    cd => cd.CampaignId != null,
                    include: q => q
                        .Include(cd => cd.DonationOrder)
                        .Include(cd => cd.Campaign)
                            .ThenInclude(c => c.Category)
                );

            // فلتر بعد ما اتجبت البيانات
            var campaigns = cashDonations
                .Where(cd => cd.DonationOrder != null && cd.DonationOrder.DonorId == donor.Id)
                .Select(cd => cd.Campaign)
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();
            // 4) Map to DTO
            var result = campaigns.Select(c =>
            {
                var totalCash = c.DonationOrders.Sum(d => d.CashAmount ?? 0m);

                var donorCount = c.DonationOrders
                    .Where(d => d.DonorId.HasValue)
                    .Select(d => d.DonorId)
                    .Distinct()
                    .Count();

                int progressPercentage = 0;

                if (c.GoalCash.HasValue && c.GoalCash > 0)
                {
                    progressPercentage = (int)Math.Min(100,
                        Math.Round((double)(totalCash / c.GoalCash.Value) * 100));
                }

                return new CampaignDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    ShortDescription = string.IsNullOrEmpty(c.Description)
                        ? ""
                        : (c.Description.Length > 150 ? c.Description.Substring(0, 150) : c.Description),

                    PictureUrl = c.PictureUrl,
                    CategoryName = c.Category?.Name ?? "",
                    Status = (c.EndDate.HasValue && c.EndDate.Value < DateTime.UtcNow)
                        ? "Completed"
                        : "Active",

                    ProgressPercentage = progressPercentage,
                    DonorCount = donorCount
                };

            }).ToList();

            return result;
        }
        // ---------------------------------------------------------------------------
        // 4) Get Profile To Donor 
        // ---------------------------------------------------------------------------
        public async Task<GetProfileDto> GetProfileAsync(string userId)
        {
            // 1) استخدم UserManager للحصول على AppUser
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser == null)
                throw new Exception("User not found");

            // 2) ثم احصل على Donor
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor is null)
                throw new Exception("Donor not found");

            // 3) Map to DTO
            var profileDto = new GetProfileDto
            {

                FullName = donor.FullName ?? string.Empty,
                NationalId = donor.NationalId ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber ?? string.Empty,
                Address = donor.Address ?? string.Empty,
                Email = appUser.Email ?? string.Empty
            };
            return profileDto;
        }


        // ---------------------------------------------------------------------------
        // 5) Update Profile To Donor
        // ---------------------------------------------------------------------------
        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var donorRebo = _unitOfWork.Repository<Donor, int>();

            var donor = await donorRebo.FirstOrDefaultAsync(
                d => d.AppUserId == userId
            );

            //var dtos = dto;

            //foreach (var prop in typeof(UpdateProfileDto).GetProperties())
            //{
            //    var value = prop.GetValue(dtos) as string;
            //    if (value == "string")
            //    {
            //        prop.SetValue(dtos, null);
            //    }
            //}

            if (donor is null)
                throw new Exception("Donor not found");

            // Update fields only when a value is provided (non-null/non-whitespace)
            if (!string.IsNullOrWhiteSpace(dto.FullName) && !(dto.FullName == "string"))
                donor.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.NationalId) && !(dto.NationalId == "string"))
                donor.NationalId = dto.NationalId;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
                donor.Phone = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Address) && !(dto.Address == "string"))
                donor.Address = dto.Address;

            // If email is provided, update Identity user email (and optionally beneficiary.Email)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !(dto.Email == "string"))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
                    throw new Exception("App user not found");

                if (!string.Equals(appUser.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.Email = dto.Email;
                    // Do not change UserName unless your system expects it; uncomment if needed:
                    // appUser.UserName = dto.Email;

                    var result = await _userManager.UpdateAsync(appUser);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update identity user: {errors}");
                    }
                }

                // Keep beneficiary.Email in sync if you store it there too
                donor.Email = dto.Email;
            }
            // If PhoneNumber is provided, update Identity user phone number
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
                    throw new Exception("App user not found");
                if (!string.Equals(appUser.PhoneNumber, dto.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    var result = await _userManager.SetPhoneNumberAsync(appUser, dto.PhoneNumber);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update phone number: {errors}");
                    }
                }

                // Keep donor.Phone in sync
                donor.Phone = dto.PhoneNumber;
            }


            _unitOfWork.Repository<Donor, int>().Update(donor);
            await _unitOfWork.Complete();

            return true;
        }

      
    }
}

