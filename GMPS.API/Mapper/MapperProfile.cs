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







            //------------------------ Production Part ----------------------------------------------------------//

            CreateMap<GPMS.APPLICATION.DTOs.ProductionPartDetailViewDTO, ProductionPartDetailDTO>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Part.Id))
             .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.Part.ProductionId))
             .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.PartName))
             .ForMember(dest => dest.TeamLeaderId, opt => opt.MapFrom(src => src.Part.TeamLeaderId))
             .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Part.StartDate))
             .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Part.EndDate))
             .ForMember(dest => dest.Cpu, opt => opt.MapFrom(src => src.Part.Cpu))
             .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Part.StatusId))
             .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Part.StatusName))
             .ForMember(dest => dest.TeamLeader, opt => opt.MapFrom(src => src.TeamLeader))
             .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.Assignees));

            CreateMap<User, ProductionPartUserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));


            //----------------------------------------------------------------------------------------------------//

            CreateMap<AssignWorkerViewDTO, DataAssignWorkerViewDTO>();
            CreateMap<User, WorkerInfor>()
                .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.FullName));
            CreateMap<WorkerSkill, WorkerSkillInfo>();
            CreateMap<LeaveRequest, WorerLRInfo>()
                .ForMember(dest => dest.DateLR, opt => opt.MapFrom(src => src.DateReply));


        }

    }
}
