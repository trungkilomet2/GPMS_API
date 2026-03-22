using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Http;
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
        private readonly Mock<IUserRepositories> _userRepo = new();

        private EmailController BuildController()
        {
            return new EmailController(_cache.Object, _emailRepo.Object, _logger.Object, _userRepo.Object);
        }


        [Fact]
        public async Task SendOTPEmail_Returns200_WhenEmailValid()
        {
            var controller = BuildController();

            _userRepo.Setup(x => x.IsEmailExists(It.IsAny<string>()))
                     .ReturnsAsync(true);

            var input = new VerifyEmailDTO
            {
                Email = "test@gmail.com"
            };

            var result = await controller.SendOTPEmail(input);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, obj.StatusCode);

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

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        }

        [Fact]
        public async Task SendOTPEmail_Returns409_WhenEmailExists()
        {
            var controller = BuildController();

            _userRepo.Setup(x => x.IsEmailExists(It.IsAny<string>())).ReturnsAsync(true);

            var input = new VerifyEmailDTO
            {
                Email = "test@gmail.com"
            };

            var result = await controller.SendOTPEmail(input);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, obj.StatusCode);
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

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        }


        [Fact]
        public void VerifyEmail_Returns404_WhenOtpNotFound()
        {
            var controller = BuildController();

            var result = controller.VerifyEmail(new VerifyOtpDTO
            {
                Email = "test@gmail.com",
                Otp = "123456"
            });

            var obj = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, obj.StatusCode);
        }

        [Fact]
        public void VerifyEmail_Returns409_WhenAlreadyVerified()
        {
            var controller = BuildController();

            var email = "test@gmail.com";
            var otp = "123456";

            object cacheOtp = "123456";
            object cacheVerified = true;

            _cache.Setup(x => x.TryGetValue($"{email}_otp", out cacheOtp))
                  .Returns(true);

            _cache.Setup(x => x.TryGetValue($"{email}_verified", out cacheVerified))
                  .Returns(true);

            var result = controller.VerifyEmail(new VerifyOtpDTO
            {
                Email = email,
                Otp = otp
            });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, obj.StatusCode);
        }
    }
}
