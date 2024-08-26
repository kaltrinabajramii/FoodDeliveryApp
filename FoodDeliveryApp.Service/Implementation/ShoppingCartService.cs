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

namespace FoodDeliveryApp.Service.Implementation
{
    public class ShoppingCartService : IShoppingCartService
    {

        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IRepository<ShoppingCart> _shoppingCartRepository;
        private readonly ApplicationDbContext _context;
        private readonly  IRepository<FoodItemInCart> _foodItemInCartRepository;
        private readonly ICustomerRepository _userRepository;
        
        public ShoppingCartService(IRepository<ShoppingCart> shoppingCartRepository,
            IRepository<FoodItemInCart> foodItemInCartRepository,
            ApplicationDbContext context,
            ICustomerRepository userRepository, ILogger<ShoppingCartService> logger)
        {
            _foodItemInCartRepository = foodItemInCartRepository;
            _userRepository = userRepository;
            _shoppingCartRepository=shoppingCartRepository;
            _logger = logger;
            _context = context;
            
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
                .Sum(item => (double)(item.Quantity * item.FoodItem.Price)) ?? 0;

            ShoppingCartDTO model = new ShoppingCartDTO()
            {
                FoodItemsInCarts = shoppingCart.FoodItemsInCart?.ToList() ?? new List<FoodItemInCart>(),
                TotalPrice = totalPrice,
            };

            return model;
        }

    }
}
