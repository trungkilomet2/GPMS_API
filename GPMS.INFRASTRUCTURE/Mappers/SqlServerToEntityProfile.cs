using AutoMapper;
using GPMS.APPLICATION.DTOs;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
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
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.MANAGER_ID))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.US_ID))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.ROLE))
                .ForMember(dest => dest.WorkerSkills, opt => opt.MapFrom(src => src.WS))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.US))
                .ReverseMap()
                .ForMember(dest => dest.MANAGER_ID, opt => opt.MapFrom(src => src.ManagerId == 0 ? (int?)null : src.ManagerId));

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

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_TEMPLATE, GPMS.DOMAIN.Entities.OrderTemplate>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.NAME))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TYPE))
                .ForMember(dest => dest.File, opt => opt.MapFrom(src => src.FILE))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.NOTE));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.ORDER_SIZE, GPMS.DOMAIN.Entities.OrderSize>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OD_ID))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ORDER_ID))
                .ForMember(dest => dest.SizeId, opt => opt.MapFrom(src => src.SIZE_ID))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.COLOR))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QUANTITY))
                .ForMember(dest => dest.OrderSizeStatusId, opt => opt.MapFrom(src => src.OSS_ID))
                .ReverseMap()
                .ForMember(dest => dest.OD_ID, opt => opt.MapFrom(src => src.Id));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_MATERIAL, GPMS.DOMAIN.Entities.OMaterial>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OM_ID))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ORDER_ID))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.COLOR))
                .ForMember(dest => dest.Uom, opt => opt.MapFrom(src => src.UOM))
                .ReverseMap()
                .ForMember(dest => dest.OM_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ORDER_ID, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.COLOR, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Uom));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.O_MATERIAL, GPMS.DOMAIN.Entities.OrderMaterial>()
                .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.NAME))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.COLOR))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.IMAGE))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.VALUE))
                .ForMember(dest => dest.Uom, opt => opt.MapFrom(src => src.UOM))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.NOTE));

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
                .ForMember(dest => dest.GuestId, opt => opt.MapFrom(src => src.GUEST_ID))
                .ForMember(dest => dest.OrderName, opt => opt.MapFrom(src => src.ORDER_NAME))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.IMAGE))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.START_DATE))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.END_DATE))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.TOTAL_QUANTITY))
                .ForMember(dest => dest.Cpu, opt => opt.MapFrom(src => src.CPU))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.NOTE))
                .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(src => src.CREATE_TIME))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.OS_ID))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.OS != null ? src.OS.NAME : null))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.ORDER_SIZE))
                .ForMember(dest => dest.Template, opt => opt.MapFrom(src => src.O_TEMPLATE))
                .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.O_MATERIAL))
                .ForMember(dest => dest.Histories, opt => opt.MapFrom(src => src.O_HISTORY_UPDATE));

            CreateMap<GPMS.DOMAIN.Entities.Order, GPMS.INFRASTRUCTURE.DataContext.ORDER>()
                .ForMember(dest => dest.ORDER_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.USER_ID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.GUEST_ID, opt => opt.MapFrom(src => src.GuestId))
                .ForMember(dest => dest.ORDER_NAME, opt => opt.MapFrom(src => src.OrderName))
                .ForMember(dest => dest.IMAGE, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.START_DATE, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.END_DATE, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.TOTAL_QUANTITY, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.CPU, opt => opt.MapFrom(src => src.Cpu ?? 0))
                .ForMember(dest => dest.NOTE, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.CREATE_TIME, opt => opt.MapFrom(src => src.CreateTime))
                .ForMember(dest => dest.OS_ID, opt => opt.MapFrom(src => src.Status));

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.SIZE, GPMS.DOMAIN.Entities.Size>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SIZE_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME)).ReverseMap();

            CreateMap<GPMS.INFRASTRUCTURE.DataContext.GUEST_ORDER, GPMS.DOMAIN.Entities.Guest>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GUEST_ID))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GUEST_NAME))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.GUEST_PHONE))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.GUEST_ADDRESS)).ReverseMap();

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
                .ForMember(dest => dest.Cpu, opt => opt.MapFrom(src => src.CPU))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.PPS_ID))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.PPS != null ? src.PPS.NAME : null))
                .ReverseMap();
            // PRODUCTION PART
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.P_PART, GPMS.DOMAIN.Entities.ProductionPart>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PP_ID))
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.PRODUCTION_ID))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.PART_NAME))
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
                .ForMember(dest => dest.PS_ID, opt => opt.MapFrom(src => src.StatusId));

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
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PPOS != null ? src.PPOS.PP_ID : 0))
                .ForMember(dest => dest.PartOrderSizeId, opt => opt.MapFrom(src => src.PPOS_ID))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.PPOS != null ? src.PPOS.SIZE : string.Empty))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.PPOS != null ? src.PPOS.COLOR : string.Empty))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QUANTITY))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CREATE_DATE))
                .ForMember(dest => dest.IsReadOnly, opt => opt.MapFrom(src => src.IS_READ_ONLY ?? false))
                .ForMember(dest => dest.IsPayment, opt => opt.MapFrom(src => src.IS_PAYMENT ?? false))
                .ReverseMap()
                .ForMember(dest => dest.WL_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PPOS_ID, opt => opt.MapFrom(src => src.PartOrderSizeId))
                .ForMember(dest => dest.USER_ID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.QUANTITY, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.CREATE_DATE, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.IS_READ_ONLY, opt => opt.MapFrom(src => src.IsReadOnly))
                .ForMember(dest => dest.IS_PAYMENT, opt => opt.MapFrom(src => src.IsPayment));

        // TrungNT Added - 22-03-26

            // PRODUCTION REJECT REASON
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.PRODUCTION_REJECT_REASON, GPMS.DOMAIN.Entities.ProductionRejectReason>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ORR_ID))
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.PRODUCTION_ID))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.REASON))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CREATED_AT))
                .ReverseMap()
                .ForMember(dest => dest.ORR_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PRODUCTION_ID, opt => opt.MapFrom(src => src.ProductionId))
                .ForMember(dest => dest.USER_ID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.REASON, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.CREATED_AT, opt => opt.MapFrom(src => src.CreatedAt));

            // PRODUCTION ISSUE LOG
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.PRODUCTION_ISSUE_LOG, GPMS.DOMAIN.Entities.ProductionIssueLog>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ISSUE_ID))
                .ForMember(dest => dest.PartOrderSizeId, opt => opt.MapFrom(src => src.PPOS_ID))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CREATED_BY))
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.ASSIGNED_TO))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QUANTITY))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.TITLE))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DESCRIPTION))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.PRIORITY ?? 2))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.IS_ID))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.IMAGE))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CREATED_AT))
                .ReverseMap()
                .ForMember(dest => dest.ISSUE_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PPOS_ID, opt => opt.MapFrom(src => src.PartOrderSizeId))
                .ForMember(dest => dest.CREATED_BY, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.ASSIGNED_TO, opt => opt.MapFrom(src => src.AssignedTo))
                .ForMember(dest => dest.QUANTITY, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.TITLE, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.DESCRIPTION, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.PRIORITY, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.IS_ID, opt => opt.MapFrom(src => src.StatusId))
                .ForMember(dest => dest.IMAGE, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.CREATED_AT, opt => opt.MapFrom(src => src.CreatedAt));

            // DELIVERY
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.DELIVERY, GPMS.DOMAIN.Entities.Delivery>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DELIVERY_ID))
                .ForMember(dest => dest.OrderSizeId, opt => opt.MapFrom(src => src.ORDER_SIZE_ID))
                .ForMember(dest => dest.DeliverQuantity, opt => opt.MapFrom(src => src.DELIVER_QUANTITY))
                .ForMember(dest => dest.DeliveredAt, opt => opt.MapFrom(src => src.SHIPPED_DATE))
                .ForMember(dest => dest.ReceivedDate, opt => opt.MapFrom(src => src.RECEIVED_DATE))
                .ForMember(dest => dest.DeliverStatusId, opt => opt.MapFrom(src => src.DS_ID))
                .ReverseMap()
                .ForMember(dest => dest.DELIVERY_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ORDER_SIZE_ID, opt => opt.MapFrom(src => src.OrderSizeId))
                .ForMember(dest => dest.DELIVER_QUANTITY, opt => opt.MapFrom(src => src.DeliverQuantity))
                .ForMember(dest => dest.SHIPPED_DATE, opt => opt.MapFrom(src => src.DeliveredAt))
                .ForMember(dest => dest.RECEIVED_DATE, opt => opt.MapFrom(src => src.ReceivedDate))
                .ForMember(dest => dest.DS_ID, opt => opt.MapFrom(src => src.DeliverStatusId));
          
            // CUTTING NOTE BOOK
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.CUTTING_NOTEBOOK, GPMS.DOMAIN.Entities.CuttingNotebook>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CP_ID))
                .ForMember(dest => dest.ProductionId, opt => opt.MapFrom(src => src.PRODUCTION_ID))
                .ForMember(dest => dest.MarkerLength, opt => opt.MapFrom(src => src.MARKER_LENGTH))
                .ForMember(dest => dest.FabricWidth, opt => opt.MapFrom(src => src.FABRIC_WIDTH))
                .ReverseMap()
                .ForMember(dest => dest.CP_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PRODUCTION_ID, opt => opt.MapFrom(src => src.ProductionId))
                .ForMember(dest => dest.MARKER_LENGTH, opt => opt.MapFrom(src => src.MarkerLength))
                .ForMember(dest => dest.FABRIC_WIDTH, opt => opt.MapFrom(src => src.FabricWidth));
            
            // CUTING NOTE BOOK LOG
            CreateMap<GPMS.INFRASTRUCTURE.DataContext.CUTTING_NOTEBOOK_LOG, GPMS.DOMAIN.Entities.CuttingNotebookLog>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CND_ID))
                .ForMember(dest => dest.NotebookId, opt => opt.MapFrom(src => src.CP_ID))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.USER_ID))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.COLOR))
                .ForMember(dest => dest.MeterPerKg, opt => opt.MapFrom(src => src.METER_PER_KG))
                .ForMember(dest => dest.Layer, opt => opt.MapFrom(src => src.LAYER))
                .ForMember(dest => dest.ProductQty, opt => opt.MapFrom(src => src.PRODUCT_QTY))
                .ForMember(dest => dest.AvgConsumption, opt => opt.MapFrom(src => src.AVG_CONSUMPTION))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.NOTE))
                .ForMember(dest => dest.DateCreate, opt => opt.MapFrom(src => src.DATE_CREATE))
                .ForMember(dest => dest.IsReadOnly, opt => opt.MapFrom(src => src.IS_READ_ONLY ?? false))
                .ForMember(dest => dest.IsPayment, opt => opt.MapFrom(src => src.IS_PAYMENT ?? false))
                .ReverseMap()
                .ForMember(dest => dest.CND_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CP_ID, opt => opt.MapFrom(src => src.NotebookId))
                .ForMember(dest => dest.USER_ID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.COLOR, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.METER_PER_KG, opt => opt.MapFrom(src => src.MeterPerKg))
                .ForMember(dest => dest.LAYER, opt => opt.MapFrom(src => src.Layer))
                .ForMember(dest => dest.PRODUCT_QTY, opt => opt.MapFrom(src => src.ProductQty))
                .ForMember(dest => dest.AVG_CONSUMPTION, opt => opt.MapFrom(src => src.AvgConsumption))
                .ForMember(dest => dest.NOTE, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.DATE_CREATE, opt => opt.MapFrom(src => src.DateCreate))
                .ForMember(dest => dest.IS_READ_ONLY, opt => opt.MapFrom(src => src.IsReadOnly))
                .ForMember(dest => dest.IS_PAYMENT, opt => opt.MapFrom(src => src.IsPayment));

            CreateMap<LOG_EVENTS, LogEvent>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.MESSAGE))
                .ForMember(dest => dest.MessageTemplate, opt => opt.MapFrom(src => src.MESSAGE_TEMPLATE))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.LEVEL))
                .ForMember(dest => dest.TimeStemp, opt => opt.MapFrom(src => src.TIMPESTAMP))
                .ForMember(dest => dest.Exception, opt => opt.MapFrom(src => src.EXCEPTION))
                .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.PROPERTIES)).ReverseMap();

            // Mapping part order size
            CreateMap<P_PART_ORDER_SIZE, ProductionPartOrderSize>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PPOS_ID))
                .ForMember(dest => dest.ProductionPartId, opt => opt.MapFrom(src => src.PP_ID))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.SIZE))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QUANTITY))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.COLOR))
                .ForMember(dest => dest.PartOrderSizeStatusId, opt => opt.MapFrom(src => src.PPOSS_ID))
                //      .ForMember(dest => dest.AssigneeIds, opt => opt.MapFrom(src => src.USER.Select(u=>u.USER_ID).ToList()))
                .ReverseMap()
                .ForMember(dest => dest.PPOS_ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PP_ID, opt => opt.MapFrom(src => src.ProductionPartId))
                .ForMember(dest => dest.SIZE, opt => opt.MapFrom(src => src.Size))
                .ForMember(dest => dest.QUANTITY, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.COLOR, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.PPOSS_ID, opt => opt.MapFrom(src => src.PartOrderSizeStatusId));
            //    .ForMember(dest => dest.USER.Select(u => u.USER_ID).ToList(), opt => opt.MapFrom(src => src.AssigneeIds));


        }
    }
}
