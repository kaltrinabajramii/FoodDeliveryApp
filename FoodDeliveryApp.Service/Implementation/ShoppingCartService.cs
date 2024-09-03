using FoodDeliveryApp.Domain.DomainModels;
using FoodDeliveryApp.Domain.DTO;
using FoodDeliveryApp.Repository;
using FoodDeliveryApp.Repository.Implementation;
using FoodDeliveryApp.Repository.Interface;
using FoodDeliveryApp.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using FoodDeliveryApp.Domain.Email;

namespace FoodDeliveryApp.Service.Implementation
{
    public class ShoppingCartService : IShoppingCartService
    {

        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IRepository<ShoppingCart> _shoppingCartRepository;
        private readonly ApplicationDbContext _context;
        private readonly  IRepository<FoodItemInCart> _foodItemInCartRepository;
        private readonly ICustomerRepository _userRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IEmailService _emailService;
        
        public ShoppingCartService(IRepository<ShoppingCart> shoppingCartRepository,
            IRepository<FoodItemInCart> foodItemInCartRepository,
            ApplicationDbContext context, IRepository<Order> orderRepository,
            ICustomerRepository userRepository, ILogger<ShoppingCartService> logger,
            IEmailService emailService)
        {
            _foodItemInCartRepository = foodItemInCartRepository;
            _userRepository = userRepository;
            _shoppingCartRepository=shoppingCartRepository;
            _logger = logger;
            _context = context;
            _orderRepository = orderRepository;
            _emailService = emailService;
            
        }
        
        public bool AddToShoppingConfirmed(FoodItemInCart model, string userId)
        {
            var user = this._userRepository.GetCustomer(userId);
            var shoppingCart = user.ShoppingCart;
            FoodItemInCart itemToAdd = new FoodItemInCart
            {
                Id = Guid.NewGuid(),
                FoodItem = model.FoodItem,
                FoodItemId = model.FoodItemId,
                ShoppingCart = shoppingCart,
                ShoppingCartId = shoppingCart.Id,
                Quantity = model.Quantity
            };

            _foodItemInCartRepository.Insert(itemToAdd);
            return true;
        }

        public bool DeleteFromShoppingCart(string userId, Guid FoodId)
        {
            if (!string.IsNullOrEmpty(userId) && FoodId != null)
            {
                var loggedInUser = this._userRepository.GetCustomer(userId);

                var userShoppingCart = loggedInUser.ShoppingCart;

                var itemToDelete = userShoppingCart.FoodItemsInCart.Where(z => z.FoodItemId.Equals(FoodId)).FirstOrDefault();

                userShoppingCart.FoodItemsInCart.Remove(itemToDelete);

                this._shoppingCartRepository.Update(userShoppingCart);

                return true;
            }
            return false;
        }

        public ShoppingCartDTO GetInfoShoppingCart(string userId)
        {
            var user = this._userRepository.GetCustomer(userId);
            var shoppingCart = _shoppingCartRepository.GetAll()
                .FirstOrDefault(c => c.Id == user.ShoppingCart.Id);

            if (shoppingCart == null)
            {
                return new ShoppingCartDTO()
                {
                    FoodItemsInCarts = new List<FoodItemInCart>(),
                    TotalPrice = 0
                };
            }

            // Manually load related entities
            foreach (var foodItemInCart in shoppingCart.FoodItemsInCart)
            {
                _context.Entry(foodItemInCart)
                    .Reference(fic => fic.FoodItem)
                .Load();

                _context.Entry(foodItemInCart.FoodItem)
                    .Reference(fi => fi.Restaurant)
                    .Load();
            }

            double totalPrice = shoppingCart.FoodItemsInCart?
                .Sum(item => (double)((item.Quantity * item.FoodItem.Price)+item.FoodItem.Restaurant.BaseDeliveryFee)) ?? 0;

            ShoppingCartDTO model = new ShoppingCartDTO()
            {
                FoodItemsInCarts = shoppingCart.FoodItemsInCart?.ToList() ?? new List<FoodItemInCart>(),
                TotalPrice = totalPrice,
            };

            return model;
        }

        public bool Order(string userID)
        {
            if (!string.IsNullOrEmpty(userID))
            {
                var loggedInUser = this._userRepository.GetCustomer(userID);
                var userCart = loggedInUser.ShoppingCart;
                /*Email here for order*/
                EmailMessage emailmessage = new EmailMessage();
                emailmessage.Subject = "Ordered succesfully!";
                emailmessage.MailTo = loggedInUser.Email;

                Order order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = userID,
                    OrderDate = DateTime.Now,
                    Status = 0,
                    TotalAmount = userCart.FoodItemsInCart.Sum(f => f.FoodItem.Price * f.Quantity),
                    RestaurantId = userCart.FoodItemsInCart.First().FoodItem.RestaurantId,
                   
                };
                order.FoodItemsInOrder=userCart.FoodItemsInCart.Select(f=> new FoodItemInOrder
                {
                    Id=Guid.NewGuid(),
                    OrderId=order.Id,
                    FoodItemId=f.FoodItemId,
                    FoodItem=f.FoodItem,
                    Quantity=f.Quantity,
                   
                }).ToList();

                StringBuilder sb = new StringBuilder();

                var totalPrice = 0.0;

                sb.AppendLine("Your order is completed. The order conatins: ");
                sb.AppendLine();

                foreach(var item in order.FoodItemsInOrder)
                {
                    sb.AppendLine($"- {item.FoodItem.Name} (Quantity: {item.Quantity}, Price: {item.FoodItem.Price:C})");
                }
                sb.AppendLine();
                sb.AppendLine("Total price for your order: "+ order.TotalAmount.ToString("C"));
                sb.AppendLine();
                sb.AppendLine("Thank you for ordering!");

                emailmessage.Content = sb.ToString();
                this._emailService.SendEmailMessage(emailmessage);
                       

                this._orderRepository.Insert(order);
                userCart.FoodItemsInCart.Clear();
                this._userRepository.Update(loggedInUser);

               


               

                return true;

                

                
            }
            return false;
        }
    }
}
