using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Enum;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Moq;

namespace GPMS.TEST.Application.Services;

public class AccountServiceTest
{

    private readonly Mock<IBaseAccountRepositories> _accountBaseRepo = new();
    private readonly Mock<IBaseRepositories<Role>> _roleBaseRepo = new();
    private readonly Mock<IBaseUserRoleRepo> _userRoleRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AccountService BuildService()
    {
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        return new AccountService(_accountBaseRepo.Object, _roleBaseRepo.Object, _userRoleRepo.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Register_ReturnsFailed_WhenUsernameTooShort()
    {
        var service = BuildService();

        var result = await service.Register(new User { UserName = "abc", PasswordHash = "123456" });

        Assert.Equal(RegisterStatus.Failed, result.Status);
        Assert.True(result.Errors.ContainsKey("UserName"));
    }

    [Fact]
    public async Task Register_ReturnsSuccess_WhenInputValid()
    {
        var user = new User { Id = 10, UserName = "validuser", PasswordHash = "123456" };
        _accountBaseRepo.Setup(x => x.Register(It.IsAny<User>())).ReturnsAsync(user);
        _accountBaseRepo.Setup(x => x.FindUserByUserName("validuser")).ReturnsAsync(user);

        var service = BuildService();

        var result = await service.Register(user);

        Assert.Equal(RegisterStatus.Success, result.Status);
        _userRoleRepo.Verify(x => x.AddUserRole(user, Roles_Constants.Customer), Times.Once);
    }

    [Fact]
    public async Task Login_ReturnsNull_WhenPasswordInvalid()
    {
        var user = new User { UserName = "validuser", PasswordHash = "AQAAAAIAAYagAAAAEOxoaXaFwlfrzFu68tB383ozk6AVd/UmOzboySqemEoZCgFhi+DYkS+kNKuTtGVCQA==" };
        _accountBaseRepo.Setup(x => x.FindUserByUserName("validuser")).ReturnsAsync(user);

        var service = BuildService();

        var result = await service.Login("validuser", "wrong-password");

        Assert.Null(result);
    }
}