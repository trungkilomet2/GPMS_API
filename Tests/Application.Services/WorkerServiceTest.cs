using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.TEST.Application.Services
{
    public class WorkerServiceTest
    {
        private readonly Mock<IBaseRepositories<Role>> _mockRoleRepo;
        private readonly Mock<IBaseRepositories<WorkerSkill>> _mockWorkerSkillRepo;
        private readonly Mock<IBaseRepositories<UserStatus>> _mockStatusRepo;
        private readonly Mock<IBaseWorkerRepository> _mockWorkerRepo;
        private readonly Mock<IBaseAccountRepositories> _mockAccRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        private readonly WorkerService _service;

        public WorkerServiceTest()
        {
            _mockRoleRepo = new Mock<IBaseRepositories<Role>>();
            _mockWorkerSkillRepo = new Mock<IBaseRepositories<WorkerSkill>>();
            _mockStatusRepo = new Mock<IBaseRepositories<UserStatus>>();
            _mockAccRepo = new Mock<IBaseAccountRepositories>();
            _mockWorkerRepo = new Mock<IBaseWorkerRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _service = new WorkerService(
                _mockRoleRepo.Object,
                _mockStatusRepo.Object,
                _mockWorkerRepo.Object,
                _mockUnitOfWork.Object,
                _mockAccRepo.Object,
                _mockWorkerSkillRepo.Object
            );
        }

        private void SetupTransaction()
        {
            _mockUnitOfWork.Setup(x =>
                x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task>, CancellationToken>((action, _) => action());
        }

        [Fact]
        public async Task GetAllEmployees_ReturnsEmployees()
        {
            var users = new List<User>
            {
                new User{ Id = 1, UserName = "worker1"},
                new User{ Id = 2, UserName = "worker2"}
            };

            _mockWorkerRepo.Setup(x => x.GetAll(null))
                           .ReturnsAsync(users);

            var result = await _service.GetAllEmployees();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsEmployee()
        {
            var user = new User { Id = 1, UserName = "worker1" };

            _mockWorkerRepo.Setup(x => x.GetWorkerById(1))
                           .ReturnsAsync(user);

            var result = await _service.GetEmployeeById(1);

            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task CreateEmployee_ThrowsException_WhenUserNull()
        {
            await Assert.ThrowsAsync<Exception>(() => _service.CreateEmployee(null));
        }

        [Fact]
        public async Task CreateEmployee_ThrowsKeyNotFound_WhenStatusNotFound()
        {
            var user = new User
            {
                StatusId = 1
            };

            _mockStatusRepo.Setup(x => x.GetById(1))
                           .ReturnsAsync((UserStatus)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateEmployee(user));
        }

        [Fact]
        public async Task CreateEmployee_ThrowsKeyNotFound_WhenRoleNotFound()
        {
            var user = new User
            {
                StatusId = 1,
                Roles = new List<Role> { new Role { Id = 2 } }
            };

            _mockStatusRepo.Setup(x => x.GetById(1))
                           .ReturnsAsync(new UserStatus());

            _mockRoleRepo.Setup(x => x.GetById(2))
                         .ReturnsAsync((Role)null);

            SetupTransaction();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateEmployee(user));
        }

        [Fact]
        public async Task CreateEmployee_ReturnsUser_WhenSuccess()
        {
            var user = new User
            {
                Id = 1,
                StatusId = 1,
                ManagerId= 1,
                Roles = new List<Role> { new Role { Id = 2 } }
            };

            _mockStatusRepo.Setup(x => x.GetById(1))
                           .ReturnsAsync(new UserStatus());

            _mockRoleRepo.Setup(x => x.GetById(2))
                         .ReturnsAsync(new Role { Id = 2 });

            _mockWorkerRepo.Setup(x => x.GetWorkerById(1))
                           .ReturnsAsync(new User { Id = 1 });

            _mockWorkerRepo.Setup(x => x.Create(It.IsAny<User>()))
                           .ReturnsAsync(user);

            SetupTransaction();

            var result = await _service.CreateEmployee(user);

            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task UpdateEmployee_ThrowsException_WhenUserNull()
        {
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateEmployee(1, null));
        }

        [Fact]
        public async Task UpdateEmployee_ThrowsKeyNotFound_WhenStatusNotFound()
        {
            var user = new User { StatusId = 1 };

            _mockStatusRepo.Setup(x => x.GetById(1))
                           .ReturnsAsync((UserStatus)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateEmployee(1, user));
        }

        [Fact]
        public async Task UpdateEmployee_ThrowsKeyNotFound_WhenEmployeeNotFound()
        {
            var user = new User { StatusId = 1 };

            _mockStatusRepo.Setup(x => x.GetById(1))
                           .ReturnsAsync(new UserStatus());

            _mockWorkerRepo.Setup(x => x.GetWorkerById(1))
                           .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateEmployee(1, user));
        }

        [Fact]
        public async Task UpdateEmployee_ThrowsKeyNotFound_WhenRoleNotFound()
        {
            var user = new User
            {
                StatusId = 1,
                Roles = new List<Role> { new Role { Id = 2 } }
            };

            _mockStatusRepo.Setup(x => x.GetById(1))
                           .ReturnsAsync(new UserStatus());

            _mockWorkerRepo.Setup(x => x.GetWorkerById(1))
                           .ReturnsAsync(new User());

            _mockRoleRepo.Setup(x => x.GetById(2))
                         .ReturnsAsync((Role)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateEmployee(1, user));
        }

        [Fact]
        public async Task UpdateEmployee_ReturnsUser_WhenSuccess()
        {
            var user = new User
            {
                Id = 1,
                StatusId = 1,
                ManagerId = 1,
                Roles = new List<Role> { new Role { Id = 2 } }
            };

            _mockStatusRepo.Setup(x => x.GetById(1))
                           .ReturnsAsync(new UserStatus());

            _mockWorkerRepo.Setup(x => x.GetWorkerById(1))
                           .ReturnsAsync(new User());

            _mockRoleRepo.Setup(x => x.GetById(2))
                         .ReturnsAsync(new Role());

            _mockWorkerRepo.Setup(x => x.Update(It.IsAny<User>()))
                           .ReturnsAsync(user);

            var result = await _service.UpdateEmployee(1, user);

            Assert.Equal(1, result.Id);
        }
    }
}
