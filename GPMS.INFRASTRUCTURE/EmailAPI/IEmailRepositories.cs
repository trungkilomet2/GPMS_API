using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.EmailAPI
{
    public interface IEmailRepositories
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
