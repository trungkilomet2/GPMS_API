using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ILogger<OrderController> _logger;
        private readonly ICloudinaryService _cloudinaryService;

        public OrderController(IOrderRepositories orderRepo, ILogger<OrderController> logger, ICloudinaryService cloudinaryService)
        {
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        // api/order/order-list
        [HttpGet("order-list", Name = "Get all order list")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<RestDTO<IEnumerable<OrderListDTO>>>> GetOrders([FromQuery] OrderRequestDTO input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Getting all orders - PageIndex: {PageIndex}, PageSize: {PageSize}, Status: {Status}, StartDateFrom: {StartDateFrom}, StartDateTo: {StartDateTo}",
                    input.PageIndex, input.PageSize, input.Status, input.StartDateFrom, input.StartDateTo);

                var validStatuses = new[]
                {
                    OrderStatus_Constants.Pending, OrderStatus_Constants.Modification,
                    OrderStatus_Constants.Approved, OrderStatus_Constants.Rejected, OrderStatus_Constants.Cancelled
                };

                if (!string.IsNullOrEmpty(input.Status) && !validStatuses.Contains(input.Status))
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "Invalid Status value '{Status}' provided", input.Status);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { $"Status must be one of: '{OrderStatus_Constants.Pending}', '{OrderStatus_Constants.Modification}', '{OrderStatus_Constants.Approved}', '{OrderStatus_Constants.Rejected}', '{OrderStatus_Constants.Cancelled}'." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (input.StartDateFrom.HasValue && input.StartDateTo.HasValue && input.StartDateFrom > input.StartDateTo)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "StartDateFrom {StartDateFrom} is greater than StartDateTo {StartDateTo}",
                        input.StartDateFrom, input.StartDateTo);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "startDateFrom", new[] { "StartDateFrom must be less than or equal to StartDateTo." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "Invalid model state while getting all orders");

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var result = await _orderRepo.GetAllOrders();

                if (!string.IsNullOrEmpty(input.FilterQuery))
                    result = result.Where(o => o.OrderName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(input.Status))
                    result = result.Where(o => o.StatusName != null &&
                        o.StatusName.Equals(input.Status, StringComparison.OrdinalIgnoreCase));

                if (input.StartDateFrom.HasValue)
                    result = result.Where(o => o.StartDate >= input.StartDateFrom.Value);

                if (input.StartDateTo.HasValue)
                    result = result.Where(o => o.StartDate <= input.StartDateTo.Value);

                var isDesc = string.IsNullOrEmpty(input.SortColumn)
                    || string.Equals(input.SortOrder, "DESC", StringComparison.OrdinalIgnoreCase);

                result = input.SortColumn?.ToLower() switch
                {
                    "startdate" => isDesc ? result.OrderByDescending(o => o.StartDate) : result.OrderBy(o => o.StartDate),
                    "enddate"   => isDesc ? result.OrderByDescending(o => o.EndDate)   : result.OrderBy(o => o.EndDate),
                    "status"    => isDesc ? result.OrderByDescending(o => o.StatusName) : result.OrderBy(o => o.StatusName),
                    "quantity"  => isDesc ? result.OrderByDescending(o => o.Quantity)  : result.OrderBy(o => o.Quantity),
                    "name"      => isDesc ? result.OrderByDescending(o => o.OrderName) : result.OrderBy(o => o.OrderName),
                    _           => isDesc ? result.OrderByDescending(o => o.Id)        : result.OrderBy(o => o.Id)
                };

                var recordCount = result.Count();
                var totalPages = (int)Math.Ceiling((double)recordCount / input.PageSize);

                if (recordCount > 0 && input.PageIndex >= totalPages)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "PageIndex {PageIndex} out of range. Total pages: {TotalPages}", input.PageIndex, totalPages);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "pageIndex", new[] { $"Page {input.PageIndex} not exist. Total number of pages currently available: {totalPages}" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .Select(o => new OrderListDTO
                    {
                        Id = o.Id,
                        UserId = o.UserId,
                        OrderName = o.OrderName,
                        Type = o.Type,
                        Size = o.Size,
                        Color = o.Color,
                        Quantity = o.Quantity,
                        Cpu = o.Cpu,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        Image = o.Image,
                        Status = o.StatusName
                    });

                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Returned {Count} orders successfully", data.Count());

                return Ok(new RestDTO<IEnumerable<OrderListDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "Order", new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Get, ex,
                    "Error occurred while getting all orders");

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // api/order/my-orders
        [HttpGet("my-orders", Name = "Get order list by customer")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult<RestDTO<IEnumerable<OrderListDTO>>>> GetMyOrders([FromQuery] OrderRequestDTO input)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Getting orders for UserId {UserId} - PageIndex: {PageIndex}, PageSize: {PageSize}, Status: {Status}, StartDateFrom: {StartDateFrom}, StartDateTo: {StartDateTo}",
                    userId, input.PageIndex, input.PageSize, input.Status, input.StartDateFrom, input.StartDateTo);

                var validStatuses = new[]
                {
                    OrderStatus_Constants.Pending, OrderStatus_Constants.Modification,
                    OrderStatus_Constants.Approved, OrderStatus_Constants.Rejected, OrderStatus_Constants.Cancelled
                };

                if (!string.IsNullOrEmpty(input.Status) && !validStatuses.Contains(input.Status))
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "UserId {UserId} provided invalid Status value '{Status}'", userId, input.Status);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { $"Status must be one of: '{OrderStatus_Constants.Pending}', '{OrderStatus_Constants.Modification}', '{OrderStatus_Constants.Approved}', '{OrderStatus_Constants.Rejected}', '{OrderStatus_Constants.Cancelled}'." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (input.StartDateFrom.HasValue && input.StartDateTo.HasValue && input.StartDateFrom > input.StartDateTo)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "UserId {UserId}: StartDateFrom {StartDateFrom} is greater than StartDateTo {StartDateTo}",
                        userId, input.StartDateFrom, input.StartDateTo);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "startDateFrom", new[] { "StartDateFrom must be less than or equal to StartDateTo." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "Invalid model state while getting orders for UserId {UserId}", userId);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var result = await _orderRepo.GetOrdersByUserId(userId);

                if (!string.IsNullOrEmpty(input.FilterQuery))
                    result = result.Where(o => o.OrderName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(input.Status))
                    result = result.Where(o => o.StatusName != null &&
                        o.StatusName.Equals(input.Status, StringComparison.OrdinalIgnoreCase));

                if (input.StartDateFrom.HasValue)
                    result = result.Where(o => o.StartDate >= input.StartDateFrom.Value);

                if (input.StartDateTo.HasValue)
                    result = result.Where(o => o.StartDate <= input.StartDateTo.Value);

                var isDesc = string.IsNullOrEmpty(input.SortColumn)
                    || string.Equals(input.SortOrder, "DESC", StringComparison.OrdinalIgnoreCase);

                result = input.SortColumn?.ToLower() switch
                {
                    "startdate" => isDesc ? result.OrderByDescending(o => o.StartDate) : result.OrderBy(o => o.StartDate),
                    "enddate"   => isDesc ? result.OrderByDescending(o => o.EndDate)   : result.OrderBy(o => o.EndDate),
                    "status"    => isDesc ? result.OrderByDescending(o => o.StatusName) : result.OrderBy(o => o.StatusName),
                    "quantity"  => isDesc ? result.OrderByDescending(o => o.Quantity)  : result.OrderBy(o => o.Quantity),
                    "name"      => isDesc ? result.OrderByDescending(o => o.OrderName) : result.OrderBy(o => o.OrderName),
                    _           => isDesc ? result.OrderByDescending(o => o.Id)        : result.OrderBy(o => o.Id)
                };

                var recordCount = result.Count();
                var totalPages = (int)Math.Ceiling((double)recordCount / input.PageSize);

                if (input.PageIndex > 0 && (recordCount == 0 || input.PageIndex >= totalPages))
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "PageIndex {PageIndex} out of range for UserId {UserId}. Total pages: {TotalPages}",
                        input.PageIndex, userId, totalPages);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "pageIndex", new[] { $"Page {input.PageIndex} not exist. Total number of pages currently available: {totalPages}" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .Select(o => new OrderListDTO
                    {
                        Id = o.Id,
                        UserId = o.UserId,
                        OrderName = o.OrderName,
                        Type = o.Type,
                        Size = o.Size,
                        Color = o.Color,
                        Quantity = o.Quantity,
                        Cpu = o.Cpu,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        Image = o.Image,
                        Status = o.StatusName
                    });

                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Returned {Count} orders for UserId {UserId}", data.Count(), userId);

                return Ok(new RestDTO<IEnumerable<OrderListDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "Order", new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Get, ex,
                    "Error occurred while getting orders for UserId from token");

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // api/order/order-detail/{id}
        [HttpGet("order-detail/{id}", Name = "Get order detail by id")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult<RestDTO<OrderDetailDTO>>> GetOrderDetail(int id)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Getting order detail for OrderId {OrderId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "Invalid OrderId {OrderId} - must be greater than 0", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var order = await _orderRepo.GetOrderDetail(id);

                if (order is null)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "Order {OrderId} not found", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order with id '{id}' not found" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                var data = new OrderDetailDTO
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    UserFullName = order.UserFullName,
                    UserPhone = order.UserPhone,
                    UserLocation = order.UserLocation,
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
                    Status = order.StatusName,
                    Templates = order.Templates,
                    Materials = order.Materials
                };

                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Returned detail for OrderId {OrderId} successfully", id);

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
                _logger.LogError(CustomLogEvents.OrderController_Get, ex,
                    "Error occurred while getting detail for OrderId {OrderId}", id);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // api/order/{id}/history
        [HttpGet("{id}/history", Name = "Get order update history by id")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult<RestDTO<IEnumerable<OHistoryUpdate>>>> GetOrderHistory(int id)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Getting update history for OrderId {OrderId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "Invalid OrderId {OrderId} - must be greater than 0", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var order = await _orderRepo.GetOrderDetail(id);

                if (order is null)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "Order {OrderId} not found when getting history", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order with id '{id}' not found" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                _logger.LogInformation(CustomLogEvents.OrderController_Get,
                    "Returned {Count} history records for OrderId {OrderId}",
                    order.Histories.Count(), id);

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
                _logger.LogError(CustomLogEvents.OrderController_Get, ex,
                    "Error occurred while getting history for OrderId {OrderId}", id);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // api/order
        [HttpPost("create-order")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDTO? input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post, "Creating new order for UserId {UserId}", input?.UserId);
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
                        Status = 1,
                        Material = input.Materials?.Select(m => new OrderMaterial
                        {
                            MaterialName = m.MaterialName,
                            Image = m.Image,
                            Value = m.Value,
                            Uom = m.Uom,
                            Note = m.Note
                        }).ToList(),

                        Template = input.Templates?.Select(t => new OrderTemplate
                        {
                            TemplateName = t.TemplateName,
                            Type = t.Type,
                            File = t.File,
                            Quantity = t.Quantity,
                            Note = t.Note
                        }).ToList(),
                    };
                    var result = await _orderRepo.CreateOrder(newOrder);
                    _logger.LogInformation(CustomLogEvents.OrderController_Post, "Order {OrderId} created successfully for UserId {UserId}", result.Id, input.UserId);

                    return StatusCode(StatusCodes.Status201Created, $"Order '{result.Id}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post, "Invalid model state while creating order for UserId {UserId}", input?.UserId);

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
                _logger.LogError(CustomLogEvents.OrderController_Post, ex, "Error occurred while creating order for UserId {UserId}", input?.UserId);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails.Detail);
            }
        }

        // api/order/{orderId}/materials
        [HttpPost("{orderId}/materials", Name = "Add material to order")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> AddMaterial(int orderId, [FromBody] CreateMaterialDTO? input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Adding material to OrderId {OrderId}", orderId);

                if (ModelState.IsValid)
                {
                    var material = new OMaterial
                    {
                        Name = input.MaterialName,
                        Image = input.Image,
                        Value = input.Value,
                        Uom = input.Uom,
                        Note = input.Note
                    };
                    var result = await _orderRepo.AddMaterial(orderId, material);

                    _logger.LogInformation(CustomLogEvents.OrderController_Post,
                        "Material {MaterialId} added successfully to OrderId {OrderId}", result.Id, orderId);

                    return StatusCode(StatusCodes.Status201Created, $"Material '{result.Id}' has been added to order '{orderId}'");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post,
                        "Invalid model state while adding material to OrderId {OrderId}", orderId);

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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Post,
                    "Cannot add material to OrderId {OrderId}: {Message}", orderId, ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
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

        // api/order/{id}/update
        [HttpPut("{id}/update", Name = "Update order by customer")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDTO? input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Updating OrderId {OrderId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Invalid OrderId {OrderId} - must be greater than 0", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Invalid model state while updating OrderId {OrderId}", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                var existingOrder = await _orderRepo.GetOrderDetail(id);
                if (existingOrder is null)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Order {OrderId} not found", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order with id ' {id} ' not found" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                if (existingOrder.StatusName != OrderStatus_Constants.Modification)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Order {OrderId} cannot be updated - current status is '{Status}', required 'Modification'",
                        id, existingOrder.StatusName);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { $"Only modify order with status '{OrderStatus_Constants.Modification}'" } }
                    };
                    return StatusCode(StatusCodes.Status403Forbidden, errorDetails);
                }

                if (existingOrder.UserId != userId)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "UserId {UserId} is not the owner of OrderId {OrderId}", userId, id);

                    return Forbid();
                }

                // Giữ ảnh cũ nếu user không gửi ảnh mới
                var resolvedImage = !string.IsNullOrEmpty(input!.Image)
                    ? input.Image
                    : existingOrder.Image;

                var histories = new List<OHistoryUpdate>();
                void TrackChange(string field, string? oldVal, string? newVal)
                {
                    if (oldVal != newVal)
                        histories.Add(new OHistoryUpdate
                        {
                            OrderId = id,
                            FieldName = field,
                            OldValue = oldVal ?? string.Empty,
                            NewValue = newVal ?? string.Empty
                        });
                }

                TrackChange("OrderName", existingOrder.OrderName, input.OrderName);
                TrackChange("Type", existingOrder.Type, input.Type);
                TrackChange("Size", existingOrder.Size, input.Size);
                TrackChange("Color", existingOrder.Color, input.Color);
                TrackChange("StartDate", existingOrder.StartDate.ToString(), input.StartDate.ToString());
                TrackChange("EndDate", existingOrder.EndDate.ToString(), input.EndDate.ToString());
                TrackChange("Quantity", existingOrder.Quantity.ToString(), input.Quantity.ToString());
                TrackChange("Image", existingOrder.Image, resolvedImage);
                TrackChange("Note", existingOrder.Note, input.Note);

                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Tracked {Count} field change(s) for OrderId {OrderId}", histories.Count, id);

                var updatedOrder = new Order
                {
                    Id = id,
                    UserId = userId,
                    OrderName = input.OrderName,
                    Type = input.Type,
                    Size = input.Size,
                    Color = input.Color,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    Quantity = input.Quantity,
                    Image = resolvedImage,
                    Note = input.Note,
                    Template = input.Templates?.Select(t => new OrderTemplate
                    {
                        TemplateName = t.TemplateName,
                        Type = t.Type,
                        File = t.File,
                        Quantity = t.Quantity,
                        Note = t.Note
                    }).ToList(),
                    Material = input.Materials?.Select(m => new OrderMaterial
                    {
                        MaterialName = m.MaterialName,
                        Image = m.Image,
                        Value = m.Value,
                        Uom = m.Uom,
                        Note = m.Note
                    }).ToList()
                };

                await _orderRepo.UpdateOrder(id, updatedOrder, histories);

                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Order {OrderId} updated successfully by UserId {UserId}", id, userId);

                return Ok($"Order '{id}' updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Put, ex,
                    "Error occurred while updating OrderId {OrderId}", id);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost("request-order-modification/{orderId}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> RequestOrderModification(int orderId)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Requesting modification for OrderId {OrderId}", orderId);
                if (orderId <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Invalid OrderId {OrderId} - must be greater than 0", orderId);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
                var existingOrder = await _orderRepo.GetOrderDetail(orderId);
                if (existingOrder is null)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Order {OrderId} not found", orderId);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order with id ' {orderId} ' not found" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }
                if (existingOrder.StatusName != OrderStatus_Constants.Pending)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Order {OrderId} cannot request modification - current status is '{Status}', required Chờ Xét Duyệt",
                        orderId, existingOrder.StatusName);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { "Only Chờ Xét Duyệt order can request modification" } }
                    };
                    return StatusCode(StatusCodes.Status403Forbidden, errorDetails);
                }
                var histories = new List<OHistoryUpdate>();
                void TrackChange(string field, string? oldVal, string? newVal)
                {
                    if (oldVal != newVal)
                        histories.Add(new OHistoryUpdate
                        {
                            OrderId = orderId,
                            FieldName = field,
                            OldValue = oldVal ?? string.Empty,
                            NewValue = newVal ?? string.Empty
                        });
                }
                TrackChange("Status",existingOrder.StatusName,OrderStatus_Constants.Modification);
                var updatedOrder = new Order
                {
                    Id = orderId,
                    Status = 2
                };
                await _orderRepo.RequestOrderModification(orderId, updatedOrder, histories);

                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Modification request for OrderId {OrderId} submitted successfully", orderId);

                return Ok($"Modification request for order '{orderId}' submitted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Put, ex,
                    "Error occurred while requesting modification for OrderId {OrderId}", orderId);
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost("deny-order/{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> DenyOrder(int orderId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Requesting deny for OrderId {OrderId}", orderId);
                if (orderId <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Invalid OrderId {OrderId} - must be greater than 0", orderId);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
                var existingOrder = await _orderRepo.GetOrderDetail(orderId);
                if (existingOrder is null)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Order {OrderId} not found", orderId);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order with id ' {orderId} ' not found" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }
                if (existingOrder.StatusName != OrderStatus_Constants.Pending)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "Order {OrderId} cannot request deny - current status is '{Status}', required Chờ Xét Duyệt",
                        orderId, existingOrder.StatusName);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { "Only Chờ Xét Duyệt order can request deny" } }
                    };
                    return StatusCode(StatusCodes.Status403Forbidden, errorDetails);
                }
                var histories = new List<OHistoryUpdate>();
                void TrackChange(string field, string? oldVal, string? newVal)
                {
                    if (oldVal != newVal)
                        histories.Add(new OHistoryUpdate
                        {
                            OrderId = orderId,
                            FieldName = field,
                            OldValue = oldVal ?? string.Empty,
                            NewValue = newVal ?? string.Empty
                        });
                }
                TrackChange("Status", existingOrder.StatusName, OrderStatus_Constants.Modification);
                var updatedOrder = new Order
                {
                    Id = orderId,
                    Status = 5
                };
                await _orderRepo.DenyOrder(userId, orderId, updatedOrder, histories);

                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Deny request for OrderId {OrderId} submitted successfully", orderId);

                return Ok($"Modification request for order '{orderId}' submitted successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put, ex.Message);

                return NotFound(new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Put, ex,
                    "Error occurred while requesting deny for OrderId {OrderId}", orderId);
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost("{orderId}/approve")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> ApproveOrder(int orderId)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Requesting approve for OrderId {OrderId}", orderId);
                if (orderId <= 0)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Order Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
                var existingOrder = await _orderRepo.GetOrderDetail(orderId);
                if (existingOrder is null)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Order with id '{orderId}' not found" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }
                if (existingOrder.StatusName == OrderStatus_Constants.Approved ||
                    existingOrder.StatusName == OrderStatus_Constants.Rejected)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post,
                        "Order {OrderId} has already been processed with status '{Status}'", orderId, existingOrder.StatusName);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status409Conflict,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { "This order request has already been processed." } }
                    };
                    return StatusCode(StatusCodes.Status409Conflict, errorDetails);
                }
                if (existingOrder.StatusName != OrderStatus_Constants.Pending)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post,
                        "Order {OrderId} cannot be approved - current status is '{Status}'", orderId, existingOrder.StatusName);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { "Only Chờ Xét Duyệt order can be approved" } }
                    };
                    return StatusCode(StatusCodes.Status403Forbidden, errorDetails);
                }
                var histories = new List<OHistoryUpdate>
                {
                    new OHistoryUpdate
                    {
                        OrderId = orderId,
                        FieldName = "Status",
                        OldValue = existingOrder.StatusName,
                        NewValue = OrderStatus_Constants.Approved
                    }
                };
                var updatedOrder = new Order
                {
                    Id = orderId,
                    Status = OrderStatus_Constants.Approved_ID
                };
                await _orderRepo.ApproveOrder(orderId, updatedOrder, histories);

                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Order {OrderId} approved successfully", orderId);

                return Ok($"Order '{orderId}' has been approved successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Post, ex.Message);
                return NotFound(new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Post, ex,
                    "Error occurred while approving OrderId {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }
    }
}