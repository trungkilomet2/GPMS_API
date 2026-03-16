using AutoMapper;
using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.DOMAIN.Entities;

namespace GMPS.API.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {

            //CreateMap<ListProductionDTO, ProductionDetailViewDTO>()
            //    .ForMember(dest => dest.Production.Id, opt => opt.MapFrom(src => src.Id))
            //    .ReverseMap();


            CreateMap<ProductionDetailViewDTO, ListProductionDTO>()
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.Production.Id))
                .ForMember(dest => dest.PmId, opt => opt.MapFrom(src => src.Production.PmId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Production.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Production.EndDate))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Production.StatusId))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order));
            CreateMap<Order, ListOrderProductionDTO>();
                
            // ProductionDetail
            CreateMap<ProductionDetailViewDTO, ProductionDetailDTO>()
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.Production.Id))
                .ForMember(dest => dest.Pm, opt => opt.MapFrom(src => src.ProjectManager))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Production.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Production.EndDate))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Production.StatusId))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order));
            CreateMap<User, ProductionDetailPMDTO>();
            CreateMap<Order, ProductionDetailOrderDTO>();

        }

    }
}
