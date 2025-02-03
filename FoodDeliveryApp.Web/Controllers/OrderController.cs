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

        public IActionResult OrderHistory(String? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var userOrders = _orderService.GetAllOrders().Where(k => k.CustomerId == userId).ToList();

            return View(userOrders);
        }
        
    }
}
