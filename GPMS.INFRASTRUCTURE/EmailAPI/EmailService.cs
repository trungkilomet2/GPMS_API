using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
                    subject = string.IsNullOrEmpty(subject) ? "Order Notification" : subject;
                    break;
                default:
                    subject = string.IsNullOrEmpty(subject) ? "General Notification" : subject;
                    break;
            }

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_config["Gmail:FromEmail"]));
            mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var memStream = new MemoryStream();
            await mimeMessage.WriteToAsync(memStream);
            var rawMessage = Convert.ToBase64String(memStream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            var credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = _config["Gmail:ClientId"],
                            ClientSecret = _config["Gmail:ClientSecret"]
                        },
                        Scopes = new[] { GmailService.Scope.GmailSend }
                    }),
                "user",
                new TokenResponse
                {
                    RefreshToken = _config["Gmail:RefreshToken"]
                }
            );

            // Tạo Gmail API service
            var gmailService = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "GPMS"
            });

            // Gửi email
            var gmailMessage = new Message { Raw = rawMessage };
            await gmailService.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();

            // Lưu OTP vào cache nếu có
            if (generatedOtp != null && cacheKey != null)
            {
                _memoryCache.Set(cacheKey, generatedOtp, TimeSpan.FromMinutes(5));
            }
        }
    }
}
