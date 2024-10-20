using ETICARET.Business.Abstract;
using ETICARET.Entities;
using ETICARET.WebUI.Extensions;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
                    var payment = PaymentProcess(model);

                    if(payment.Result.Status == "success")
                    {
                        SaveOrder(model, payment, userId);
                        ClearCart(cart.Id.ToString());
                        TempData.Put("message", new ResultModel()
                        {
                            Title = "Order Completed",
                            Message = "Conguratulations. Your order has been received.",
                            Css = "success"
                        });

                        return View("Success");

                    }
                    else
                    {
                        TempData.Put("message", new ResultModel()
                        {
                            Title = "Order Error",
                            Message = payment.Result.ErrorMessage,
                            Css = "danger"
                        });
                    }

                    
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
            else
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Order Status",
                    Message = "Please fill a form.",
                    Css = "warning"
                });
                return View(model);
            }


            return View(model);
        }

        private Task<Payment> PaymentProcess(OrderModel model)
        {
            Options options = new Options();
            options.ApiKey = "sandbox-cNnJEaoyNt0sCREL4nOq8PajTLQwWeXz";
            options.SecretKey = "sandbox-cmJxJfaGlVarqNV3c5ZQcMTwVNh8qswx";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";


            string extarnalIP = new WebClient().DownloadString("https://www.icanhazip.com").Replace("\\r\n", "").Replace("\\n", "").Trim();

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = Guid.NewGuid().ToString();
            request.Price = model.CartModel.TotalPrice().ToString().Split(',')[0];
            request.PaidPrice = model.CartModel.TotalPrice().ToString().Split(',')[0];
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = model.CartModel.CartId.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.PaymentChannel = PaymentChannel.WEB.ToString();

            PaymentCard paymentCard = new PaymentCard()
            {
                CardHolderName = model.CardName,
                CardNumber = model.CardNumber,
                ExpireMonth = model.ExprationMonth,
                ExpireYear = model.ExprationYear,
                Cvc = model.CVV,
                RegisterCard = 0
            };

            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer()
            {
                Id = _userManager.GetUserId(User),
                Name = model.FirstName,
                Surname = model.LastName,
                GsmNumber = model.Phone,
                Email = model.Email,
                IdentityNumber = "11111111111",
                RegistrationAddress = model.Address,
                Ip = extarnalIP,
                City = model.City,
                Country = "TURKEY",
                ZipCode = "34000"
            };

            request.Buyer = buyer;


            Address shippingAddress = new Address()
            {
                ContactName = model.FirstName +" "+ model.LastName,
                City = model.City.ToString(),
                Country = "TURKEY",
                ZipCode = "34000",
                Description = model.Address
            };

            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address()
            {
                ContactName = model.FirstName + " " + model.LastName,
                City = model.City.ToString(),
                Country = "TURKEY",
                ZipCode = "34000",
                Description = model.Address
            };

            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem basketItem;

            foreach (var cartItem in model.CartModel.CartItems)
            {
                basketItem = new BasketItem()
                {
                    Id = cartItem.ProductId.ToString(),
                    Name = cartItem.Name,
                    Category1 = _productService.GetProductDetail(cartItem.ProductId).ProductCategories.FirstOrDefault().CategoryId.ToString(),
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = (cartItem.Price * cartItem.Quantity).ToString().Split(",")[0]
                };

                basketItems.Add(basketItem);
            }

            request.BasketItems = basketItems;

            var payment = Payment.Create(request,options);

            return payment;

        }

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
                var orderItem = new Entities.OrderItem()
                {
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId
                };
                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);
        }

        // Kredi kartı için çalışan Sipariş Oluşturma Methodu
        private void SaveOrder(OrderModel model, Task<Payment> payment, string userId)
        {
            var order = new Order()
            {
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = EnumOrderState.completed,
                PaymentTypes = EnumPaymentTypes.CreditCard,
                PaymentToken = Guid.NewGuid().ToString(),
                ConversionId = payment.Result.ConversationId,
                PaymentId = payment.Result.PaymentId,
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
                var orderItem = new Entities.OrderItem()
                {
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId
                };
                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);
        }

        public IActionResult GetOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = _orderService.GetOrders(userId);

            var orderListModel = new List<OrderListModel>();

            OrderListModel orderModel;

            foreach (var order in orders)
            {
                orderModel = new OrderListModel();
                orderModel.OrderId = order.Id;
                orderModel.OrderNumber = order.OrderNumber;
                orderModel.OrderDate = order.OrderDate;
                orderModel.OrderNote = order.OrderNote;
                orderModel.OrderState = order.OrderState;
                orderModel.PaymentTypes = order.PaymentTypes;
                orderModel.Phone = order.Phone;
                orderModel.Address = order.Address;
                orderModel.Email = order.Email;
                orderModel.FirstName = order.FirstName;
                orderModel.LastName = order.LastName;
                orderModel.City = order.City;

                orderModel.OrderItems = order.OrderItems.Select(i => new OrderItemModel()
                {
                    OrderItemId = i.Id,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product.Images[0].ImageUrl
                }).ToList();

                orderListModel.Add(orderModel);

            }

            return View(orderListModel);
        }
    }
}
