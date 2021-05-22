using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.AdminPanel.Models;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Enums;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.AdminPanel.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _repo;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IUnitOfWork unitOfWork, ILogger<OrdersController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repo = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }


        public async Task<IActionResult> Index(OrdersIndexFilterParams filterParams = null)
        {
            try
            {
                if (filterParams is null)
                {
                    filterParams = new OrdersIndexFilterParams();
                    filterParams.Shipping = ShippingInfoFilter.All;
                } 
                Expression<Func<Order, bool>> predicate;
                if (string.IsNullOrEmpty(filterParams.UserId))
                {
                    predicate = order => true;
                }
                else
                {
                    predicate = order => order.UserId == filterParams.UserId;
                }
                IEnumerable<Order> orders;
                if (filterParams.Shipping == ShippingInfoFilter.All)
                {
                    orders = await _repo.Orders.GetOrdersWithOrderItemsAndShippingInfoAsync(filterParams.Page, filterParams.PageSize, filterParams.Search, predicate);
                }else
                {
                    orders = await _repo.Orders.GetOrdersByShippingInfoFilter(filterParams.Shipping,filterParams.Page,filterParams.PageSize,filterParams.Search,predicate);
                }
                var viewModel = new OrderIndexViewModel()
                {
                    FilterParams = filterParams,
                    Orders = orders,
                    // TODO: remove if not needed
                    UserId = filterParams.UserId
                };
                return View(viewModel);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        public async Task<IActionResult> Details(int id)
        {
            try
            {
                Order order = await _repo.Orders.GetOneAsync(id);
                if (order is null) return BadRequest();
                order.OrderItems =
                    (ICollection<OrderItem>)
                    await _repo.OrderItems.GetOrderItemsWithProductsAndCategoriesAsync(
                        oi => oi.OrderId == id);
                order.ShippingInfo = await _repo.ShippingInfoSet.GetShippingInfoByOrderId(order.Id);
                return View(order);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsShipped(int orderId)
        {
            try
            {
                if (orderId <= 0) return NotFound();
                var order = await _repo.Orders.GetOneAsync(orderId);
                if (order is null) return NotFound();
                var shippingInfo = await _repo.ShippingInfoSet.GetShippingInfoByOrderId(orderId);
                if (shippingInfo is not null) return BadRequest();
                shippingInfo = new ShippingInfo()
                {
                    ShippedDate = DateTime.UtcNow,
                    OrderId = order.Id
                };
                shippingInfo = await _repo.ShippingInfoSet.CreateOneAsync(shippingInfo);
                await _repo.SaveChangesAsync();
                OrdersIndexFilterParams parameters = new OrdersIndexFilterParams();
                parameters.Shipping = ShippingInfoFilter.ShippedNotArrived;
                return RedirectToAction(nameof(Index), parameters);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsArrived(int orderId)
        {
            try
            {
                if (orderId <= 0) return NotFound();
                var order = await _repo.Orders.GetOneAsync(orderId);
                if (order is null) return NotFound();
                var shippingInfo = await _repo.ShippingInfoSet.GetShippingInfoByOrderId(orderId);
                if (shippingInfo is null || shippingInfo.ShippedDate is null || shippingInfo.ArrivalDate is not null)
                    return BadRequest();
                shippingInfo.ArrivalDate = DateTime.UtcNow;
                shippingInfo = await _repo.ShippingInfoSet.UpdateOneAsync(shippingInfo.Id, shippingInfo);
                await _repo.SaveChangesAsync();

                OrdersIndexFilterParams parameters = new OrdersIndexFilterParams();
                parameters.Shipping = ShippingInfoFilter.Arrived;
                return RedirectToAction(nameof(Index), parameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}