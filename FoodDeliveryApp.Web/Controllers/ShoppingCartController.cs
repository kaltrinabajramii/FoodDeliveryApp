using FoodDeliveryApp.Domain.DomainModels;
using FoodDeliveryApp.Domain.Email;
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
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IEmailService _emailService;

        public ShoppingCartController(
            IShoppingCartService shoppingCartService,
            IEmailService emailService)
        {
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action("Index", "ShoppingCart") });
            }

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
        public async Task<bool> Order()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _shoppingCartService.Order(userId);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayOrder(string stripeEmail, string stripeToken)
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
                await this.Order(); // Changed to await the Order call

                var emailMessage = new EmailMessage
                {
                    MailTo = stripeEmail,
                    Subject = "Order Confirmation - Foodie Deliveries",
                    Content = $"Thank you for your order!\n\n" +
                             $"Order Total: {order.TotalPrice} MKD\n" +
                             $"Order Status: Paid\n\n" +
                             "We'll notify you when your order is ready for delivery.",
                };

                await _emailService.SendEmailMessage(emailMessage);

                return RedirectToAction("SuccessfulPayment", "ShoppingCart");
            }
            else
            {
                var emailMessage = new EmailMessage
                {
                    MailTo = stripeEmail,
                    Subject = "Payment Failed - Foodie Deliveries",
                    Content = "We're sorry, but your payment could not be processed. Please try again or contact support."
                };

                await _emailService.SendEmailMessage(emailMessage);

                return RedirectToAction("FailedPayment", "ShoppingCart");
            }
        }





    }
}
