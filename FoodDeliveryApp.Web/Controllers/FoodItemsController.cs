using Microsoft.AspNetCore.Mvc;
using FoodDeliveryApp.Domain.DomainModels;
using FoodDeliveryApp.Service.Interface;
using FoodDeliveryApp.Domain.DTO;
using FoodDeliveryApp.Service.Implementation;
using System.Security.Claims;

namespace FoodDeliveryApp.Web.Controllers
{
    public class FoodItemsController : Controller
    {
        private readonly IFoodItemService _foodItemService;
        private readonly IRestaurantService _restaurantService;
        private readonly IExtraService _extraService;
        private readonly IShoppingCartService _shoppingCartService;

        public FoodItemsController(IFoodItemService foodItemService, IRestaurantService restaurantService, IExtraService extraService, IShoppingCartService shoppingCartService)
        {
            _foodItemService = foodItemService;
            _restaurantService = restaurantService;
            _extraService = extraService;
            _shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateNewFoodItem(Guid restaurantId)
        {
            var restaurant = _restaurantService.GetDetails(restaurantId);
            if (restaurant == null)
            {
                return NotFound();
            }

            var viewModel = new CreateFoodItemViewModel
            {
                RestaurantId = restaurantId,
                RestaurantName = restaurant.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNewFoodItem(CreateFoodItemViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
               
                _foodItemService.CreateNewFoodItem(viewModel);

                return RedirectToAction("Details", "Restaurants", new { id = viewModel.RestaurantId });
            }

            return View(viewModel);
        }

        

        public IActionResult Delete(Guid id)
        {
            var foodItem = _foodItemService.GetFoodItemById(id);
            if (foodItem == null)
            {
                return NotFound();
            }
            
            return View(foodItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var foodItem = _foodItemService.GetFoodItemById(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            _foodItemService.DeleteFoodItem(id);
         
            return RedirectToAction("Details", "Restaurants", new { id = foodItem.RestaurantId });
        }

        // GET: FoodItems/Edit/5
        public IActionResult Edit(Guid id)
        {
            var foodItem = _foodItemService.GetFoodItemById(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            var viewModel = new EditFoodItemViewModel
            {
                FoodItemId = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = foodItem.Price,
                ImageUrl = foodItem.ImageUrl,
                RestaurantId = foodItem.RestaurantId,
                ExtraInFoodItems = foodItem.Extras.Select(e => new ExtraInFoodItemViewModel
                {
                    Id = e.Id,
                    ExtraId = e.ExtraId,
                    FoodItemId = e.FoodItemId,
                    ExtraName = e.Extra.Name,
                    Price = e.Price
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditFoodItemViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _foodItemService.UpdateFoodItem(viewModel);
                    return RedirectToAction("Details", "Restaurants", new { id = viewModel.RestaurantId });
                }
                catch (ArgumentException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the food item: " + ex.Message);
                }
            }
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> AddToCartConfirmed(FoodItemInCart model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            _shoppingCartService.AddToShoppingConfirmed(model, userId);
            return RedirectToAction("Index", "ShoppingCart");
        }
        public async Task<IActionResult> AddToCart(Guid? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            // Retrieve the FoodItem using the provided Id
            var foodItem = _foodItemService.GetFoodItemById((Guid)Id);

            if (foodItem == null)
            {
                return NotFound();
            }

            // Create a new instance of FoodItemInCart
            FoodItemInCart foodItemInCart = new FoodItemInCart
            {
                // Set the FoodItemId to the retrieved FoodItem's Id
                FoodItemId = foodItem.Id
            };

            // Pass the FoodItemInCart object to the view
            return View(foodItemInCart);
        }
        public IActionResult LoadAddToCartModal(Guid foodItemId)
        {
            var model = new FoodItemInCart
            {
                FoodItemId = foodItemId,
                // Initialize other properties as needed
            };
            return PartialView("_AddToCartModal", model);
        }


    }
}