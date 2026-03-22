using AutoMapper;
using GPMS.APPLICATION.DTOs;
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
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FULLNAME))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PHONE_NUMBER))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.USERNAME))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PASSWORDHASH))
                .ForMember(dest => dest.AvartarUrl, opt => opt.MapFrom(src => src.AVATAR))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.LOCATION))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EMAIL))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.US_ID))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.ROLE))
                .ForMember(dest => dest.WorkerSkills, opt => opt.MapFrom(src => src.WS))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.US))
                .ReverseMap();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ROLE, GPMS.DOMAIN.Entities.Role>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ROLE_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ReverseMap();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.WORKER_SKILL, GPMS.DOMAIN.Entities.WorkerSkill>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WS_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ReverseMap();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.U_STATUS, GPMS.DOMAIN.Entities.UserStatus>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.US_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ReverseMap();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_TEMPLATE, GPMS.DOMAIN.Entities.OTemplate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OT_ID));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_MATERIAL, GPMS.DOMAIN.Entities.OMaterial>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OM_ID))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ORDER_ID))
                .ForMember(dest => dest.Uom, opt => opt.MapFrom(src => src.UOM))
                .ReverseMap()
                .ForMember(dest => dest.OM_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ORDER_ID, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Uom));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_HISTORY_UPDATE, GPMS.DOMAIN.Entities.OHistoryUpdate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OHU_ID))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ORDER_ID))
                .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.FIELD_NAME))
                .ForMember(dest => dest.OldValue, opt => opt.MapFrom(src => src.OLD_VALUE))
                .ForMember(dest => dest.NewValue, opt => opt.MapFrom(src => src.NEW_VALUE))
                .ReverseMap();

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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.OS_ID))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.OS != null ? src.OS.NAME : null))
                .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => src.O_TEMPLATE))
                .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.O_MATERIAL))
                .ForMember(dest => dest.Histories, opt => opt.MapFrom(src => src.O_HISTORY_UPDATE))
                .ReverseMap();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.UO_COMMENT, GPMS.DOMAIN.Entities.Comment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OC_ID))
                .ForMember(dest => dest.fromUserId, opt => opt.MapFrom(src => src.FROM_USER))
                .ForMember(dest => dest.toOrderId, opt => opt.MapFrom(src => src.TO_ORDER))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.CONTENT))
                .ForMember(dest => dest.SendDateTime, opt => opt.MapFrom(src => src.SEND_DATETIME))
                .ReverseMap();

            // LEAVE REQUEST
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.LEAVE_REQUEST, GPMS.DOMAIN.Entities.LeaveRequest>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.LR_ID))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.USER != null ? src.USER.FULLNAME : null))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.CONTENT))
                .ForMember(dest => dest.DateCreate, opt => opt.MapFrom(src => src.DATE_CREATE))
                .ForMember(dest => dest.DateReply, opt => opt.MapFrom(src => src.DATE_REPLY))
                .ForMember(dest => dest.FromDate, opt => opt.MapFrom(src => src.FROM_DATE))
                .ForMember(dest => dest.ToDate, opt => opt.MapFrom(src => src.TO_DATE))
                .ForMember(dest => dest.DenyContent, opt => opt.MapFrom(src => src.DENY_CONTENT))
                .ForMember(dest => dest.ApprovedBy, opt => opt.MapFrom(src => src.APPROVED_BY))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.APPROVED_BYNavigation != null ? src.APPROVED_BYNavigation.FULLNAME : null))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.LRS_ID))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.LRS != null ? src.LRS.NAME : null));


            CreateMap<GPMS.INFRASTRUCTURE.DataContext.P_PART, GPMS.DOMAIN.Entities.ProductionPart>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PP_ID))
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.PRODUCTION_ID))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.PART_NAME))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.START_DATE))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.END_DATE))
                .ForMember(dest => dest.Cpu, opt => opt.MapFrom(src => src.CPU))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.PPS_ID))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.PPS != null ? src.PPS.NAME : null))
                .ReverseMap();
            // PRODUCTION PART
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.P_PART, GPMS.DOMAIN.Entities.ProductionPart>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PP_ID))
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.PRODUCTION_ID))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.PART_NAME))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.START_DATE))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.END_DATE))
                .ForMember(dest => dest.Cpu, opt => opt.MapFrom(src => src.CPU))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.PPS_ID))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.PPS != null ? src.PPS.NAME : null))
                .ReverseMap();

            // PRODUCTION 
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.PRODUCTION, GPMS.DOMAIN.Entities.Production>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PRODUCTION_ID))
                .ForMember(dest => dest.PmId, opt => opt.MapFrom(src => src.PM_ID))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ORDER_ID))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.P_START_DATE))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.P_END_DATE))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.PS_ID))
                .ReverseMap()
                .ForMember(dest => dest.PRODUCTION_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PM_ID, opt => opt.MapFrom(src => src.PmId))
                .ForMember(dest => dest.ORDER_ID, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.P_START_DATE, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.P_END_DATE, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.PS_ID, opt => opt.MapFrom(src => src.StatusId))
                ;



            //ORDER REJECT REASON
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ORDER_REJECT_REASON, GPMS.DOMAIN.Entities.OrderRejectReason>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ORR_ID))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ORDER_ID))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.REASON))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CREATED_AT))
                .ReverseMap();



            // PART WORK LOG
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.PART_WORK_LOG, GPMS.DOMAIN.Entities.ProductionPartWorkLog>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WL_ID))
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PP_ID))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QUANTITY))
                .ForMember(dest => dest.WorkDate, opt => opt.MapFrom(src => src.CREATE_DATE))
                .ForMember(dest => dest.IsReadOnly, opt => opt.MapFrom(src => src.IS_READ_ONLY ?? false))
                .ForMember(dest => dest.IsPayment, opt => opt.MapFrom(src => src.IS_PAYMENT ?? false))
                .ReverseMap()
                .ForMember(dest => dest.WL_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PP_ID, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.USER_ID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.QUANTITY, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.CREATE_DATE, opt => opt.MapFrom(src => src.WorkDate))
                .ForMember(dest => dest.IS_READ_ONLY, opt => opt.MapFrom(src => src.IsReadOnly))
                .ForMember(dest => dest.IS_PAYMENT, opt => opt.MapFrom(src => src.IsPayment));






        }
    }
}
