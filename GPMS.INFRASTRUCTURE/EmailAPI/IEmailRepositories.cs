using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.EmailAPI
{
    public enum EmailType
    {
        Verification = 1,
        PasswordReset = 2,
        OrderNotification = 3
    }
    public interface IEmailRepositories
    {
        Task SendEmailAsync(string toEmail, string subject, string body, EmailType emailType);
    }
}
