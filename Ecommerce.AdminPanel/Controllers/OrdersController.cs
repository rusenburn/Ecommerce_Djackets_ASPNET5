using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ecommerce.AdminPanel.Models;
using Ecommerce.Shared.Entities;
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


        public async Task<IActionResult> Index(FilterParams filterParams = null, string userId = "")
        {
            try
            {
                if (filterParams is null) filterParams = new FilterParams();
                Expression<Func<Order, bool>> predicate;
                if (string.IsNullOrEmpty(userId))
                {
                    predicate = order => true;
                }
                else
                {
                    predicate = order => order.UserId == userId;
                }
                IEnumerable<Order> orders = await _repo.Orders.GetOrdersWithOrderItemsAsync(filterParams.Page, filterParams.PageSize, filterParams.Search, predicate);

                var viewModel = new OrderIndexViewModel()
                {
                    FilterParams = filterParams,
                    Orders = orders,
                    UserId = userId
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
                return View(order);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}