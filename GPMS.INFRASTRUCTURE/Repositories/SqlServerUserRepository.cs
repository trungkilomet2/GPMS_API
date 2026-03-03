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

        public async Task<User> Create(User entity)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public Task<User> FindUserByPhoneNumber(string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<User> FindUserByUserName(string username)
        {
            var data = await _context.USER.Where(u => u.UserName.Equals(username)).FirstOrDefaultAsync();
            
            if(data is null) return null;
         
            return _mapper.Map<User>(data);
        }

        public async Task<IEnumerable<User>> GetAll(object? obj)
        {

            var data = await _context.USER.ToListAsync();

            return _mapper.Map<IEnumerable<GPMS.DOMAIN.Entities.User>>(data);
        }

        public async Task<User> GetById(object id)
        {
            var data = await _context.USER.FindAsync(id);
            return _mapper.Map<User>(data);
        }

        public async Task<User> Login(string UserName, string password)
        {
            var data = await _context.USER.Where(u => u.UserName.Equals(UserName) && u.PASSWORDHASH.Equals(password)).FirstOrDefaultAsync();
         
            return _mapper.Map<User>(data);
        }

        public Task<User> Register(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<User> Update(User entity)
        {
            var existingUser = await _context.USER.FindAsync(entity.Id);
            if (existingUser != null)
            {
                existingUser.UserName = entity.UserName;
                existingUser.PASSWORDHASH = entity.PasswordHash;
                existingUser.FULLNAME = entity.FullName;
                existingUser.PHONE_NUMBER = entity.PhoneNumber;
                existingUser.AVATAR = entity.AvartarUrl;
                existingUser.EMAIL = entity.Email;
            }
                _context.USER.Update(existingUser);
                await _context.SaveChangesAsync();
                return _mapper.Map<User>(existingUser);       
        }
    }
}
