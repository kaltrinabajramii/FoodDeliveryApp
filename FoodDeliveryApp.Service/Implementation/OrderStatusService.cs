using FoodDeliveryApp.Repository.Interface;
using FoodDeliveryApp.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Service.Implementation
{
    public class OrderStatusService : IOrderStatusService
    {
       
        private readonly IRepository<Domain.DomainModels.Order> _orderRepository;


         public OrderStatusService(IRepository<Domain.DomainModels.Order> orderRepository)
        {
            
            _orderRepository = orderRepository;

        }
        /*this is a timer for changing Status of order */
        /*the status of the order changes every three minutes*/


        public void ViewOrderStatus(Guid orderId)
        {

            TimerCallback timerCallback = (state) =>
            {
                var order = _orderRepository.Get(orderId);
                this.UpdateOrderStatus(order);

            };
            Timer timer = new Timer(timerCallback, null, 0, 10000);
        }

        private void UpdateOrderStatus(Domain.DomainModels.Order order)
        {
            if (order.Status.Equals("Created") == true)
            {
                order.Status = (Domain.DomainModels.OrderStatus)1;
            }
            else if (order.Status.Equals("Ready") == true)
            {
                order.Status = (Domain.DomainModels.OrderStatus)2;
            }
            else if (order.Status.Equals("BeingDelivered") == true)
            {
                order.Status = (Domain.DomainModels.OrderStatus)3;
            }


        }





    }
}

