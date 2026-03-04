using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Mappers
{
    public class SqlServerToEntityProfile : Profile
    {
        public SqlServerToEntityProfile()
        {
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.USER, GPMS.DOMAIN.Entities.User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PHONE_NUMBER))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PASSWORDHASH))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FULLNAME))
                .ForMember(dest => dest.AvartarUrl, opt => opt.MapFrom(src => src.AVATAR))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.LOCATION))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EMAIL));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ROLE, GPMS.DOMAIN.Entities.Role>();
                .ForMember(dest => dest.Id,opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.PhoneNumber,opt => opt.MapFrom(src => src.PHONE_NUMBER)).ForMember(dest => dest.FullName,opt => opt.MapFrom(src => src.FULLNAME)).ReverseMap();
                
        }
    }
}
