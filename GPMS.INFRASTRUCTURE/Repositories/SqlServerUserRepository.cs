using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
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
            var data = await _context.USER.Where(u => u.USERNAME.Equals(username)).FirstOrDefaultAsync();

            if (data is null) return null;

            return _mapper.Map<User>(data);
        }

        public async Task<IEnumerable<User>> GetAll(object? obj)
        {
            var data = await _context.USER
                .Include(u => u.ROLE)
                .Include(u => u.US)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GPMS.DOMAIN.Entities.User>>(data);
        }

        public async Task<User> GetById(object id)
        {
            var data = await _context.USER.Include(u => u.ROLE).Include(u => u.US)
                              .Include(u => u.WS).Where(u => u.USER_ID == (int)id).FirstOrDefaultAsync();
            return _mapper.Map<User>(data);
        }

        public async Task<User> GetOwner()
        {
            var data = await _context.USER.Include(u => u.ROLE).Include(u => u.US)
                              .Include(u => u.WS).Where(u => u.ROLE.Any(r => r.NAME == Roles_Constants.Owner)).FirstOrDefaultAsync();
            return _mapper.Map<User>(data);
        }

        public async Task<User> GetUserByMail(string mail)
        {
            var data = await _context.USER.Where(u => u.EMAIL.Equals(mail.ToLower())).FirstOrDefaultAsync();
            return _mapper.Map<User>(data);
        }

        public async Task<User> Login(string UserName, string password)
        {
            var data = await _context.USER.Where(u => u.USERNAME.Equals(UserName) && u.PASSWORDHASH.Equals(password)).FirstOrDefaultAsync();

            return _mapper.Map<User>(data);
        }

        public async Task<User> Register(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            User existUser = await FindUserByUserName(user.UserName);
            if (existUser is not null)
            {
                throw new DbUpdateException("Tên tài khoản đã tồn tại.");
            }
            var existEmail = await GetUserByMail(user.Email);
            if(existEmail is not null)
            {
                throw new DbUpdateException("Tên email đã tồn tại trong hệ thống.");
            }
            try
            {   
                USER userSQL = _mapper.Map<USER>(user);
                userSQL.US_ID = UserStatus_Constants.Active;
                userSQL.MANAGER_ID = null;
                await _context.USER.AddAsync(userSQL);
                return _mapper.Map<User>(userSQL);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> Update(User entity)
        {
            var existingUser = await _context.USER.FindAsync(entity.Id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID {entity.Id} not found");
            }
                existingUser.FULLNAME = entity.FullName;
                existingUser.PHONE_NUMBER = entity.PhoneNumber;
                existingUser.LOCATION = entity.Location;
                existingUser.AVATAR = entity.AvartarUrl;
                existingUser.EMAIL = entity.Email;
                existingUser.US_ID = entity.StatusId;
                _context.USER.Update(existingUser);
                await _context.SaveChangesAsync();
            return _mapper.Map<User>(existingUser);
        }

        public async Task UpdatePassword(string email, string newPasswordHash)
        {
            var user = await _context.USER.Where(u => u.EMAIL == email.Trim().ToLower()).FirstOrDefaultAsync();
            if (user == null)
                throw new KeyNotFoundException($"User with email '{email}' not found.");
            user.PASSWORDHASH = newPasswordHash;
            _context.USER.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
