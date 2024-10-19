using ETICARET.Business.Abstract;
using ETICARET.Entities;
using ETICARET.WebUI.Extensions;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using Iyzipay;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETICARET.WebUI.Controllers
{
    public class CartController : Controller
    {
        private ICartService _cartService;
        private IOrderService _orderService;
        private IProductService _productService;
        private UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService,IProductService productService,IOrderService orderService,UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _productService = productService;
            _userManager = userManager;

        }
        public IActionResult Index()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));


            return View(
                    new CartModel()
                    {
                        CartId = cart.Id,
                        CartItems = cart.CartItems.Select(i => new CartItemModel()
                        {
                            CartItemId = i.Id,
                            ProductId = i.ProductId,    
                            Name = i.Product.Name,
                            Price = i.Product.Price,
                            Quantity = i.Quantity,
                            ImageUrl = i.Product.Images[0].ImageUrl,
                        }).ToList()
                    }
                
           );
        }

        public IActionResult AddToCart(int productId,int quantity)
        {
            _cartService.AddToCart(_userManager.GetUserId(User), productId, quantity);

            return RedirectToAction("Index");
        }

        public IActionResult DeleteFromCart(int productId)
        {
            _cartService.DeleteFromCart(_userManager.GetUserId(User),productId);
            
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            OrderModel orderModel = new OrderModel();

            orderModel.CartModel = new CartModel()
            {
                CartId = cart.Id,
                CartItems = cart.CartItems.Select( i => new CartItemModel()
                {
                    CartItemId = i.Id,
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product.Images[0].ImageUrl,
                }).ToList()
            };

            return View(orderModel);
        }

        [HttpPost]
        public IActionResult Checkout(OrderModel model, string paymentMethod)
        {
            ModelState.Remove("CartModel");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var cart = _cartService.GetCartByUserId(userId);

                model.CartModel = new CartModel()
                {
                    CartId = cart.Id,
                    CartItems = cart.CartItems.Select(i => new CartItemModel()
                    {
                        CartItemId = i.Id,
                        ProductId = i.ProductId,
                        Name = i.Product.Name,
                        Price = i.Product.Price,
                        Quantity = i.Quantity,
                        ImageUrl = i.Product.Images[0].ImageUrl
                    }).ToList()
                };

                if(paymentMethod == "credit")
                {
                    // ödeme sistemi
                    //var payment = PaymentProcess(model);
                }
                else
                {
                    SaveOrder(model,userId);
                    ClearCart(cart.Id.ToString());
                    TempData.Put("message", new ResultModel()
                    {
                        Title = "Order Completed",
                        Message = "Conguratulations. Your order has been received.",
                        Css = "success"
                    });

                    return View("Success");
                }


            }


            return View(model);
        }

        //private object PaymentProcess(OrderModel model)
        //{
        //    Options options = new Options();
        //    options.ApiKey = "sandbox-cNnJEaoyNt0sCREL4nOq8PajTLQwWeXz";
        //    options.SecretKey = "sandbox-cmJxJfaGlVarqNV3c5ZQcMTwVNh8qswx";
        //    options.BaseUrl = "https://sandbox-api.iyzipay.com";
        //}

        private void ClearCart(string cartId)
        {
            _cartService.ClearCart(cartId);
        }


        // Eft için çalışan Sipariş Oluşturma Methodu
        private void SaveOrder(OrderModel model, string userId)
        {
            var order = new Order()
            {
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = EnumOrderState.completed,
                PaymentTypes = EnumPaymentTypes.Eft,
                PaymentToken = Guid.NewGuid().ToString(),
                ConversionId = Guid.NewGuid().ToString(),
                PaymentId = Guid.NewGuid().ToString(),
                OrderNote = model.OrderNote,
                OrderDate = DateTime.Now,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                Email = model.Email,
                City = model.City,
                Phone = model.Phone,
                UserId = userId,
            };

            foreach (var item in model.CartModel.CartItems)
            {
                var orderItem = new OrderItem()
                {
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId
                };
                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);
        }

    }
}
