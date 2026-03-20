using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult> GetAllCustomer([FromBody] RequestDTO<CustomerDTO>? input)
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
                var totalPages = (int)Math.Ceiling((double)recordCount / input.PageSize);

                if (recordCount > 0 && input.PageIndex >= totalPages)
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Page {input.PageIndex} not exist");
                }
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
    }
}
