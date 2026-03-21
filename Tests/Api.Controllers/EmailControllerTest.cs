using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.TEST.Api.Controllers
{
    public class EmailControllerTest
    {
        private readonly Mock<IEmailRepositories> _emailRepo = new();
        private readonly Mock<IMemoryCache> _cache = new();
        private readonly Mock<ILogger<EmailController>> _logger = new();

        private EmailController BuildController()
        {
            return new EmailController(_cache.Object, _emailRepo.Object, _logger.Object);
        }


        [Fact]
        public async Task SendOTPEmail_Returns200_WhenEmailValid()
        {
            var controller = BuildController();

            var input = new VerifyEmailDTO
            {
                Email = "test@gmail.com"
            };

            var result = await controller.SendOTPEmail(input);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, obj.StatusCode);

            _emailRepo.Verify(x => x.SendEmailAsync(
                input.Email,
                null,
                null,
                EmailType.Verification
            ), Times.Once);
        }

        [Fact]
        public async Task SendOTPEmail_Returns400_WhenEmailNull()
        {
            var controller = BuildController();

            var result = await controller.SendOTPEmail(null);

            var obj = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task SendOTPEmail_Returns400_WhenEmailEmpty()
        {
            var controller = BuildController();

            var input = new VerifyEmailDTO
            {
                Email = ""
            };

            var result = await controller.SendOTPEmail(input);

            var obj = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }


        [Fact]
        public void VerifyEmail_Returns400_WhenOtpNotFound()
        {
            var controller = BuildController();

            object cacheValue = null;

            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                  .Returns(false);

            var input = new VerifyOtpDTO
            {
                Email = "test@gmail.com",
                Otp = "123456"
            };

            var result = controller.VerifyEmail(input);

            var obj = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public void VerifyEmail_Returns400_WhenOtpInvalid()
        {
            var controller = BuildController();

            var email = "test@gmail.com";
            object cacheValue = "999999";

            _cache.Setup(x => x.TryGetValue($"{email}_otp", out cacheValue))
                  .Returns(true);

            var input = new VerifyOtpDTO
            {
                Email = email,
                Otp = "123456"
            };

            var result = controller.VerifyEmail(input);

            var obj = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public void VerifyEmail_Returns200_WhenOtpValid()
        {
            var controller = BuildController();

            var email = "test@gmail.com";
            var otp = "123456";

            object cacheValue = otp;

            _cache.Setup(x => x.TryGetValue($"{email}_otp", out cacheValue))
                  .Returns(true);

            var input = new VerifyOtpDTO
            {
                Email = email,
                Otp = otp
            };

            var result = controller.VerifyEmail(input);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, obj.StatusCode);

            _cache.Verify(x => x.Remove($"{email}_otp"), Times.Once);
            _cache.Verify(x => x.Set(
                $"{email}_verified",
                true,
                It.IsAny<TimeSpan>()
            ), Times.Once);
        }
    }
}
