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
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PHONE_NUMBER));

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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.OS.NAME))
                .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.O_MATERIAL))
                .ForMember(dest => dest.Samples, opt => opt.MapFrom(src => src.O_TEMPLATE));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_MATERIAL, GPMS.DOMAIN.Entities.OrderMaterial>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OM_ID));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_TEMPLATE, GPMS.DOMAIN.Entities.OrderSample>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OT_ID));
        }
    }
}
