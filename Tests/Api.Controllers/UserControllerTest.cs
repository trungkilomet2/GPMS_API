//using GMPS.API.Controllers;
//using GMPS.API.DTOs;
//using GPMS.APPLICATION.Repositories;
//using GPMS.DOMAIN.Entities;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GPMS.TEST.Api.Controllers
//{
//    public class UserControllerTest
//    {
//        private readonly Mock<IUserRepositories> _mockRepo;
//        private readonly UserController _controller;

//        public UserControllerTest()
//        {
//            _mockRepo = new Mock<IUserRepositories>();
//            _controller = new UserController(_mockRepo.Object, null);
//            var httpContext = new DefaultHttpContext();
//            httpContext.Request.Scheme = "http";
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = httpContext
//            };
//            var mockUrl = new Mock<IUrlHelper>();
//            mockUrl.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
//                   .Returns("http://localhost/api/user/1");
//            _controller.Url = mockUrl.Object;
//        }


//        [Fact]
//        public async Task ViewProfile_ReturnsOk_WhenUserExists()
//        {
//            var fakeUser = new User
//            {
//                Id = 1,
//                UserName = "testuser",
//                FullName = "Test User",
//                PhoneNumber = "0123456789",
//                AvartarUrl = "avatar.jpg",
//                Email = ""
//            };

//            _mockRepo.Setup(x => x.ViewProfile(fakeUser.Id)).ReturnsAsync(fakeUser);

//            var result = await _controller.ViewProfile(fakeUser.Id);


//            var okResult = Assert.IsType<ObjectResult>(result.Result);
//            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

//            var returnValue = Assert.IsType<RestDTO<ViewProfileDTO>>(okResult.Value);
//            Assert.Equal(fakeUser.FullName, returnValue.Data.FullName);
//            Assert.Equal(fakeUser.PhoneNumber, returnValue.Data.PhoneNumber);
//            Assert.Single(returnValue.Links);
//        }

//        [Fact]
//        public async Task ViewProfile_Returns500_WhenRepositoryThrowsException()
//        {
//            _mockRepo.Setup(x => x.ViewProfile(It.IsAny<int>()))
//           .ThrowsAsync(new Exception("db error"));
//            var result = await _controller.ViewProfile(99);
//            var objectResult = Assert.IsType<ObjectResult>(result.Result);
//            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
//        }

//        [Fact]
//        public async Task UpdateUser_ReturnsBadRequest_WhenModelStateIsInvalid()
//        {
//            _controller.ModelState.AddModelError("Email", "Email is required");

//            var result = await _controller.UpdateUser(1, new UpdatedUserDTO());

//            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
//            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
//        }

//        [Fact]
//        public async Task UpdateUser_ReturnsOk_WhenInputIsValid()
//        {
//            var input = new UpdatedUserDTO
//            {
//                FullName = "Updated Name",
//                PhoneNumber = "0123456789",
//                AvartarUrl = "https://example.com/avatar.jpg",
//                Location = "HN",
//                Email = "updated@mail.com"
//            };

//            _mockRepo.Setup(x => x.UpdateProfile(1, It.IsAny<User>()))
//                .ReturnsAsync(new User
//                {
//                    Id = 1,
//                    FullName = input.FullName,
//                    PhoneNumber = input.PhoneNumber,
//                    AvartarUrl = input.AvartarUrl,
//                    Location = input.Location,
//                    Email = input.Email
//                });

//            var result = await _controller.UpdateUser(1, input);

//            var okResult = Assert.IsType<ObjectResult>(result.Result);
//            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

//            var response = Assert.IsType<RestDTO<User>>(okResult.Value);
//            Assert.Equal(1, response.Data.Id);
//            Assert.Equal(input.Email, response.Data.Email);
//        }
//    }

//}
