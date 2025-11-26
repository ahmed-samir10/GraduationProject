using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.Entity.Identity;
using KonSanad.APIS.DTOs;

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
        }
    }
}
