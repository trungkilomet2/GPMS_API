using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Moq;

namespace GPMS.TEST.Application.Services
{
    public class UserServiceTest
    {
        private readonly Mock<IBaseRepositories<User>> _userRepo = new();
        private readonly Mock<IBaseRepositories<Role>> _roleRepo = new();
        private readonly Mock<IBaseUserRoleRepo> _userRoleRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IBaseAccountRepositories> _baseAccount = new();
        private UserService BuildService()
        {
            return new UserService(_userRepo.Object, _roleRepo.Object, _userRoleRepo.Object, _unitOfWork.Object, _baseAccount.Object);
        }

        [Fact]
        public async Task GetAllUser_ReturnsUserList()
        {
            var users = new List<User>
            {
                new User { Id = 1, UserName = "user1" },
                new User { Id = 2, UserName = "user2" }
            };

            _userRepo.Setup(x => x.GetAll(null))
                .ReturnsAsync(users);

            var service = BuildService();

            var result = await service.GetAllUser();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ViewProfile_ThrowsException_WhenUserNotFound()
        {
            _userRepo.Setup(x => x.GetById(1))
                .ReturnsAsync((User?)null);

            var service = BuildService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ViewProfile(1));
        }

        [Fact]
        public async Task ViewProfile_ReturnsUser_WhenUserExists()
        {
            var user = new User
            {
                Id = 1,
                UserName = "testuser"
            };

            _userRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(user);

            var service = BuildService();

            var result = await service.ViewProfile(1);

            Assert.NotNull(result);
            Assert.Equal("testuser", result.UserName);
        }

        [Fact]
        public async Task UpdateProfile_ThrowsException_WhenUserNotFound()
        {
            _userRepo.Setup(x => x.GetById(1))
                .ReturnsAsync((User?)null);

            var service = BuildService();

            var user = new User { Id = 1, UserName = "updated" };

            await Assert.ThrowsAsync<Exception>(() => service.UpdateProfile(1, user));
        }

        [Fact]
        public async Task UpdateProfile_ThrowsException_WhenIdMismatch()
        {
            var existingUser = new User
            {
                Id = 2,
                UserName = "olduser"
            };

            _userRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(existingUser);

            var service = BuildService();

            var user = new User { Id = 1, UserName = "updated" };

            await Assert.ThrowsAsync<Exception>(() => service.UpdateProfile(1, user));
        }

        [Fact]
        public async Task UpdateProfile_ReturnsUpdatedUser_WhenValid()
        {
            var existingUser = new User
            {
                Id = 1,
                UserName = "olduser"
            };

            var updatedUser = new User
            {
                Id = 1,
                UserName = "newuser"
            };

            _userRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(existingUser);

            _userRepo.Setup(x => x.Update(It.IsAny<User>()))
                .ReturnsAsync(updatedUser);

            var service = BuildService();

            var result = await service.UpdateProfile(1, updatedUser);

            Assert.NotNull(result);
            Assert.Equal("newuser", result.UserName);
        }

        // ─── GetUserById ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetUserById_ReturnsUser_WhenFound()
        {
            var user = new User { Id = 1, UserName = "testuser" };
            _userRepo.Setup(x => x.GetById(1)).ReturnsAsync(user);

            var service = BuildService();
            var result = await service.GetUserById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        // ─── CreateNewUser ────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateNewUser_ReturnsUser_WhenSuccessful()
        {
            var user = new User { Id = 0, UserName = "newuser", PasswordHash = "plain123" };
            var createdUser = new User { Id = 5, UserName = "newuser" };
            var role = new Role { Id = 1, Name = "Admin" };

            _unitOfWork
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task>, CancellationToken>((action, ct) => action());

            _userRepo.Setup(x => x.Create(It.IsAny<User>())).ReturnsAsync(createdUser);
            _roleRepo.Setup(x => x.GetById(1)).ReturnsAsync(role);
            _userRoleRepo.Setup(x => x.AddUserRole(It.IsAny<User>(), "Admin")).Returns(Task.CompletedTask);

            var service = BuildService();
            var result = await service.CreateNewUser(user, new List<int> { 1 });

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
        }

        [Fact]
        public async Task CreateNewUser_Throws_WhenRoleNotFound()
        {
            var user = new User { Id = 0, UserName = "newuser", PasswordHash = "plain123" };
            var createdUser = new User { Id = 5, UserName = "newuser" };

            _unitOfWork
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task>, CancellationToken>((action, ct) => action());

            _userRepo.Setup(x => x.Create(It.IsAny<User>())).ReturnsAsync(createdUser);
            _roleRepo.Setup(x => x.GetById(99)).ReturnsAsync((Role)null);

            var service = BuildService();

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.CreateNewUser(user, new List<int> { 99 }));

            Assert.Equal("Role with ID 99 not found.", ex.Message);
        }

