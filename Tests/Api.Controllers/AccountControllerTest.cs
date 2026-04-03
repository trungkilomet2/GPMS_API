using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GPMS.TEST.Api.Controllers;

public class AccountControllerTest
{
    private readonly Mock<IAccountRepositories> _accountRepo = new();
    private readonly Mock<IBaseAccountRepositories> _accountBaseRepo = new();
    private readonly Mock<IEmailRepositories> _emailRepo = new();
    private readonly Mock<ILogger<AccountController>> _logger = new();
    private readonly Mock<IMemoryCache> _memoryCache = new();
    private readonly IConfiguration _configuration;
    private readonly AccountController _controller;

    public AccountControllerTest()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JWT:SigningKey"] = "this-is-a-test-signing-key-1234567890",
            ["JWT:Issuer"] = "test-issuer",
            ["JWT:Audience"] = "test-audience"
        }).Build();

        _controller = new AccountController(
            _accountRepo.Object,
            _accountBaseRepo.Object,
            _configuration,
            _logger.Object,
            _emailRepo.Object,
            _memoryCache.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Login_ReturnsNotFound_WhenCredentialsInvalid()
    {
        _accountRepo.Setup(x => x.Login("user", "wrong")).ReturnsAsync((GPMS.APPLICATION.DTOs.LoginDTO)null!);

        var result = await _controller.Login(new LoginDTO { UserName = "user", Password = "wrong" });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsJwt_WhenCredentialsValid()
    {
        _accountRepo.Setup(x => x.Login("user", "pass")).ReturnsAsync(new GPMS.APPLICATION.DTOs.LoginDTO
        {
            User = new User { UserName = "user" },
            UserRole = [new Role { Name = "Customer" }]
        });

        var result = await _controller.Login(new LoginDTO { UserName = "user", Password = "pass" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.IsType<string>(objectResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
    {
        var result = await _controller.Register(new RegisterDTO
        {
            UserName = "newuser",
            FullName = "New User",
            Password = "123456",
            RePassword = "654321"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── ForgotPassword ────────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_Returns200_WhenEmailExists()
    {
        var user = new User { Email = "test@example.com" };
        _accountBaseRepo.Setup(x => x.GetUserByMail("test@example.com")).ReturnsAsync(user);
        _emailRepo.Setup(x => x.SendEmailAsync("test@example.com", string.Empty, string.Empty, EmailType.PasswordReset))
                  .Returns(Task.CompletedTask);

        var result = await _controller.ForgotPassword(new ForgotPasswordDTO { Email = "test@example.com" });

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, obj.StatusCode);
        _emailRepo.Verify(x => x.SendEmailAsync("test@example.com", string.Empty, string.Empty, EmailType.PasswordReset), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_Returns200_WhenEmailNotFound()
    {
        _accountBaseRepo.Setup(x => x.GetUserByMail(It.IsAny<string>())).ReturnsAsync((User)null!);

        var result = await _controller.ForgotPassword(new ForgotPasswordDTO { Email = "notfound@example.com" });

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, obj.StatusCode);
        _emailRepo.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_Returns400_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Email không hợp lệ");

        var result = await _controller.ForgotPassword(new ForgotPasswordDTO { Email = "bad-email" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ForgotPassword_Returns500_WhenExceptionThrown()
    {
        _accountBaseRepo.Setup(x => x.GetUserByMail(It.IsAny<string>())).ThrowsAsync(new Exception("DB error"));

        var result = await _controller.ForgotPassword(new ForgotPasswordDTO { Email = "test@example.com" });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_Returns200_WhenSuccess()
    {
        var email = "test@example.com";
        var otp = "123456";
        object cachedOtp = otp;
        _memoryCache.Setup(x => x.TryGetValue(email + "_reset_otp", out cachedOtp)).Returns(true);
        _accountBaseRepo.Setup(x => x.GetUserByMail(email)).ReturnsAsync(new User { Email = email });
        _accountBaseRepo.Setup(x => x.UpdatePassword(email, It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _controller.ResetPassword(new ResetPasswordDTO
        {
            Email = email,
            Otp = otp,
            NewPassword = "newpass123",
            ConfirmPassword = "newpass123"
        });

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, obj.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_Returns400_WhenPasswordMismatch()
    {
        var result = await _controller.ResetPassword(new ResetPasswordDTO
        {
            Email = "test@example.com",
            Otp = "123456",
            NewPassword = "newpass123",
            ConfirmPassword = "different"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Returns400_WhenOtpInvalid()
    {
        var email = "test@example.com";
        object? cachedOtp = null;
        _memoryCache.Setup(x => x.TryGetValue(email + "_reset_otp", out cachedOtp)).Returns(false);

        var result = await _controller.ResetPassword(new ResetPasswordDTO
        {
            Email = email,
            Otp = "wrongotp",
            NewPassword = "newpass123",
            ConfirmPassword = "newpass123"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Returns404_WhenUserNotFound()
    {
        var email = "test@example.com";
        var otp = "123456";
        object cachedOtp = otp;
        _memoryCache.Setup(x => x.TryGetValue(email + "_reset_otp", out cachedOtp)).Returns(true);
        _accountBaseRepo.Setup(x => x.GetUserByMail(email)).ReturnsAsync((User)null!);

        var result = await _controller.ResetPassword(new ResetPasswordDTO
        {
            Email = email,
            Otp = otp,
            NewPassword = "newpass123",
            ConfirmPassword = "newpass123"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Returns400_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Email không hợp lệ");

        var result = await _controller.ResetPassword(new ResetPasswordDTO
        {
            Email = "bad",
            Otp = "123456",
            NewPassword = "newpass123",
            ConfirmPassword = "newpass123"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Returns500_WhenExceptionThrown()
    {
        var email = "test@example.com";
        var otp = "123456";
        object cachedOtp = otp;
        _memoryCache.Setup(x => x.TryGetValue(email + "_reset_otp", out cachedOtp)).Returns(true);
        _accountBaseRepo.Setup(x => x.GetUserByMail(email)).ThrowsAsync(new Exception("DB error"));

        var result = await _controller.ResetPassword(new ResetPasswordDTO
        {
            Email = email,
            Otp = otp,
            NewPassword = "newpass123",
            ConfirmPassword = "newpass123"
        });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
    }
}
