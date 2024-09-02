using FoodDeliveryApp.Repository;
using FoodDeliveryApp.Repository.Interface;
using FoodDeliveryApp.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodDeliveryApp.Domain;
using FoodDeliveryApp.Domain.DomainModels;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDeliveryApp.Service.Implementation
{
    public class OrderStatusService : IOrderStatusService
    {

        //scope here is used for Timer since it allows DBcontext to be used in the backround
        private readonly IServiceScopeFactory _scopeFactory;


        public OrderStatusService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /*this is a timer for changing Status of order 
        the status of the order changes every three minutes*/
        


        public void ViewOrderStatus(Guid orderId)
        {

            TimerCallback timerCallback = (state) =>
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var order = context.Orders.SingleOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        this.UpdateOrderStatus(order, context);
                    }
                }

            };
            //for testing it is set to 10 seconds on deploment it should be set to 60 000
            Timer timer = new Timer(timerCallback,
                                    null,
                                    0,
                                    10000);
        }

        private void UpdateOrderStatus(Order order, ApplicationDbContext context)
        {
            switch (order.Status)
            {
                case OrderStatus.Created:
                    order.Status = OrderStatus.Ready;
                    break;

                case OrderStatus.Ready:
                    order.Status = OrderStatus.BeingDelivered;
                    break;

                case OrderStatus.BeingDelivered:
                    order.Status = OrderStatus.Delivered;
                    break;

                case OrderStatus.Delivered:
                    // Order is already delivered 
                    //Email should be sent
                    return;
            }

            context.Orders.Update(order);
            context.SaveChanges();
        }

        // This code displayes latest order not !!



    }
}

