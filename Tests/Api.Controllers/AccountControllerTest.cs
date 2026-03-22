using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GPMS.TEST.Api.Controllers;

public class AccountControllerTest
{
    private readonly Mock<IAccountRepositories> _accountRepo = new();
    private readonly Mock<IEmailRepositories> _emailRepo = new();
    private readonly Mock<ILogger<AccountController>> _logger = new();
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

        _controller = new AccountController(_accountRepo.Object, _configuration, _logger.Object,_emailRepo.Object)
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
}