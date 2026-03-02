using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Owner")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepositories _orderRepo;

        public OrderController(IOrderRepositories orderRepo)
        {
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
        }

        [HttpGet]
        public async Task<ActionResult<RestDTO<IEnumerable<Order>>>> GetOrders([FromQuery] RequestDTO<Order> input)
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
                    new LinkDTO(
                        Url.Action(null, "Order", new { input.PageIndex, input.PageSize }, Request.Scheme)!,
                        "self",
                        "GET"
                    )
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestDTO<Order>>> GetOrderById(int id)
        {
            var result = await _orderRepo.GetOrderById(id);

            if (result is null)
                return NotFound();

            return Ok(new RestDTO<Order>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action("GetOrderById", "Order", new { id }, Request.Scheme)!,
                        "self",
                        "GET"
                    )
                }
            });
        }
    }
}