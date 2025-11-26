
                using AutoMapper;
                using KonSanad.Core.Dtos;
                using KonSanad.Core.Entity.Identity;
                using KonSanad.Repository.Entities;
                using System;
                using System.Linq;

                namespace KonSanad.APIS.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // RegisterDto -> AppUser
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            // RegisterDto -> UserDto (for immediate response)
            CreateMap<RegisterDto, UserDto>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            // Domain -> DTO mappings for Beneficiary and related entities
            CreateMap<Beneficiary, BeneficiaryDto>();
            CreateMap<HelpOrder, HelpOrderDto>();
            CreateMap<HelpOrderItem, HelpOrderItemDto>();
            CreateMap<SupplyMovement, SupplyMovementDto>();

            // Map Manager -> BeneficiaryDto (use manager as campaign owner shown to users)
            CreateMap<Manager, BeneficiaryDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone))
                .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address));

            // Map OrderItem -> SupplyDto (used to populate CampaignDto.Supplies and DonationItemDto)
            CreateMap<OrderItem, SupplyDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SupplyId ?? 0))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Supply != null ? src.Supply.Name : src.CustomSupplyName))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

            // Map OrderItem -> DonationItemDto
            CreateMap<OrderItem, DonationItemDto>()
                .ForMember(d => d.SupplyId, opt => opt.MapFrom(s => s.SupplyId ?? 0))
                .ForMember(d => d.Quantity, opt => opt.MapFrom(s => s.Quantity))
                .ForMember(d => d.CustomName, opt => opt.MapFrom(s => s.CustomSupplyName))
                .ForMember(d => d.Unit, opt => opt.MapFrom(s => s.Unit));

            // Map DonationOrder -> DonationResponseDto
            CreateMap<DonationOrder, DonationResponseDto>()
                .ForMember(d => d.DonationOrderId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.CashAmount, opt => opt.MapFrom(s => s.CashDonation != null ? (decimal?)s.CashDonation.Amount : s.CashAmount))
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.OrderItems));

            // Campaign -> CampaignDto (user-visible fields only)
            CreateMap<Campaign, CampaignDto>()
                .ForMember(d => d.ShortDescription, opt => opt.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.Description) ? string.Empty :
                    s.Description.Length > 180 ? s.Description.Substring(0, 180).Trim() + "…" : s.Description))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                // map campaign owner (manager) into BeneficiaryDto for UI card
                .ForMember(d => d.Beneficiary, opt => opt.MapFrom(s => s.Manager))
                // flatten donation orders' order items into Supplies and map via OrderItem -> SupplyDto
                .ForMember(d => d.Supplies, opt => opt.MapFrom(s => s.DonationOrders != null
                    ? s.DonationOrders.SelectMany(dono => dono.OrderItems)
                    : Enumerable.Empty<OrderItem>()))
             .ForMember(d => d.ProgressPercentage,
           opt => opt.MapFrom(s => CampaignMappingUtils.CalculateProgress(s)))

       .ForMember(d => d.Status,
           opt => opt.MapFrom(s => CampaignMappingUtils.CalculateStatus(s)));



        }
    }
}
