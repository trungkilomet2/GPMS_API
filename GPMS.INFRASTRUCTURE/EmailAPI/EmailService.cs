using GPMS.APPLICATION.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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
            return System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, EmailType emailType)
        {
            var normalizedEmail = toEmail.Trim().ToLower();
            string? generatedOtp = null;
            string? cacheKey = null;

            switch (emailType)
            {
                case EmailType.Verification:
                    generatedOtp = GenerateOtp();
                    cacheKey = normalizedEmail + "_otp";
                    subject = "Xác nhận email";
                    body = $"Mã OTP của bạn là: <b>{generatedOtp}</b>. Có hiệu lực 5 phút.";
                    break;
                case EmailType.ResendOTP:
                    _memoryCache.Remove(normalizedEmail + "_otp");
                    generatedOtp = GenerateOtp();
                    cacheKey = normalizedEmail + "_otp";
                    subject = "Gửi lại mã OTP";
                    body = $"Mã OTP mới của bạn là: <b>{generatedOtp}</b>. Có hiệu lực 5 phút.";
                    break;
                case EmailType.PasswordReset:
                    generatedOtp = GenerateOtp();
                    cacheKey = normalizedEmail + "_reset_otp";
                    subject = "Yêu cầu đặt lại mật khẩu";
                    body = $"Mã OTP đặt lại mật khẩu của bạn là: <b>{generatedOtp}</b>. Có hiệu lực 5 phút.";
                    break;
                case EmailType.OrderNotification:
                    subject = "Order Notification";
                    break;
                default:
                    subject = "General Notification";
                    break;
            }

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Email:Smtp"],
                int.Parse(_config["Email:Port"]!),
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            if (generatedOtp != null && cacheKey != null)
            {
                _memoryCache.Set(cacheKey, generatedOtp, TimeSpan.FromMinutes(5));
            }
        }
    }
}

