using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TradeApp.Data;
using TradeApp.Models;

namespace TradeApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _applicationDb;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDb)
        {
            _logger = logger;
            _applicationDb = applicationDb;
        }

        public IActionResult Index()
        {
            var products = _applicationDb.Products.Where(x => x.IsHome).ToList();
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _applicationDb.ShoppingCards.Where(x => x.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(Other.sessionShoppingCart, count);
            }
            return View(products);
        }
        public IActionResult Details(int id)
        {
            var product = _applicationDb.Products.FirstOrDefault(x => x.Id == id);
            ShoppingCard card = new ShoppingCard()
            {
                Product = product,
                ProductId = id
            };
            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCard shoppingCard)
        {
            shoppingCard.Id = 0;
            if (ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCard.ApplicationUserId = claim.Value;
                ShoppingCard cart = _applicationDb.ShoppingCards.FirstOrDefault(
                    x => x.ApplicationUserId == shoppingCard.ApplicationUserId && x.ProductId == shoppingCard.ProductId);
                if (cart == null)
                {
                    _applicationDb.ShoppingCards.Add(shoppingCard);
                }
                else
                {
                    cart.Count += shoppingCard.Count;
                }
                _applicationDb.SaveChanges();
                var count = _applicationDb.ShoppingCards.Where(x => x.ApplicationUserId == shoppingCard.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(Other.sessionShoppingCart, count);
                return RedirectToAction("Index");
            }
            else
            {
                var product = _applicationDb.Products.FirstOrDefault(x => x.Id == shoppingCard.Id);
                ShoppingCard card = new ShoppingCard()
                {
                    Product = product,
                    ProductId = product.Id
                };
            }

            return View(shoppingCard);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CategoryDetails(int? id)
        {
            var product = _applicationDb.Products.Where(x =>x.CategoryId == id).ToList();
            ViewBag.categoryId = id;
            return View(product);
        }

        public IActionResult Search(string q)
        {
            if (!string.IsNullOrEmpty(q))
            {
                var ara = _applicationDb.Products.Where(x => x.Title.Contains(q) || x.Description.Contains(q));
                return View(ara);
            }
            return View();
        }
    }
}
