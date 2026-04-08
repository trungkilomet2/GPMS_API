using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using GPMS.INFRASTRUCTURE.DataContext;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
        private readonly IEmailRepositories _emailRepo;
        private readonly IUserRepositories _userRepo;

        public OrderController(IOrderRepositories orderRepo, ILogger<OrderController> logger, ICloudinaryService cloudinaryService, IEmailRepositories emailRepo, IUserRepositories userRepo)
        {
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _emailRepo = emailRepo;
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        private ActionResult? ValidateOrderQuery(OrderRequestDTO input)
        {
            var validStatuses = new[]
            {
                OrderStatus_Constants.Pending, OrderStatus_Constants.Modification,
                OrderStatus_Constants.Approved, OrderStatus_Constants.Rejected, OrderStatus_Constants.Cancelled
            };

            if (!string.IsNullOrEmpty(input.Status))
            {
                var matched = validStatuses.FirstOrDefault(s =>
                    s.Normalize(NormalizationForm.FormC).Equals(
                        input.Status.Normalize(NormalizationForm.FormC), StringComparison.OrdinalIgnoreCase));

                if (matched is null)
                {
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

                input.Status = matched;
            }

            if (input.StartDateFrom.HasValue && input.StartDateTo.HasValue && input.StartDateFrom > input.StartDateTo)
            {
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

            return null;
        }

        private IEnumerable<Order> ApplyOrderFilterSort(IEnumerable<Order> source, OrderRequestDTO input)
        {
            if (!string.IsNullOrEmpty(input.FilterQuery))
                source = source.Where(o => o.OrderName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(input.Status))
                source = source.Where(o => o.StatusName == input.Status);

            if (input.StartDateFrom.HasValue)
                source = source.Where(o => o.StartDate >= input.StartDateFrom.Value);

            if (input.StartDateTo.HasValue)
                source = source.Where(o => o.StartDate <= input.StartDateTo.Value);

            var isDesc = string.IsNullOrEmpty(input.SortColumn)
                || string.Equals(input.SortOrder, "DESC", StringComparison.OrdinalIgnoreCase);

            return input.SortColumn?.ToLower() switch
            {
                "startdate" => isDesc ? source.OrderByDescending(o => o.StartDate) : source.OrderBy(o => o.StartDate),
                "enddate"   => isDesc ? source.OrderByDescending(o => o.EndDate)   : source.OrderBy(o => o.EndDate),
                "status"    => isDesc ? source.OrderByDescending(o => o.StatusName) : source.OrderBy(o => o.StatusName),
                "quantity"  => isDesc ? source.OrderByDescending(o => o.Quantity)  : source.OrderBy(o => o.Quantity),
                "name"      => isDesc ? source.OrderByDescending(o => o.OrderName) : source.OrderBy(o => o.OrderName),
                _           => isDesc ? source.OrderByDescending(o => o.Id)        : source.OrderBy(o => o.Id)
            };
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

                var validationResult = ValidateOrderQuery(input);
                if (validationResult is not null) return validationResult;

                var result = await _orderRepo.GetAllOrders();
                result = ApplyOrderFilterSort(result, input);

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

                var validationResult = ValidateOrderQuery(input);
                if (validationResult is not null) return validationResult;

                var result = await _orderRepo.GetOrdersByUserId(userId);
                result = ApplyOrderFilterSort(result, input);

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

                if (User.IsInRole("Customer"))
                {
                    var requesterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                    if (order.UserId != requesterId)
                        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                        {
                            Detail = "You are not allowed to view this order.",
                            Status = StatusCodes.Status403Forbidden
                        });
                }

                var orderUser = await _userRepo.GetUserById(order.UserId);

                var data = new OrderDetailDTO
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    UserFullName = orderUser?.FullName,
                    UserPhone = orderUser?.PhoneNumber,
                    UserLocation = orderUser?.Location,
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

                if (User.IsInRole("Customer"))
                {
                    var requesterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                    if (order.UserId != requesterId)
                        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                        {
                            Detail = "You are not allowed to view this order's history.",
                            Status = StatusCodes.Status403Forbidden
                        });
                }

                if (!order.Histories.Any())
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Get,
                        "No history records found for OrderId {OrderId}", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"No history found for order '{id}'" } }
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

        [HttpPost("create-order")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDTO? input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Tạo đơn hàng cho khách hàng có Id là: {UserId}", input?.UserId);
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                if (ModelState.IsValid)
                {
                    var newOrder = new Order
                    {
                        UserId = input.UserId,
                        Image = input.Image,
                        OrderName = input.OrderName,
                        Size = input.Sizes?.Select(s => new OrderSize
                        {
                            SizeId = s.SizeId,
                            Color = s.Color,
                            Quantity = s.Quantity,
                            OrderSizeStatusId = OrderSizeStatus_Constants.Pending_Id
                        }).ToList(),
                        StartDate = input.StartDate,
                        EndDate = input.EndDate,
                        Quantity = input.Quantity,
                        Cpu = input.Cpu,
                        Note = input.Note,
                        Status = OrderStatus_Constants.Pending_ID,
                        CreateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                        Material = input.Materials?.Select(m => new OrderMaterial
                        {
                            MaterialName = m.MaterialName,
                            Image = m.Image,                            
                            Value = m.Value,
                            Color = m.Color,
                            Uom = m.Uom,
                            Note = m.Note
                        }).ToList(),

                        Template = input.Templates?.Select(t => new OrderTemplate
                        {
                            TemplateName = t.TemplateName,
                            Type = t.Type,
                            File = t.File,
                            Note = t.Note
                        }).ToList(),
                    };

                    var result = await _orderRepo.CreateOrder(newOrder);
                    var owner = await _userRepo.GetOwner();
                        if (owner != null)
                        {
                            var subject = $"Đơn hàng mới đã được tạo với Id là - {result.Id}";
                            var body = $@"
                                          <h3>Chi tiết</h3>
                                           <p>Mã đơn hàng: {result.Id}</p>
                                           <p>Tên đơn hàng: {result.OrderName}</p>
                                           <p>Số lượng: {result.Quantity}</p>
                                           <p>Giá từng sản phẩm: {result.Cpu}</p>
                                           <p>Ghi chú: {result.Note}</p>
                                           <p>Trạng thái: {OrderStatus_Constants.Pending}</p>";

                            await _emailRepo.SendEmailAsync(owner.Email, subject, body, EmailType.OrderNotification);
                        }                
                    _logger.LogInformation(CustomLogEvents.OrderController_Post,
                        "Order {OrderId} được tạo thành công cho khách hàng với Id là: {UserId}",
                        result.Id, input.UserId);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Order '{result.Id}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post,
                        "Lỗi model state khi tạo đơn hàng cho khách hàng với Id là: {UserId}",
                        input?.UserId);

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
                _logger.LogError(CustomLogEvents.OrderController_Post, ex,
                    "Lỗi khi tạo đơn hàng cho khách hàng với Id là: {UserId}", input?.UserId);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                return StatusCode(StatusCodes.Status500InternalServerError,
                    exceptionDetails.Detail);
            }
        }

        [HttpPost("create-manual-order")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<ActionResult> CreateManualOrder([FromBody] CreateOrderDTO? input, [FromBody] CreateGuest? guest)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Tạo đơn hàng cho khách hàng có Id là: {UserId}", input?.UserId);
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                if (ModelState.IsValid)
                {
                    var newOrder = new Order
                    {
                        UserId = input.UserId,
                        Image = input.Image,
                        OrderName = input.OrderName,
                        Size = input.Sizes?.Select(s => new OrderSize
                        {
                            SizeId = s.SizeId,
                            Color = s.Color,
                            Quantity = s.Quantity,
                            OrderSizeStatusId = OrderSizeStatus_Constants.Pending_Id
                        }).ToList(),
                        StartDate = input.StartDate,
                        EndDate = input.EndDate,
                        Quantity = input.Quantity,
                        Cpu = input.Cpu,
                        Note = input.Note,
                        Status = OrderStatus_Constants.Pending_ID,
                        CreateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                        Material = input.Materials?.Select(m => new OrderMaterial
                        {
                            MaterialName = m.MaterialName,
                            Image = m.Image,
                            Value = m.Value,
                            Color = m.Color,
                            Uom = m.Uom,
                            Note = m.Note
                        }).ToList(),

                        Template = input.Templates?.Select(t => new OrderTemplate
                        {
                            TemplateName = t.TemplateName,
                            Type = t.Type,
                            File = t.File,
                            Note = t.Note
                        }).ToList(),
                    };
                    var guestUser = new Guest
                    {
                        FullName = guest.FullName,
                        PhoneNumber = guest.PhoneNumber,
                        Address = guest.Address
                    };
                    var result = await _orderRepo.CreateManualOrder(newOrder, guestUser);
                    var owner = await _userRepo.GetOwner();
                    if (owner != null)
                    {
                        var subject = $"Đơn hàng mới đã được tạo với Id là - {result.Id}";
                        var body = $@"
                                          <h3>Chi tiết</h3>
                                           <p>Mã đơn hàng: {result.Id}</p>
                                           <p>Tên đơn hàng: {result.OrderName}</p>
                                           <p>Số lượng: {result.Quantity}</p>
                                           <p>Giá từng sản phẩm: {result.Cpu}</p>
                                           <p>Ghi chú: {result.Note}</p>
                                           <p>Trạng thái: {OrderStatus_Constants.Pending}</p>";

                        await _emailRepo.SendEmailAsync(owner.Email, subject, body, EmailType.OrderNotification);
                    }
                    _logger.LogInformation(CustomLogEvents.OrderController_Post,
                        "Order {OrderId} được tạo thành công cho khách hàng với Id là: {UserId}",
                        result.Id, input.UserId);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Order '{result.Id}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post,
                        "Lỗi model state khi tạo đơn hàng cho khách hàng với Id là: {UserId}",
                        input?.UserId);

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
                _logger.LogError(CustomLogEvents.OrderController_Post, ex,
                    "Lỗi khi tạo đơn hàng cho khách hàng với Id là: {UserId}", input?.UserId);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                return StatusCode(StatusCodes.Status500InternalServerError,
                    exceptionDetails.Detail);
            }
        }

        // api/order/{orderId}/materials
        [HttpPost("{orderId}/materials", Name = "Add material to order")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> AddMaterial(int orderId, [FromForm] CreateMaterialDTO? input, IFormFile? imageFile)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Adding material to OrderId {OrderId}", orderId);

                if (ModelState.IsValid)
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                    var existingOrder = await _orderRepo.GetOrderDetail(orderId);

                    if (existingOrder is null)
                    {
                        _logger.LogWarning(CustomLogEvents.OrderController_Post,
                            "Order {OrderId} not found when adding material", orderId);
                        var notFoundDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status404NotFound,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                        };
                        notFoundDetails.Errors = new Dictionary<string, string[]>
                        {
                            { "id", new[] { $"Order with id '{orderId}' not found" } }
                        };
                        return StatusCode(StatusCodes.Status404NotFound, notFoundDetails);
                    }

                    if (existingOrder.UserId != userId)
                    {
                        _logger.LogWarning(CustomLogEvents.OrderController_Post,
                            "User {UserId} attempted to add material to order {OrderId} owned by {OwnerId}",
                            userId, orderId, existingOrder.UserId);
                        var forbidDetails = new ValidationProblemDetails(ModelState)
                        {
                            Status = StatusCodes.Status403Forbidden,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                        };
                        forbidDetails.Errors = new Dictionary<string, string[]>
                        {
                            { "authorization", new[] { "You do not have permission to add material to this order" } }
                        };
                        return StatusCode(StatusCodes.Status403Forbidden, forbidDetails);
                    }

                    string? imageUrl = null;
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadResult = await _cloudinaryService.UploadImageAsync(
                            imageFile,
                            CloudinaryConstrants.Cloudinary_Supplied_Image_Folder);
                        imageUrl = uploadResult.Url;
                    }

                    var material = new OMaterial
                    {
                        Name = input.MaterialName,
                        Color = input.Color,
                        Image = imageUrl ?? input.Image,
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

                var updateInput = new UpdateOrderInput
                {
                    OrderName = input!.OrderName,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    Quantity = input.Quantity,
                    Image = input.Image,
                    Note = input.Note,
                    Sizes = input.Sizes?.Select(s => new OrderSize
                    {
                        SizeId = s.SizeId,
                        Color = s.Color,
                        Quantity = s.Quantity,
                        OrderSizeStatusId = OrderSizeStatus_Constants.Pending_Id
                    }).ToList(),
                    Templates = input.Templates?.Select(t => new OrderTemplate
                    {
                        TemplateName = t.TemplateName,
                        Type = t.Type,
                        File = t.File,
                        Note = t.Note
                    }).ToList(),
                    Materials = input.Materials?.Select(m => new OrderMaterial
                    {
                        MaterialName = m.MaterialName,
                        Color = m.Color,
                        Image = m.Image,
                        Value = m.Value,
                        Uom = m.Uom,
                        Note = m.Note
                    }).ToList()
                };

                await _orderRepo.UpdateOrder(id, userId, updateInput);

                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Order {OrderId} updated successfully by UserId {UserId}", id, userId);

                return Ok($"Order '{id}' updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put,
                    "Order {OrderId} not found", id);
                return StatusCode(StatusCodes.Status404NotFound, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put,
                    "Invalid date input for Order {OrderId}: {Message}", id, ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put,
                    "Order {OrderId} status error: {Message}", id, ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put,
                    "Unauthorized access to update Order {OrderId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Put, ex,
                    "Error occurred while updating OrderId {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }

        [HttpPut("request-order-modification/{orderId}")]
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
                        Status = StatusCodes.Status403Forbidden,
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
                TrackChange("Status", existingOrder.StatusName, OrderStatus_Constants.Modification);
                var updatedOrder = new Order
                {
                    Id = orderId,
                    Status = 2
                };
                await _orderRepo.RequestOrderModification(orderId, updatedOrder, histories);
                var user = await _userRepo.GetUserById(existingOrder.UserId);
                if (user == null)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Put,
                        "User {UserId} not found for OrderId {OrderId} when sending modification email",
                        existingOrder.UserId, orderId);
                }
                else
                {
                    await _emailRepo.SendEmailAsync(user.Email, "Thông báo yêu cầu chỉnh sửa đơn hàng",
                            $"Đơn hàng với Id: '{existingOrder.Id}' đã bị yêu cầu chỉnh sửa.",EmailType.OrderNotification);
                    _logger.LogInformation(CustomLogEvents.OrderController_Put,
                        "Modification request for OrderId {OrderId} submitted successfully", orderId);
                }
                return StatusCode(StatusCodes.Status200OK, $"Modification request submitted successfully");                
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

        [HttpPut("deny-order/{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> DenyOrder(int orderId)
        {
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                await _orderRepo.DenyOrder(userId, orderId);

                _logger.LogInformation(CustomLogEvents.OrderController_Put,
                    "Deny request for OrderId {OrderId} submitted successfully", orderId);

                return Ok($"Deny request for order '{orderId}' submitted successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put,
                    "Order {OrderId} not found", orderId);
                return StatusCode(StatusCodes.Status404NotFound, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put,
                    "Unauthorized deny attempt on OrderId {OrderId}", orderId);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Put,
                    "Order {OrderId} status error: {Message}", orderId, ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.OrderController_Put, ex,
                    "Error occurred while requesting deny for OrderId {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }

        [HttpPut("{orderId}/approve")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> ApproveOrder(int orderId)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Requesting approve for OrderId {OrderId}", orderId);

                if (orderId <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.OrderController_Post,
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

                await _orderRepo.ApproveOrder(orderId);

                _logger.LogInformation(CustomLogEvents.OrderController_Post,
                    "Order {OrderId} approved successfully", orderId);

                return Ok($"Order '{orderId}' has been approved successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Post,
                    "Order {OrderId} not found", orderId);
                return StatusCode(StatusCodes.Status404NotFound, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }
            catch (InvalidOperationException ex) when (ex.Message == "This order request has already been processed.")
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Post,
                    "Order {OrderId} has already been processed", orderId);
                return StatusCode(StatusCodes.Status409Conflict, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.OrderController_Post,
                    "Order {OrderId} status error: {Message}", orderId, ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
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