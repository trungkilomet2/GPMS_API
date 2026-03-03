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
                .ForMember(dest => dest.PhoneNumber,opt => opt.MapFrom(src => src.PHONE_NUMBER));
                
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ROLE, GPMS.DOMAIN.Entities.Role>();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.UO_COMMENT, GPMS.DOMAIN.Entities.Comment>()
                .ForMember(dest => dest.Id,opt => opt.MapFrom(src => src.OC_ID))
                .ForMember(dest => dest.fromUserId, opt => opt.MapFrom(src => src.FROM_USER))
                .ForMember(dest => dest.toOrderId, opt => opt.MapFrom(src => src.TO_ORDER))
                .ForMember(dest => dest.Content,opt => opt.MapFrom(src => src.CONTENT))
                .ForMember(dest => dest.SendDateTime,opt => opt.MapFrom(src => src.SEND_DATETIME)).ReverseMap();
        }
    }
}
