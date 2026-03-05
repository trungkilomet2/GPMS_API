using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepositories _orderRepo;

        public OrderController(IOrderRepositories orderRepo)
        {
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
        }

        [HttpGet]
        //[Authorize(Roles = "Owner")]
        public async Task<ActionResult<RestDTO<IEnumerable<OrderListDTO>>>> GetOrders([FromQuery] RequestDTO<Order> input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _orderRepo.GetAllOrders();

                    var data = result.Select(o => new OrderListDTO
                    {
                        Id = o.Id,
                        OrderName = o.OrderName,
                        Type = o.Type,
                        Size = o.Size,
                        Color = o.Color,
                        Quantity = o.Quantity,
                        Cpu = o.Cpu,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        Image = o.Image,
                        Status = o.Status
                    });

                    return Ok(new RestDTO<IEnumerable<OrderListDTO>>
                    {
                        Data = data,
                        PageIndex = input.PageIndex,
                        PageSize = input.PageSize,
                        RecordCount = data.Count(),
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
        public async Task<ActionResult<RestDTO<IEnumerable<OrderListDTO>>>> GetMyOrders([FromQuery] RequestDTO<Order> input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim is null) return Unauthorized();

                    var userId = int.Parse(userIdClaim);
                    var result = await _orderRepo.GetOrdersByUserId(userId);

                    var data = result.Select(o => new OrderListDTO
                    {
                        Id = o.Id,
                        OrderName = o.OrderName,
                        Type = o.Type,
                        Size = o.Size,
                        Color = o.Color,
                        Quantity = o.Quantity,
                        Cpu = o.Cpu,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        Image = o.Image,
                        Status = o.Status
                    });

                    return Ok(new RestDTO<IEnumerable<OrderListDTO>>
                    {
                        Data = data,
                        PageIndex = input.PageIndex,
                        PageSize = input.PageSize,
                        RecordCount = data.Count(),
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

        // UC Order Detail
        [HttpGet("{id}")]
        //[Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult<RestDTO<OrderDetailDTO>>> GetOrderDetail(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id phải lớn hơn 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var order = await _orderRepo.GetOrderDetail(id);

                if (order is null)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order với id '{id}' không tồn tại trong hệ thống" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                var data = new OrderDetailDTO
                {
                    Id = order.Id,
                    OrderName = order.OrderName,
                    Type = order.Type,
                    Size = order.Size,
                    Color = order.Color,
                    Quantity = order.Quantity,
                    Cpu = order.Cpu,
                    StartDate = order.StartDate,
                    EndDate = order.EndDate,
                    Image = order.Image,
                    Note = order.Note,
                    Status = order.Status,
                    Templates = order.Templates,
                    Materials = order.Materials
                };

                return Ok(new RestDTO<OrderDetailDTO>
                {
                    Data = data,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action("GetOrderDetail", "Order", new { id }, Request.Scheme)!, "self", "GET"),
                        new LinkDTO(Url.Action("GetOrderHistory", "Order", new { id }, Request.Scheme)!, "history", "GET")
                    }
                });
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

        // View History Update of Order
        [HttpGet("{id}/history")]
        //[Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult<RestDTO<IEnumerable<OHistoryUpdate>>>> GetOrderHistory(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id phải lớn hơn 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var order = await _orderRepo.GetOrderDetail(id);

                if (order is null)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order với id '{id}' không tồn tại trong hệ thống" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                return Ok(new RestDTO<IEnumerable<OHistoryUpdate>>
                {
                    Data = order.Histories,
                    RecordCount = order.Histories.Count(),
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action("GetOrderHistory", "Order", new { id }, Request.Scheme)!, "self", "GET")
                    }
                });
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