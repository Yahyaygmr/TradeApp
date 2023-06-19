using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TradeApp.Data;
using TradeApp.Models;

namespace TradeApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public OrderDetailsViewModel orderDetailsViewModel { get; set; }

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Details(int id)
        {
            orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == id),
                OrderDetails = _context.OrderDetails.Where(x => x.OrderId == id).Include(x => x.Product)
            };
            return View(orderDetailsViewModel);
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderList = _context.OrderHeaders.ToList();
            }
            else
            {
                orderHeaderList = _context.OrderHeaders.Where(x => x.ApplicationUserId == claim.Value).Include(x => x.ApplicationUser);

            }
            return View(orderHeaderList);
        }

        public IActionResult OnHold()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderList = _context.OrderHeaders.Where(x => x.OrderStatus == Other.Status_Onhold);
            }
            else
            {
                orderHeaderList = _context.OrderHeaders.Where(x => x.ApplicationUserId == claim.Value && x.OrderStatus == Other.Status_Onhold).Include(x => x.ApplicationUser);

            }
            return View(orderHeaderList);
        }

        public IActionResult Approved()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderList = _context.OrderHeaders.Where(x => x.OrderStatus == Other.Status_Approved);
            }
            else
            {
                orderHeaderList = _context.OrderHeaders.Where(x => x.ApplicationUserId == claim.Value && x.OrderStatus == Other.Status_Approved).Include(x => x.ApplicationUser);

            }
            return View(orderHeaderList);
        }

        public IActionResult Shipped()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderList = _context.OrderHeaders.Where(x => x.OrderStatus == Other.Status_Shipped);
            }
            else
            {
                orderHeaderList = _context.OrderHeaders.Where(x => x.ApplicationUserId == claim.Value && x.OrderStatus == Other.Status_Shipped).Include(x => x.ApplicationUser);

            }
            return View(orderHeaderList);
        }

        [HttpPost]
        [Authorize(Roles = Other.Role_Admin)]
        public IActionResult Approve()
        {
            OrderHeader orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == orderDetailsViewModel.OrderHeader.Id);
            orderHeader.OrderStatus = Other.Status_Approved;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize(Roles = Other.Role_Admin)]
        public IActionResult ShipIt()
        {
            OrderHeader orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == orderDetailsViewModel.OrderHeader.Id);
            orderHeader.OrderStatus = Other.Status_Shipped;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}