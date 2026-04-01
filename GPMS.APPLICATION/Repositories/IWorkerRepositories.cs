using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IWorkerRepositories
    {
        Task<IEnumerable<User>> GetAllEmployees();
        Task<IEnumerable<User>> GetAllEmployeesByPMId(int id);
        Task<User> GetEmployeeById(int id);
        Task<User> CreateEmployee(User user);
        Task<User> UpdateEmployee(int userId, User user);
    }
}
