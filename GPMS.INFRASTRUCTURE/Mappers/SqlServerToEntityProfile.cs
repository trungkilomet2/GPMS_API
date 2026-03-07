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
                .ForMember(dest => dest.Id,opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.PhoneNumber,opt => opt.MapFrom(src => src.PHONE_NUMBER)).ForMember(dest => dest.FullName,opt => opt.MapFrom(src => src.FULLNAME)).ReverseMap();
                
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ROLE, GPMS.DOMAIN.Entities.Role>().ReverseMap();
        }
    }
}
