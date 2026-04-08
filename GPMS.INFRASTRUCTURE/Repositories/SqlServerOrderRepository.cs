using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerOrderRepository : IBaseOrderRepositories, IBaseOrderStatusRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerOrderRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<Order>> GetAll(object? obj)
        {
            if (obj is int userId)
            {
                var filtered = await _context.ORDER
                    .Include(o => o.OS)
                    .Where(o => o.USER_ID == userId)
                    .ToListAsync();
                return _mapper.Map<IEnumerable<Order>>(filtered);
            }

            var data = await _context.ORDER.Include(o => o.OS).ToListAsync();
            return _mapper.Map<IEnumerable<Order>>(data);
        }

        public async Task<Order> GetById(object id)
        {
            var data = await _context.ORDER
                .Include(o => o.OS)
                .Include(o => o.USER)
                .Include(o => o.O_TEMPLATE)
                .Include(o => o.O_MATERIAL)
                .Include(o => o.O_HISTORY_UPDATE)
                .Include(o => o.ORDER_SIZE)
                .Where(o => o.ORDER_ID == (int)id)
                .FirstOrDefaultAsync();
            return _mapper.Map<Order>(data);
        }

        public async Task<Order> CreateManualOrder(Order order)
        {
            var orderEntity = _mapper.Map<ORDER>(order);
            await _context.ORDER.AddAsync(orderEntity);

            if (order.Size != null)
            {
                foreach (var m in order.Size)
                {
                    var sizeExists = await _context.SIZE.AnyAsync(x => x.SIZE_ID == m.SizeId);

                    if (!sizeExists)
                    {
                        throw new Exception($"SizeId {m.SizeId} không tồn tại");
                    }
                    await _context.ORDER_SIZE.AddAsync(new ORDER_SIZE
                    {
                        ORDER = orderEntity,
                        ORDER_ID = orderEntity.ORDER_ID,
                        SIZE_ID = m.SizeId,
                        COLOR = m.Color,
                        QUANTITY = m.Quantity,
                        OSS_ID = m.OrderSizeStatusId,
                    });
                }
            }
            if (order.Material != null)
            {
                foreach (var m in order.Material)
                {
                    await _context.O_MATERIAL.AddAsync(new O_MATERIAL
                    {
                        ORDER = orderEntity,
                        ORDER_ID = orderEntity.ORDER_ID,
                        NAME = m.MaterialName,
                        COLOR = m.Color,
                        IMAGE = m.Image,
                        VALUE = m.Value,
                        UOM = m.Uom,
                        NOTE = m.Note
                    });
                }
            }

            if (order.Template != null)
            {
                foreach (var t in order.Template)
                {
                    await _context.O_TEMPLATE.AddAsync(new O_TEMPLATE
                    {
                        ORDER = orderEntity,
                        ORDER_ID = orderEntity.ORDER_ID,
                        NAME = t.TemplateName,
                        TYPE = t.Type,
                        FILE = t.File,
                        NOTE = t.Note
                    });
                }
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(orderEntity);
        }

        public async Task<Order> Create(Order entity)
        {
            var orderEntity = _mapper.Map<ORDER>(entity);
            await _context.ORDER.AddAsync(orderEntity);
            await _context.SaveChangesAsync();

            if (entity.Size != null)
            {
                foreach (var m in entity.Size)
                {
                    var sizeExists = await _context.SIZE.AnyAsync(x => x.SIZE_ID == m.SizeId);

                    if (!sizeExists)
                    {
                        throw new Exception($"SizeId {m.SizeId} không tồn tại");
                    }
                    await _context.ORDER_SIZE.AddAsync(new ORDER_SIZE
                    {
                        ORDER_ID = orderEntity.ORDER_ID,
                        SIZE_ID = m.SizeId,
                        COLOR = m.Color,
                        QUANTITY = m.Quantity,
                        OSS_ID = m.OrderSizeStatusId,
                    });
                }
            }
            if (entity.Material != null)
            {
                foreach (var m in entity.Material)
                {
                    await _context.O_MATERIAL.AddAsync(new O_MATERIAL
                    {
                        ORDER_ID = orderEntity.ORDER_ID,
                        NAME = m.MaterialName,
                        COLOR = m.Color,
                        IMAGE = m.Image,
                        VALUE = m.Value,
                        UOM = m.Uom,
                        NOTE = m.Note
                    });
                }
            }

            if (entity.Template != null)
            {
                foreach (var t in entity.Template)
                {
                    await _context.O_TEMPLATE.AddAsync(new O_TEMPLATE
                    {
                        ORDER_ID = orderEntity.ORDER_ID,
                        NAME = t.TemplateName,
                        TYPE = t.Type,
                        FILE = t.File,
                        NOTE = t.Note
                    });
                }
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(orderEntity);
        }

        public async Task<Order> UpdateOrder(int orderId, Order updatedOrder, List<OHistoryUpdate> histories)
        {
            var existing = await _context.ORDER
                .Include(o => o.O_TEMPLATE)
                .Include(o => o.O_MATERIAL)
                .Include(o => o.ORDER_SIZE)
                .Include(o => o.OS)
                .FirstOrDefaultAsync(o => o.ORDER_ID == orderId);

            if (existing is null)
                throw new KeyNotFoundException($"Order '{orderId}' not exist");

            existing.ORDER_NAME = updatedOrder.OrderName;
            existing.START_DATE = updatedOrder.StartDate;
            existing.END_DATE = updatedOrder.EndDate;
            existing.TOTAL_QUANTITY = updatedOrder.Quantity;
            existing.IMAGE = updatedOrder.Image;
            existing.NOTE = updatedOrder.Note;

            var pendingStatus = await _context.O_STATUS
                .FirstOrDefaultAsync(s => s.NAME == OrderStatus_Constants.Pending);
            if (pendingStatus is null)
                throw new InvalidOperationException($"Status '{OrderStatus_Constants.Pending}' not exist in system");

            existing.OS_ID = pendingStatus.OS_ID;

            _context.O_TEMPLATE.RemoveRange(existing.O_TEMPLATE);
            foreach (var t in updatedOrder.Template ?? Enumerable.Empty<OrderTemplate>())
            {
                await _context.O_TEMPLATE.AddAsync(new O_TEMPLATE
                {
                    ORDER_ID = orderId,
                    NAME = t.TemplateName,
                    TYPE = t.Type,
                    FILE = t.File,
                    NOTE = t.Note
                });
            }

            _context.O_MATERIAL.RemoveRange(existing.O_MATERIAL);
            foreach (var m in updatedOrder.Material ?? Enumerable.Empty<OrderMaterial>())
            {
                await _context.O_MATERIAL.AddAsync(new O_MATERIAL
                {
                    ORDER_ID = orderId,
                    NAME = m.MaterialName,
                    COLOR = m.Color,
                    IMAGE = m.Image,
                    VALUE = m.Value,
                    UOM = m.Uom,
                    NOTE = m.Note
                });
            }

            _context.ORDER_SIZE.RemoveRange(existing.ORDER_SIZE);
            foreach (var s in updatedOrder.Size ?? Enumerable.Empty<OrderSize>())
            {
                await _context.ORDER_SIZE.AddAsync(new ORDER_SIZE
                {
                    ORDER_ID = orderId,
                    SIZE_ID = s.SizeId,
                    COLOR = s.Color,
                    QUANTITY = s.Quantity,
                    OSS_ID = s.OrderSizeStatusId
                });
            }

            foreach (var history in histories)
            {
                await _context.O_HISTORY_UPDATE.AddAsync(new O_HISTORY_UPDATE
                {
                    ORDER_ID = orderId,
                    FIELD_NAME = history.FieldName,
                    OLD_VALUE = history.OldValue,
                    NEW_VALUE = history.NewValue
                });
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(existing);
        }

        public Task<Order> Update(Order entity)
        {
            throw new NotImplementedException();
        }
        public Task Delete(object id) => throw new NotImplementedException();

        public async Task<Order> RequestOrderModification(int orderId, Order updatedOrder, List<OHistoryUpdate> histories)
        {
            var existing = await _context.ORDER
                .Include(o => o.O_HISTORY_UPDATE)
                .FirstOrDefaultAsync(o => o.ORDER_ID == orderId);
            if(existing == null)
                throw new KeyNotFoundException($"Order '{orderId}' not exist");
            existing.OS_ID = updatedOrder.Status;
            foreach (var history in histories)
            {
                await _context.O_HISTORY_UPDATE.AddAsync(new O_HISTORY_UPDATE
                {
                    ORDER_ID = orderId,
                    FIELD_NAME = history.FieldName,
                    OLD_VALUE = history.OldValue,
                    NEW_VALUE = history.NewValue
                });
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(existing);
        }

        public async Task<Order> ChangeStatus(int orderId, int newStatus)
        {
            var existing = await _context.ORDER.FirstOrDefaultAsync(o => o.ORDER_ID == orderId);
            if (existing == null)
                throw new KeyNotFoundException($"Order '{orderId}' not exist");
            existing.OS_ID = newStatus;
            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(existing);
        }

        public async Task<Order> DenyOrder(int orderId, Order updatedOrder, List<OHistoryUpdate> histories)
        {
            var existing = await _context.ORDER
                .Include(o => o.O_HISTORY_UPDATE)
                .FirstOrDefaultAsync(o => o.ORDER_ID == orderId);
            if (existing == null)
                throw new KeyNotFoundException($"Order '{orderId}' not exist");
            existing.OS_ID = updatedOrder.Status;
            foreach (var history in histories)
            {
                await _context.O_HISTORY_UPDATE.AddAsync(new O_HISTORY_UPDATE
                {
                    ORDER_ID = orderId,
                    FIELD_NAME = history.FieldName,
                    OLD_VALUE = history.OldValue,
                    NEW_VALUE = history.NewValue
                });
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(existing);
        }

        public async Task<Order> ApproveOrder(int orderId, Order updatedOrder, List<OHistoryUpdate> histories)
        {
            var existing = await _context.ORDER
                .Include(o => o.O_HISTORY_UPDATE)
                .FirstOrDefaultAsync(o => o.ORDER_ID == orderId);
            if (existing == null)
                throw new KeyNotFoundException($"Order '{orderId}' not exist");
            existing.OS_ID = updatedOrder.Status;
            foreach (var history in histories)
            {
                await _context.O_HISTORY_UPDATE.AddAsync(new O_HISTORY_UPDATE
                {
                    ORDER_ID = orderId,
                    FIELD_NAME = history.FieldName,
                    OLD_VALUE = history.OldValue,
                    NEW_VALUE = history.NewValue
                });
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(existing);
        }
        
    }
}