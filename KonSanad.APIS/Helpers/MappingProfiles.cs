using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.Entity.Identity;
using KonSanad.Repository.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace KonSanad.APIS.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // existing mappings kept...
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<RegisterDto, UserDto>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<Beneficiary, BeneficiaryDto>();
            CreateMap<HelpOrder, HelpOrderDto>();
            CreateMap<HelpOrderItem, HelpOrderItemDto>();
            CreateMap<SupplyMovement, SupplyMovementDto>();

            CreateMap<OrderItem, SupplyDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SupplyId ?? 0))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Supply != null ? src.Supply.Name : src.CustomSupplyName))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

            CreateMap<Manager, BeneficiaryDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone))
                .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address));

            // CreateDto -> Campaign entity (for create/update)
            CreateMap<CampaignCreateDto, Campaign>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            // Campaign -> CampaignDto (presentation)
            CreateMap<Campaign, CampaignDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.ShortDescription, opt => opt.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.Description) ? string.Empty :
                    s.Description.Length > 180 ? s.Description.Substring(0, 180).Trim() + "…" : s.Description))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
                .ForMember(d => d.ProgressPercentage, opt => opt.MapFrom(s => CampaignMetrics.CalculateProgress(s)))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => CampaignMetrics.CalculateStatus(s)))
                // resolve PictureUrl via resolver registered in DI (CampaignUrlResolver depends on IConfiguration)
                .ForMember(d => d.PictureUrl, opt => opt.MapFrom<CampaignUrlResolver>());

            // --- New DTO -> Entity mappings (help requests) ---
            // Map a "new product" request into a HelpOrder (store details in Description / ImageUrlsJson)
            CreateMap<NewProductRequestDto, HelpOrder>()
                // readable description (no JSON)
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                    $"ProductName: {src.ProductName ?? string.Empty}; Condition: {src.Status ?? string.Empty}; Reason: {src.Reason ?? string.Empty}"))
                // map image input using dedicated resolver (ImageUrlsResolver should return string? JSON array or single URL string)
                .ForMember(dest => dest.ImageUrlsJson, opt => opt.MapFrom<ImageUrlsResolver>())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Requested"))
                .ForMember(dest => dest.ManagerId, opt => opt.Ignore())
                .ForMember(dest => dest.BeneficiaryId, opt => opt.Ignore());
                //.ForAllOtherMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Map an "existing product" request (references an OrderItem) into a HelpOrder (short description)
            CreateMap<ExistingProductRequestDto, HelpOrder>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                    $"Request for donated item Id={src.OrderItemId}. Reason: {src.Reason ?? string.Empty}"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Requested"))
                .ForMember(dest => dest.ManagerId, opt => opt.Ignore())
                .ForMember(dest => dest.BeneficiaryId, opt => opt.Ignore());
        }
    }
}