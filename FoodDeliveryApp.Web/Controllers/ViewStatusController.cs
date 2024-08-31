﻿using FoodDeliveryApp.Repository.Interface;
using FoodDeliveryApp.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDeliveryApp.Web.Controllers
{
    public class ViewStatusController : Controller
    {
        private readonly IOrderStatusService _orderStatus;
        private readonly ICustomerRepository _customerRepository;
        

        public ViewStatusController(IOrderStatusService orderStatus,ICustomerRepository customerRepository)
        {
            this._orderStatus = orderStatus;
            this._customerRepository = customerRepository;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid user;
            Guid.TryParse(userId, out user);
            _orderStatus.ViewOrderStatus(user);
            return PartialView("_ViewStatusModal");
        }
    }
}