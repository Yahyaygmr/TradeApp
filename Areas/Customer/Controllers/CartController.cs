using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TradeApp.Data;
using TradeApp.Models;

namespace TradeApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _context.ShoppingCards.Where(x => x.ApplicationUserId == claim.Value).Include(x => x.Product)
            };
            ShoppingCartViewModel.OrderHeader.OrderTotalCost = 0;
            ShoppingCartViewModel.OrderHeader.ApplicationUser = _context.ApplicationUsers.FirstOrDefault(x => x.Id == claim.Value);
            foreach (var item in ShoppingCartViewModel.ListCart)
            {
                ShoppingCartViewModel.OrderHeader.OrderTotalCost += (item.Count * item.Product.Price);
            }
            return View(ShoppingCartViewModel);
        }

        public IActionResult Add(int cartId)
        {
            var cart = _context.ShoppingCards.FirstOrDefault(x => x.Id == cartId);
            cart.Count += 1;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Decrease(int cartId)
        {
            var cart = _context.ShoppingCards.FirstOrDefault(x => x.Id == cartId);
            if (cart.Count == 1)
            {
                var count = _context.ShoppingCards.Where(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                _context.ShoppingCards.Remove(cart);
                _context.SaveChanges();
                HttpContext.Session.SetInt32(Other.sessionShoppingCart, count - 1);
            }
            else
            {
                cart.Count -= 1;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _context.ShoppingCards.FirstOrDefault(x => x.Id == cartId);
            var count = _context.ShoppingCards.Where(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            _context.ShoppingCards.Remove(cart);
            _context.SaveChanges();
            HttpContext.Session.SetInt32(Other.sessionShoppingCart, count - 1);


            return RedirectToAction("Index");
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _context.ShoppingCards.Where(x => x.ApplicationUserId == claim.Value).Include(x => x.Product)
            };
            foreach (var item in ShoppingCartViewModel.ListCart)
            {
                item.Price = item.Product.Price;
                ShoppingCartViewModel.OrderHeader.OrderTotalCost += (item.Count * item.Product.Price);
            }
            return View(ShoppingCartViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(ShoppingCartViewModel model)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartViewModel.ListCart = _context.ShoppingCards.Where(x => x.ApplicationUserId == claim.Value).Include(x => x.Product);
            ShoppingCartViewModel.OrderHeader.OrderStatus = Other.Status_Onhold;
            ShoppingCartViewModel.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            _context.OrderHeaders.Add(ShoppingCartViewModel.OrderHeader);
            _context.SaveChanges();
            foreach (var item in ShoppingCartViewModel.ListCart)
            {
                item.Price = item.Product.Price;
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = item.Product.Id,
                    OrderId = ShoppingCartViewModel.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
                ShoppingCartViewModel.OrderHeader.OrderTotalCost += item.Count * item.Product.Price;
                model.OrderHeader.OrderTotalCost += item.Count * item.Product.Price;
                _context.OrderDetails.Add(orderDetails);
            }
            var payment = PaymentProcess(model);
            _context.ShoppingCards.RemoveRange(ShoppingCartViewModel.ListCart);
            _context.SaveChanges();
            HttpContext.Session.SetInt32(Other.sessionShoppingCart, 0);
            return RedirectToAction("OrderOk");
        }

        private Payment PaymentProcess(ShoppingCartViewModel model)
        {
            Options options = new Options();
            options.ApiKey = "sandbox-5uZQWHjhCwJmVIezbS72ngEbOFOzbokl";
            options.SecretKey = "sandbox-dpBQD4aXekEM6OKUL1NqpS0aCRBaBqGt";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = new Random().Next(1111, 9999).ToString();
            request.Price = model.OrderHeader.OrderTotalCost.ToString();
            request.PaidPrice = model.OrderHeader.OrderTotalCost.ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "B67832";
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = model.OrderHeader.CardName;
            paymentCard.CardNumber = model.OrderHeader.CardNumber;
            paymentCard.ExpireMonth = model.OrderHeader.ExprationMonth;
            paymentCard.ExpireYear = model.OrderHeader.ExprationYear;
            paymentCard.Cvc = model.OrderHeader.Cvc;
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            //örnek kart bilgileri
            //PaymentCard paymentCard = new PaymentCard();
            //paymentCard.CardHolderName = "John Doe";
            //paymentCard.CardNumber = "5528790000000008";
            //paymentCard.ExpireMonth = "12";
            //paymentCard.ExpireYear = "2030";
            //paymentCard.Cvc = "123";
            //paymentCard.RegisterCard = 0;
            //request.PaymentCard = paymentCard;


            Buyer buyer = new Buyer();
            buyer.Id = model.OrderHeader.Id.ToString();
            buyer.Name = model.OrderHeader.Name;
            buyer.Surname = model.OrderHeader.Surname;
            buyer.GsmNumber = model.OrderHeader.PhoneNumber;
            buyer.Email = "email@email.com";
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = model.OrderHeader.Adress;
            buyer.Ip = "85.34.78.112";
            buyer.City = model.OrderHeader.City;
            buyer.Country = "Turkey";
            buyer.ZipCode = model.OrderHeader.PostCode;
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            foreach (var item in _context.ShoppingCards.Where(x => x.ApplicationUserId == claim.Value).Include(x => x.Product))
            {
                basketItems.Add(new BasketItem()
                {

                    Id = item.Id.ToString(),
                    Name = item.Product.Title,
                    Category1 = item.Product.CategoryId.ToString(),
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = (item.Price * item.Count).ToString()
                });
            }

            request.BasketItems = basketItems;

            return Payment.Create(request, options);
        }

        public IActionResult OrderOk()
        {
            return View();
        }
    }
}
