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
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PHONE_NUMBER)).ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FULLNAME))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PASSWORDHASH))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FULLNAME))
                .ForMember(dest => dest.AvartarUrl, opt => opt.MapFrom(src => src.AVATAR))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.LOCATION))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EMAIL)).ReverseMap();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ROLE, GPMS.DOMAIN.Entities.Role>();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ORDER, GPMS.DOMAIN.Entities.Order>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ORDER_ID))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.OrderName, opt => opt.MapFrom(src => src.ORDER_NAME))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.IMAGE))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TYPE))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.SIZE))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.COLOR))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.START_DATE))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.END_DATE))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QUANTITY))
                .ForMember(dest => dest.Cpu, opt => opt.MapFrom(src => src.CPU))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.NOTE))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.OS.NAME)).ReverseMap();
                


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
