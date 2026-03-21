using GPMS.APPLICATION.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace GPMS.INFRASTRUCTURE.EmailAPI
{
    public class EmailService : IEmailRepositories
    {
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        public EmailService(IConfiguration config, IMemoryCache memoryCache)
        {
            _config = config;
            _memoryCache = memoryCache;
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, EmailType emailType)
        {
            var smtpClient = new SmtpClient(_config["Email:Smtp"])
            {
                Port = int.Parse(_config["Email:Port"]),
                Credentials = new NetworkCredential(
                    _config["Email:Username"],
                    _config["Email:Password"]
                ),
                EnableSsl = true
            };
            switch (emailType)
            {
                case EmailType.Verification:
                    var otp = GenerateOtp();
                    _memoryCache.Set(toEmail + "_otp", otp, TimeSpan.FromMinutes(5));
                    subject = "Xác nhận email";
                    body = $"Mã OTP của bạn là: <b>{otp}</b>. Có hiệu lực 5 phút.";
                    break;
                case EmailType.PasswordReset:
                    subject = "Password Reset Request";
                    break;
                case EmailType.OrderNotification:
                    subject = "Order Notification";
                    break;
                default:
                    subject = "General Notification";
                    break;
            }

            var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await smtpClient.SendMailAsync(mail);
        }

    }
}
