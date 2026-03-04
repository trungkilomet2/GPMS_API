using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerUserRepository : IBaseRepositories<User>, IBaseAccountRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerUserRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //Create User in SQL
        public async Task<User> Create(User entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            User existUser = await FindUserByUserName(entity.UserName);

            if(existUser is not null)
            {
                throw new Exception("Tên tài khoản đã tồn tài");
            }

            USER userSQL = _mapper.Map<USER>(entity);
            await _context.AddAsync(userSQL);
            await _context.SaveChangesAsync();
            return _mapper.Map<User>(userSQL);

        }

        public async Task Delete(object id)
        {
            throw new NotImplementedException();
        }


        public async Task<User> FindUserByUserName(string username)
        {
            var data = await _context.USER.Where(u => u.UserName.Equals(username)).FirstOrDefaultAsync();

            if (data is null) return null;

            return _mapper.Map<User>(data);
        }

        public async Task<IEnumerable<User>> GetAll(object? obj)
        {

            var data = await _context.USER.ToListAsync();

            return _mapper.Map<IEnumerable<GPMS.DOMAIN.Entities.User>>(data);
        }

        public async Task<User> GetById(object id)
        {
            var data = await _context.USER.Where(u => u.USER_ID == (int)id).FirstOrDefaultAsync();
            return _mapper.Map<User>(data);
        }

        public async Task<User> Login(string UserName, string password)
        {
            var data = await _context.USER.Where(u => u.UserName.Equals(UserName) && u.PASSWORDHASH.Equals(password)).FirstOrDefaultAsync();

            return _mapper.Map<User>(data);
        }

        public async Task<User> Register(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            User existUser = await FindUserByUserName(user.UserName);
            if (existUser is not null)
            {
                throw new DbUpdateException("Tên tài khoản đã tồn tại");
            }
            try
            {
                USER userSQL = _mapper.Map<USER>(user);
                userSQL.US_ID = 1; // 1 equal Active status in User
                await _context.AddAsync(userSQL);
                await _context.SaveChangesAsync();
                return _mapper.Map<User>(userSQL);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
