using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.APIUI.Dtos;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Logging;
// using Stripe;
// using Stripe;

namespace Ecommerce.APIUI.Controllers
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly IMapper _mapper;
        private readonly IOrderHelperService _orderHelperService;
        private readonly ILogger _logger;

        public OrdersController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOrderHelperService orderHelperService,
            ILogger<OrdersController> logger)
        {
            _repo = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _orderHelperService = orderHelperService ?? throw new ArgumentNullException(nameof(orderHelperService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> CreatePaymentCharge(OrderInputDto orderDto)
        {
            try
            {
                Order order = _mapper.Map<OrderInputDto, Order>(orderDto);
                order.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                order = await _orderHelperService.HandleOrderAsync(order);
                return Created("/", order);
            }

            catch (System.Exception e)
            {
                _logger.LogCritical(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId is null) return NotFound();
                var orders = await _repo.Orders.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}