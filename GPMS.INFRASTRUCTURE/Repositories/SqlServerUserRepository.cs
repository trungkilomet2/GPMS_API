using AutoMapper;
using GPMS.APPLICATION.Abstractions;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerUserRepository : IUserInterface
    {
        private readonly GPMS_SYSTEMContext context;
        private readonly IMapper mapper;

        public SqlServerUserRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


         async Task<IEnumerable<DOMAIN.Entities.User>> IUserInterface.GetUser()
        {
            var data = await context.USER.ToListAsync();

            return mapper.Map<IEnumerable<GPMS.DOMAIN.Entities.User>>(data);
        }
        //a 
    }
}
