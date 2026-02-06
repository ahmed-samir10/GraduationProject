
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
               imageurl = s.ImageUrl,
                Unit = s.Unit,
                title = s.Title

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

        
        public async Task<DonationResponseItemDto> CreateCustomItemDonationAsync(
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

        public async Task<DonationResponseCashDto> CreateDonationCashAsync(
           CreateDonationCashRequestDto request, int donorId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

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
        // get myrequst 

        // ---------------------------------------------------------------------------
        // New: Get all DonationOrders made by the donor (cash and/or items)
        // ---------------------------------------------------------------------------

        public async Task<IEnumerable<CashDonationDto>> GetMyCashDonationsAsync(string userId)
        {
            // 1) Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");

            // 2) Query DonationOrders that have CashDonation only
            var cashDonations = await _unitOfWork.Repository<DonationOrder, int>()
                .FindAllAsync(
                    d => d.DonorId == donor.Id && d.CashDonation != null,
                    include: q => q
                        .Include(d => d.CashDonation)
                        .Include(d => d.Receipts)
                );

            // 3) Map to DTO
            var result = cashDonations.Select(d => new CashDonationDto
            {
                Id = d.Id,
                OrderDate = d.OrderDate,
                CreatedAt = d.CreatedAt,
                Status = d.Status,
                CashAmount = d.CashAmount ?? 0,
                ReceiptIds = d.Receipts?.Select(r => r.Id).ToList() ?? new List<int>()
            }).ToList();

            return result;
        }
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

       
        public async Task<IEnumerable<CampaignDto>> GetCampaignsDonatedByAsync(string userId)
        {
            // 1) Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");

            // 2) Query campaigns that have at least one DonationOrder from this donor
            var campaigns = await _unitOfWork.Repository<Campaign, int>()
                .FindAllAsync(
                    c => c.DonationOrders.Any(d => d.DonorId == donor.Id),
             include: q => q
                 .Include(c => c.Category)
                 .Include(c => c.DonationOrders)
         );

            // 3) Map to CampaignDto and compute a few helpful aggregates
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
                        ? string.Empty
                        : (c.Description.Length > 150 ? c.Description.Substring(0, 150) : c.Description),
                    PictureUrl = c.PictureUrl,
                    CategoryName = c.Category?.Name ?? string.Empty,
                    Status = (c.EndDate.HasValue && c.EndDate.Value < DateTime.UtcNow) ? "Completed" : "Active",
                    ProgressPercentage = progressPercentage,
                    DonorCount = donorCount,
                  //  CreateAt = c.CreatedAt ?? DateTime.UtcNow
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

