using FoodDeliveryApp.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FoodDeliveryApp.Domain.Identity;
using FoodDeliveryApp.Repository.Interface;
using FoodDeliveryApp.Service.Interface;
using ClosedXML.Excel;
using System.IO;
using System.Linq;
using System.Text;

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
        [Route("Orders/OrderHistory/{userId?}")]
        public IActionResult OrderHistory(string userId)
        {

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userOrders = _orderService.GetOrdersForCustomer(userId);
            return View("History", userOrders);
        }


        [HttpGet]
        public FileContentResult ExportAllOrders()
        {
            string fileName = "Orders.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Orders");

                worksheet.Cell(1, 1).Value = "Order ID";
                worksheet.Cell(1, 2).Value = "Customer Username";
                worksheet.Cell(1, 3).Value = "Total Price";

                int maxFoodItems = 0; 

  
                var orders = _orderService.GetAllOrders(); 

                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cell(row, 1).Value = order.Id.ToString();
                    worksheet.Cell(row, 2).Value = order.Customer?.UserName ?? "N/A";

                    decimal totalPrice = 0;
                    int col = 4;

                    if (order.FoodItemsInOrder != null)
                    {
                        int foodItemIndex = 1;
                        foreach (var foodItem in order.FoodItemsInOrder)
                        {
                            decimal itemPrice = foodItem.FoodItem.Price * foodItem.Quantity;

                            worksheet.Cell(1, col).Value = $"Food Item {foodItemIndex}";
                            worksheet.Cell(row, col).Value = $"{foodItem.FoodItem.Name} x{foodItem.Quantity}";
                            col++;

                            if (foodItem.FoodItem.Extras != null && foodItem.FoodItem.Extras.Any())
                            {
                                StringBuilder extraList = new StringBuilder();
                                foreach (var extra in foodItem.FoodItem.Extras)
                                {
                                    extraList.Append(extra.Extra.Name + " ($" + extra.Price + "), ");
                                    itemPrice += extra.Price; 
                                }
                                worksheet.Cell(1, col).Value = $"Extras {foodItemIndex}";
                                worksheet.Cell(row, col).Value = extraList.ToString().TrimEnd(',', ' '); // Remove last comma
                                col++;
                            }

                            totalPrice += itemPrice;
                            foodItemIndex++;
                        }
                        maxFoodItems = Math.Max(maxFoodItems, foodItemIndex);
                    }

                    worksheet.Cell(row, 3).Value = totalPrice;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, contentType, fileName);
                }
            }
        }
    }


}