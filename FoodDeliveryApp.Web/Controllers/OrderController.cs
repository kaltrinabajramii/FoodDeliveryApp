using FoodDeliveryApp.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FoodDeliveryApp.Domain.Identity;
using FoodDeliveryApp.Repository.Interface;
using FoodDeliveryApp.Service.Interface;

namespace FoodDeliveryApp.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOrderService _orderService;

        public OrderController(ILogger<HomeController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
            
        }
        [Route("Orders/OrderHistory/{userId?}")]
        public IActionResult OrderHistory(string userId)
        {
          
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userOrders = _orderService.GetOrdersForCustomer(userId);
            return View("History", userOrders); 
        }


    }
}
