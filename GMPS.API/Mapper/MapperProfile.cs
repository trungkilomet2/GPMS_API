using AutoMapper;
using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using static GMPS.API.DTOs.ProductionOutputDTO;

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
                .ForMember(dest => dest.PmInfo, opt => opt.MapFrom(src => src.ProjectManager))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Production.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Production.EndDate))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Production.StatusId))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.ProductionStatusName));
            CreateMap<Order, ListOrderProductionDTO>();
            CreateMap<User, PMInfo>();


            // ProductionDetail
            CreateMap<ProductionDetailViewDTO, ProductionDetailDTO>()
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.Production.Id))
                .ForMember(dest => dest.Pm, opt => opt.MapFrom(src => src.ProjectManager))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Production.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Production.EndDate))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Production.StatusId))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order));
            CreateMap<User, ProductionDetailPMDTO>();
            CreateMap<Order, ProductionDetailOrderDTO>()
                .ForMember(dest => dest.OrderSizes, opt => opt.MapFrom(src => src.Size))
                .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => src.Template))
                .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Material));
            CreateMap<OrderSize, ProductionDetailOrderSizeDTO>()
                .ForMember(dest => dest.OrderSizeStatusName, 
                opt => opt.MapFrom(src => src.OrderSizeStatusId == OrderSizeStatus_Constants.Pending_Id ? OrderSizeStatus_Constants.Pending :
                src.OrderSizeStatusId == OrderSizeStatus_Constants.InProgress_Id ? OrderSizeStatus_Constants.InProgress :
                src.OrderSizeStatusId == OrderSizeStatus_Constants.Completed_Id ? OrderSizeStatus_Constants.Completed : "Không Xác Định"
                ));



            //------------------------ Production Part ----------------------------------------------------------//

            CreateMap<GPMS.APPLICATION.DTOs.ProductionPartDetailViewDTO, ProductionPartDetailDTO>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Part.Id))
             .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.Part.ProductionId))
             .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.PartName))
             .ForMember(dest => dest.Cpu, opt => opt.MapFrom(src => src.Part.Cpu))
             .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Part.StatusId))
             .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Part.StatusName))
             .ForMember(dest => dest.ListPartOrderSizes, opt => opt.MapFrom(src => src.ListPartOrderSize));


            //----------------------------------------------------------------------------------------------------//

            CreateMap<AssignWorkerViewDTO, DataAssignWorkerViewDTO>()
                .ForMember(dest => dest.WorkerInfo, opt => opt.MapFrom(src => src.Workers))
                .ForMember(dest => dest.WorkerSkillInfo, opt => opt.MapFrom(src => src.Skill_Of_Worker))
                .ForMember(dest => dest.WorkerLrInfo, opt => opt.MapFrom(src => src.LeaveRequest))
                .ReverseMap();
            CreateMap<User, WorkerInfor>()
                .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.FullName)).ReverseMap();
            CreateMap<WorkerSkill, WorkerSkillInfo>()
                .ForMember(dest=> dest.SkillName,opt => opt.MapFrom(src => src.Name)).ReverseMap();
            CreateMap<LeaveRequest, WorerLRInfo>()
                .ForMember(dest => dest.DateLR, opt => opt.MapFrom(src => src.DateReply)).ReverseMap();


            CreateMap<ProductionRejectReason, RejectReasonData>().ReverseMap();


            //--------------------------------------------------------------------

            CreateMap<CuttingNotebook, CuttingNotebookResponseDTO>()
                .ForMember(dest => dest.NotebookId, opt => opt.MapFrom(src => src.Id));

            CreateMap<CuttingNotebookLog, CuttingNotebookLogResponseDTO>()
                .ForMember(dest => dest.LogId, opt => opt.MapFrom(src => src.Id));

            //   CreateMap<ProductionPartProductivityViewDTO, ProductivityHistoryItemDTO>();



            CreateMap<ProductionWorkerOutputViewDTO, ProductionWorkerOutputDTO>();
            CreateMap<WorkerProductivityHistoryViewDTO, WorkerProductivityHistoryDTO>();
            CreateMap<ProductionOutputSummaryViewDTO, ProductionOutputSummaryDTO>();
            CreateMap<WorkerAssignedPlanViewDTO, WorkerAssignedPlanDTO>();

            // Payment 
            CreateMap<PartPaymentCompletionViewDTO, PartPaymentCompletionDTO>();

            CreateMap<ProductionPartCompletionEstimateViewDTO, ProductionPartCompletionEstimateDTO>();
            CreateMap<ProductionWorkerProgressChartViewDTO, ProductionWorkerProgressChartDTO>();
            CreateMap<WorkerProductivityScoreViewDTO, WorkerProductivityScoreDTO>();

            //Mapp User For Log work
            CreateMap<User, ProductionPartUserDTO>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }

    }
}
