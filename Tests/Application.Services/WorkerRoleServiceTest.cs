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
    public class WorkerRoleServiceTest
    {
        private readonly Mock<IBaseRepositories<WorkerRole>> _mockWorkerRoleRepo;
        private readonly Mock<IBaseWorkerRoleRepositories> _mockBaseWorkerRepo;

        private readonly WorkerRoleService _service;

        public WorkerRoleServiceTest()
        {
            _mockWorkerRoleRepo = new Mock<IBaseRepositories<WorkerRole>>();
            _mockBaseWorkerRepo = new Mock<IBaseWorkerRoleRepositories>();

            _service = new WorkerRoleService(
                _mockWorkerRoleRepo.Object,
                _mockBaseWorkerRepo.Object
            );
        }

        [Fact]
        public async Task GetAllWorkerRoles_ReturnsList()
        {
            var roles = new List<WorkerRole>
            {
                new WorkerRole { Id = 1, Name = "Ironing" },
                new WorkerRole { Id = 2, Name = "Tailor" }
            };

            _mockWorkerRoleRepo
                .Setup(x => x.GetAll(null))
                .ReturnsAsync(roles);

            var result = await _service.GetAllWorkerRoles();

            Assert.Equal(2, ((List<WorkerRole>)result).Count);
        }

        [Fact]
        public async Task CreateWorkerRole_ReturnsRole_WhenSuccess()
        {
            var role = new WorkerRole
            {
                Id = 1,
                Name = "Sewer"
            };

            _mockBaseWorkerRepo
                .Setup(x => x.FindRoleByName(role.Name))
                .ReturnsAsync((WorkerRole)null);

            _mockWorkerRoleRepo
                .Setup(x => x.Create(It.IsAny<WorkerRole>()))
                .ReturnsAsync(role);

            var result = await _service.CreateWorkerRole(role);

            Assert.Equal(role.Name, result.Name);
        }

        [Fact]
        public async Task CreateWorkerRole_ThrowsException_WhenRoleAlreadyExists()
        {
            var role = new WorkerRole
            {
                Name = "Sewer"
            };

            _mockBaseWorkerRepo
                .Setup(x => x.FindRoleByName(role.Name))
                .ReturnsAsync(new WorkerRole { Name = "Sewer" });

            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateWorkerRole(role));
        }
    }
}
