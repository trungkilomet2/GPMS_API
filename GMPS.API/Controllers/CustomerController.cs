using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepositories _customerService;
        private readonly ILogger<CustomerController> _logger;
        public CustomerController(ICustomerRepositories customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger;
        }

        [HttpGet("get-all-customer")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> GetAllCustomer([FromQuery] RequestDTO<CustomerDTO>? input)
        {
            try
            {
                _logger.LogInformation("Getting all customers.");
                var data = await _customerService.GetAllCustomer();
                if(data == null || !data.Any())
                {
                    _logger.LogInformation("No customers found.");
                    return StatusCode(StatusCodes.Status404NotFound, "No customer found.");
                }

                if (!string.IsNullOrEmpty(input.FilterQuery?.Trim()))
                {
                    data = data.Where(u =>
                        (u.FullName != null && u.FullName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.UserName != null && u.UserName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Email != null && u.Email.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase))
                    );
                }
                var recordCount = data.Count();
                var customer = data.Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize).Select(c => new CustomerDTO
                {
                    Id = c.Id,
                    UserName = c.UserName,
                    AvatarUrl = c.AvartarUrl,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email
                }).ToList();
                var response = new RestDTO<IEnumerable<CustomerDTO>>
                {
                    Data = customer,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = data.Count(),
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "Customer", null, Request.Scheme)!, "self", "GET")
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                _logger.LogError(CustomLogEvents.Error_Get, ex,
                    "Error while getting customer");
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpGet("get-order-by-customer/{CustomerId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetOrderByCustomer(int CustomerId)
        {            

            try
            {
                _logger.LogInformation("Getting orders for customer {UserId}", CustomerId);

                var orders = await _customerService.GetOrdersByCustomerId(CustomerId);

                if (orders == null || !orders.Any())
                {
                    _logger.LogInformation("No orders found for customer {UserId}", CustomerId);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        Errors = { { "CustomerId", new[] { $"No orders found for customer '{CustomerId}'." } } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound,$"No orders found for customer '{CustomerId}'");
                }

                var data = orders.Select(o => new OrderListDTO
                {
                    Id = o.Id,
                    OrderName = o.OrderName,
                    Size = o.Size,
                    Type = o.Type,
                    Color = o.Color,
                    Quantity = o.Quantity,
                    Cpu = o.Cpu,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    Image = o.Image,
                    Status = o.StatusName
                }).ToList();

                var response = new RestDTO<IEnumerable<OrderListDTO>>
                {
                    Data = data,
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "Customer",
                    null,
                    Request.Scheme)!,
                    "self",
                    "GET"
                )
            }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Get, ex,
                    "Error occurred while getting order", CustomerId);

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
