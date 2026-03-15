using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
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

        private UserService BuildService()
        {
            return new UserService(_userRepo.Object, _roleRepo.Object, _userRoleRepo.Object, _unitOfWork.Object);
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
    }
}