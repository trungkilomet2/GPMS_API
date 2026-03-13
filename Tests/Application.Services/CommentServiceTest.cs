using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Entities;
using Moq;

namespace GPMS.TEST.Application.Services
{
    public class CommentServiceTest
    {
        private readonly Mock<IBaseRepositories<Comment>> _commentRepo = new();
        private readonly Mock<IBaseRepositories<Order>> _orderRepo = new();

        private CommentServices BuildService()
        {
            return new CommentServices(_commentRepo.Object, _orderRepo.Object);
        }


        [Fact]
        public async Task CreateComment_ThrowsException_WhenEntityNull()
        {
            var service = BuildService();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateComment(null));
        }

        [Fact]
        public async Task CreateComment_ThrowsException_WhenContentEmpty()
        {
            var service = BuildService();

            var comment = new Comment
            {
                Content = "",
                fromUserId = 1,
                toOrderId = 1
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateComment(comment));
        }

        [Fact]
        public async Task CreateComment_ThrowsException_WhenFromUserInvalid()
        {
            var service = BuildService();

            var comment = new Comment
            {
                Content = "Test",
                fromUserId = 0,
                toOrderId = 1
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateComment(comment));
        }

        [Fact]
        public async Task CreateComment_ThrowsException_WhenOrderInvalid()
        {
            var service = BuildService();

            var comment = new Comment
            {
                Content = "Test",
                fromUserId = 1,
                toOrderId = 0
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateComment(comment));
        }

        [Fact]
        public async Task CreateComment_ReturnsComment_WhenValid()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "Test comment",
                fromUserId = 1,
                toOrderId = 1
            };

            _commentRepo.Setup(x => x.Create(It.IsAny<Comment>()))
                .ReturnsAsync(comment);

            var service = BuildService();

            var result = await service.CreateComment(comment);

            Assert.NotNull(result);
            Assert.Equal("Test comment", result.Content);
        }


        [Fact]
        public async Task DeleteComment_ThrowsException_WhenIdInvalid()
        {
            var service = BuildService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.DeleteComment(0, 1));
        }

        [Fact]
        public async Task DeleteComment_ThrowsException_WhenCommentNotFound()
        {
            _commentRepo.Setup(x => x.GetById(1))
                .ReturnsAsync((Comment)null);

            var service = BuildService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.DeleteComment(1, 1));
        }

        [Fact]
        public async Task DeleteComment_ThrowsException_WhenUserNotOwner()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "Test",
                fromUserId = 5
            };

            _commentRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(comment);

            var service = BuildService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.DeleteComment(1, 1));
        }

        [Fact]
        public async Task DeleteComment_CallsDelete_WhenValid()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "Test",
                fromUserId = 1
            };

            _commentRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(comment);

            var service = BuildService();

            await service.DeleteComment(1, 1);

            _commentRepo.Verify(x => x.Delete(1), Times.Once);
        }


        [Fact]
        public async Task GetCommentById_ThrowsException_WhenOrderInvalid()
        {
            var service = BuildService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetCommentById(0));
        }

        [Fact]
        public async Task GetCommentById_ThrowsException_WhenOrderNotFound()
        {
            _orderRepo.Setup(x => x.GetById(1))
                .ReturnsAsync((Order)null);

            var service = BuildService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.GetCommentById(1));
        }

        [Fact]
        public async Task GetCommentById_ReturnsComments_WhenExist()
        {
            var comments = new List<Comment>
            {
                new Comment { Id = 1, Content = "Comment 1" },
                new Comment { Id = 2, Content = "Comment 2" }
            };

            _orderRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(new Order { Id = 1 });

            _commentRepo.Setup(x => x.GetAll(1))
                .ReturnsAsync(comments);

            var service = BuildService();

            var result = await service.GetCommentById(1);

            Assert.Equal(2, result.Count());
        }
       

        [Fact]
        public async Task UpdateComment_ThrowsException_WhenCommentNotFound()
        {
            var comment = new Comment { Id = 1 };

            _commentRepo.Setup(x => x.GetById(1))
                .ReturnsAsync((Comment)null);

            var service = BuildService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateComment(comment, 1));
        }

        [Fact]
        public async Task UpdateComment_ThrowsException_WhenUserNotOwner()
        {
            var existing = new Comment
            {
                Id = 1,
                fromUserId = 2
            };

            var comment = new Comment { Id = 1 };

            _commentRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(existing);

            var service = BuildService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateComment(comment, 1));
        }

        [Fact]
        public async Task UpdateComment_ReturnsUpdatedComment_WhenValid()
        {
            var existing = new Comment
            {
                Id = 1,
                fromUserId = 1
            };

            var updated = new Comment
            {
                Id = 1,
                Content = "Updated comment",
                fromUserId = 1
            };

            _commentRepo.Setup(x => x.GetById(1))
                .ReturnsAsync(existing);

            _commentRepo.Setup(x => x.Update(updated))
                .ReturnsAsync(updated);

            var service = BuildService();

            var result = await service.UpdateComment(updated, 1);

            Assert.NotNull(result);
            Assert.Equal("Updated comment", result.Content);
        }
    }
}