using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepositories _orderRepo;

        public OrderController(IOrderRepositories orderRepo)
        {
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
        }

        [HttpGet]
        [Authorize(Roles = "Owner")]

        public async Task<ActionResult<RestDTO<IEnumerable<Order>>>> GetOrders([FromQuery] RequestDTO<Order> input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _orderRepo.GetAllOrders();
                    return Ok(new RestDTO<IEnumerable<Order>>
                    {
                        Data = result,
                        PageIndex = input.PageIndex,
                        PageSize = input.PageSize,
                        RecordCount = result.Count(),
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(Url.Action(null, "Order", new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET")
                        }
                    });
                }
                else
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = ModelState
                        .Where(kvp => kvp.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // Customer & Owner xem đơn theo userId
        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult<RestDTO<IEnumerable<Order>>>> GetMyOrders([FromQuery] RequestDTO<Order> input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim is null) return Unauthorized();

                    var userId = int.Parse(userIdClaim);
                    var result = await _orderRepo.GetOrdersByUserId(userId);

                    return Ok(new RestDTO<IEnumerable<Order>>
                    {
                        Data = result,
                        PageIndex = input.PageIndex,
                        PageSize = input.PageSize,
                        RecordCount = result.Count(),
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(Url.Action(null, "Order", new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET")
                        }
                    });
                }
                else
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = ModelState
                        .Where(kvp => kvp.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<RestDTO<Order>>> CreateOrder([FromBody] CreateOrderDTO? input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newOrder = new Order
                    {
                        UserId = input.UserId,
                        Image = input.Image,
                        OrderName = input.OrderName,
                        Type = input.Type,
                        Size = input.Size,
                        Color = input.Color,
                        StartDate = input.StartDate,
                        EndDate = input.EndDate,
                        Quantity = input.Quantity,
                        Cpu = input.Cpu,
                        Note = input.Note,
                        Status = input.Status
                    };
                    var result = await _orderRepo.CreateOrder(newOrder);
                    return Ok(new RestDTO<Order>
                    {
                        Data = result,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(Url.Action(null, "Order", new { id = result.Id }, Request.Scheme)!, "self", "POST")
                        }
                    });
                }
                else
                {
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
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }

        }
    }
}