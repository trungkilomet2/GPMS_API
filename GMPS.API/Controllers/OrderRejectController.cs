using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderRejectController : ControllerBase
    {
        private readonly IOrderRejectRepositories _orderRejectRepo;
        private readonly ILogger<OrderRejectController> _logger;
        private readonly IEmailRepositories _emailRepo;
        private readonly IUserRepositories _userRepo;
        private readonly IOrderRepositories _orderRepo;

        public OrderRejectController(IOrderRejectRepositories orderRejectRepo, ILogger<OrderRejectController> logger, 
            IEmailRepositories emailRepo, IUserRepositories userRepo, IOrderRepositories orderRepo)
        {
            _orderRejectRepo = orderRejectRepo ?? throw new ArgumentNullException(nameof(orderRejectRepo));
            _logger = logger;
            _emailRepo = emailRepo ?? throw new ArgumentNullException(nameof(emailRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
        }

        [HttpPost("order-reject")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> CreateOrderReject([FromBody] CreateOrderRejectDTO? input)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post, "Creating order reject for OrderId {OrderId}", input?.OrderId);
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                if (ModelState.IsValid)
                {
                    var newOrderReject = new OrderRejectReason
                    {
                        OrderId = input.OrderId,
                        UserId = userId,
                        Reason = input.Reason,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
                    };
                    var result = await _orderRejectRepo.CreateReason(newOrderReject);
                    var order = await _orderRepo.GetOrderDetail(input.OrderId);
                    var user = await _userRepo.GetUserById(order.UserId);
                    await _emailRepo.SendEmailAsync(user.Email, "Thông báo từ chối đơn hàng", 
                        $"Đơn hàng với Id: '{input.OrderId}' đã bị từ chối bởi lý do như sau: {input.Reason}");
                    _logger.LogInformation(CustomLogEvents.OrderRejectController_Post, "Successfully created order reject for OrderId {OrderId}", input.OrderId);
                    return StatusCode(StatusCodes.Status201Created, $"Order reject with OrderId '{result.OrderId}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post, "Invalid model state for creating order reject for OrderId {OrderId}", input?.OrderId);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Post, ex, "Error occurred while creating order reject for OrderId {OrderId}", input?.OrderId);
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails.Detail);
            }
        }

        [HttpPost("order-deny")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> CreateOrderModification([FromBody] CreateOrderRejectDTO? input)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post, "Creating order deny for OrderId {OrderId}", input?.OrderId);
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                if (ModelState.IsValid)
                {
                    var newOrderReject = new OrderRejectReason
                    {
                        OrderId = input.OrderId,
                        UserId = userId,
                        Reason = input.Reason,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
                    };
                    var result = await _orderRejectRepo.CreateReason(newOrderReject);
                    var order = await _orderRepo.GetOrderDetail(input.OrderId);
                    var user = await _userRepo.GetUserById(order.UserId);
                    await _emailRepo.SendEmailAsync(user.Email, "Thông báo yêu cầu chỉnh sửa đơn hàng",
                        $"Đơn hàng với Id: '{input.OrderId}' đã bị yêu cầu chỉnh sửa bởi lý do như sau: {input.Reason}");
                    _logger.LogInformation(CustomLogEvents.OrderRejectController_Post, "Successfully created order reject for OrderId {OrderId}", input.OrderId);
                    return StatusCode(StatusCodes.Status201Created, $"Order deny with OrderId '{result.OrderId}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post, "Invalid model state for creating order reject for OrderId {OrderId}", input?.OrderId);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Post, ex, "Error occurred while creating order reject for OrderId {OrderId}", input?.OrderId);
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails.Detail);
            }
        }
    }
}