        // ─── DisableAnUser ────────────────────────────────────────────────────────

        [Fact]
        public async Task DisableAnUser_CompletesSuccessfully_WhenUserActive()
        {
            var user = new User { Id = 1, StatusId = UserStatus_Constants.Active };
            _userRepo.Setup(x => x.GetById(1)).ReturnsAsync(user);
            _userRepo.Setup(x => x.Update(It.IsAny<User>())).ReturnsAsync(user);

            var service = BuildService();

            await service.DisableAnUser(1);

            Assert.Equal(UserStatus_Constants.Inactive, user.StatusId);
        }

        [Fact]
        public async Task DisableAnUser_Throws_WhenUserNotFound()
        {
            _userRepo.Setup(x => x.GetById(99)).ReturnsAsync((User)null);

            var service = BuildService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DisableAnUser(99));
        }

        [Fact]
        public async Task DisableAnUser_Throws_WhenAlreadyInactive()
        {
            var user = new User { Id = 1, StatusId = UserStatus_Constants.Inactive };
            _userRepo.Setup(x => x.GetById(1)).ReturnsAsync(user);

            var service = BuildService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DisableAnUser(1));
        }

        // ─── AssignRoles ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AssignRoles_CompletesSuccessfully()
        {
            var user = new User { Id = 1, UserName = "testuser" };
            var role = new Role { Id = 1, Name = "Admin" };

            _userRepo.Setup(x => x.GetById(1)).ReturnsAsync(user);
            _roleRepo.Setup(x => x.GetById(1)).ReturnsAsync(role);
            _userRoleRepo.Setup(x => x.ReplaceUserRoles(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns(Task.CompletedTask);

            var service = BuildService();

            await service.AssignRoles(1, new List<int> { 1 });

            _userRoleRepo.Verify(x => x.ReplaceUserRoles(user, It.Is<List<string>>(r => r.Contains("Admin"))), Times.Once);
        }

        [Fact]
        public async Task AssignRoles_Throws_WhenUserNotFound()
        {
            _userRepo.Setup(x => x.GetById(99)).ReturnsAsync((User)null);

            var service = BuildService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.AssignRoles(99, new List<int> { 1 }));
        }

        [Fact]
        public async Task AssignRoles_Throws_WhenRoleNotFound()
        {
            var user = new User { Id = 1, UserName = "testuser" };
            _userRepo.Setup(x => x.GetById(1)).ReturnsAsync(user);
            _roleRepo.Setup(x => x.GetById(99)).ReturnsAsync((Role)null);

            var service = BuildService();

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.AssignRoles(1, new List<int> { 99 }));

            Assert.Equal("Role with ID 99 not found.", ex.Message);
        }

        // ─── UpdateUserForAdmin ───────────────────────────────────────────────────

        [Fact]
        public async Task UpdateUserForAdmin_ReturnsUpdatedUser()
        {
            var existing = new User { Id = 1, UserName = "old", StatusId = UserStatus_Constants.Active };
            var updated = new User { Id = 1, UserName = "new" };

            _userRepo.Setup(x => x.GetById(1)).ReturnsAsync(existing);
            _userRepo.Setup(x => x.Update(It.IsAny<User>())).ReturnsAsync(updated);

            var service = BuildService();
            var result = await service.UpdateUserForAdmin(1, updated);

            Assert.Equal("new", result.UserName);
        }

        [Fact]
        public async Task UpdateUserForAdmin_Throws_WhenUserNotFound()
        {
            _userRepo.Setup(x => x.GetById(99)).ReturnsAsync((User)null);

            var service = BuildService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateUserForAdmin(99, new User { Id = 99 }));
        }
    }
}