using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

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
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status = StatusCodes.Status500InternalServerError;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpGet("my-orders")]
        //[Authorize(Roles = "Customer")]
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
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status = StatusCodes.Status500InternalServerError;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "Customer, Owner")]
        public async Task<ActionResult<RestDTO<Order>>> GetOrderDetail(int id)
        {
            try
            {
                var order = await _orderRepo.GetOrderDetail(id);

                if (order is null)
                    return NotFound(new ProblemDetails
                    {
                        Detail = $"Order with id {id} does not exist.",
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    });

                return Ok(new RestDTO<Order>
                {
                    Data = order,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action("GetOrderDetail", "Order", new { id }, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status = StatusCodes.Status500InternalServerError;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }
    }
}