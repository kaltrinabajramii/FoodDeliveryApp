﻿using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryApp.Web.Controllers
{
    public class ShoppingCartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
