using FoodDeliveryApp.Domain.DomainModels;
using FoodDeliveryApp.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Repository.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext context;
        private DbSet<Order> entities;

        public OrderRepository(ApplicationDbContext context)
        {
            this.context = context;
            entities = context.Set<Order>();
        }

        public List<Order> GetAllOrders()
        {
            return entities
                .Include(z => z.FoodItemsInOrder)
                .Include(z => z.Customer)
                .Include(z => z.Restaurant)
                .Include("FoodItemsInOrder.FoodItem")
                .ToList();
        }

        public Order GetDetailsForOrder(BaseEntity id)
        {
            return entities
                .Include(z => z.FoodItemsInOrder)
                .Include(z => z.Customer)
                .Include(z=>z.Restaurant)
                .Include("FoodItemsInOrder.FoodItem")
                .SingleOrDefaultAsync(z => z.Id == id.Id).Result;
        }
    }
}
