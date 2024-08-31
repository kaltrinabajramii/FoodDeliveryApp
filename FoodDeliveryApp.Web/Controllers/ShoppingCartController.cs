using FoodDeliveryApp.Domain.DomainModels;
using FoodDeliveryApp.Repository;
using FoodDeliveryApp.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Stripe;
using System.Security.Claims;

namespace FoodDeliveryApp.Web.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(IShoppingCartService? shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }
        public IActionResult Index()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return View(this._shoppingCartService.GetInfoShoppingCart(userId));
        }

        public IActionResult SuccessfulPayment()
        {
            return View();
        }
        public IActionResult FailedPayment()
        {
            return View();
        }
        public IActionResult Delete(Guid id)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = this._shoppingCartService.DeleteFromShoppingCart(userId, id);
            if (result)
            {
                return RedirectToAction("Index", "ShoppingCart");
            }
            else
            {
                return RedirectToAction("Index", "ShoppingCart");
            }
        }
        public Boolean Order() 
        {
            var userId=User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result=this._shoppingCartService.Order(userId);
            return result;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayOrder(string stripeEmail, string stripeToken)
        {
            StripeConfiguration.ApiKey = "sk_test_51P9ow2EbutLVS09jca4drAXy47BOqmYvmBOIvBZ3y6sXXc8FOZWTaKP3LuuxV2xeBwxrxg9EgKktiB3kXLr2V55K00xU0sqwii";
            var customerService = new CustomerService();
            var chargeService = new ChargeService();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = this._shoppingCartService.GetInfoShoppingCart(userId);

            var customer = customerService.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            var charge = chargeService.Create(new ChargeCreateOptions
            {
                Amount = (Convert.ToInt32(order.TotalPrice) * 100),
                Description = "Foodie Deliveries Payment",
                Currency = "mkd",
                Customer = customer.Id
            });
            if (charge.Status == "succeeded")
            {
                this.Order();
                return RedirectToAction("SuccessfulPayment", "ShoppingCart");
            }
            else {
                
                return RedirectToAction("FailedPayment", "ShoppingCart");
            
            }
          
        }
      


        

    }
}
