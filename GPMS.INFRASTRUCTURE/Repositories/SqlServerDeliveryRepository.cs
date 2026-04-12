using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerDeliveryRepository : IBaseRepositories<Delivery>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerDeliveryRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Delivery> Create(Delivery entity)
        {
            var db = _mapper.Map<DELIVERY>(entity);
            await _context.DELIVERY.AddAsync(db);
            await _context.SaveChangesAsync();
            return _mapper.Map<Delivery>(db);
        }

        public async Task Delete(object id)
        {
            if (id is not int deliveryId) return;
            var db = await _context.DELIVERY.FirstOrDefaultAsync(x => x.DELIVERY_ID == deliveryId);
            if (db is null) return;
            _context.DELIVERY.Remove(db);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Delivery>> GetAll(object? obj)
        {
            IQueryable<DELIVERY> query = _context.DELIVERY;
            if (obj is int userId)
            {
                query = query.Where(x => x.ORDER_SIZE.ORDER.USER_ID == userId);
            }
            return _mapper.Map<IEnumerable<Delivery>>(await query.OrderByDescending(x => x.SHIPPED_DATE).ToListAsync());
        }

        public async Task<Delivery> GetById(object id)
        {
            if (id is not int deliveryId) return null!;
            var db = await _context.DELIVERY.FirstOrDefaultAsync(x => x.DELIVERY_ID == deliveryId);
            return db is null ? null! : _mapper.Map<Delivery>(db);
        }

        public async Task<Delivery> Update(Delivery entity)
        {
            var db = await _context.DELIVERY.FirstOrDefaultAsync(x => x.DELIVERY_ID == entity.Id);
            if (db is null) throw new KeyNotFoundException("Delivery not found");
            db.DELIVER_QUANTITY = entity.DeliverQuantity;
            db.SHIPPED_DATE = entity.DeliveredAt;
            db.RECEIVED_DATE = entity.ReceivedDate;
            db.DS_ID = entity.DeliverStatusId;
            await _context.SaveChangesAsync();
            return _mapper.Map<Delivery>(db);
        }
    }
}
