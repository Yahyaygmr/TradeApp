using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TradeApp.Data;

namespace TradeApp.ViewComponents
{
    public class CategoryList : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryList(ApplicationDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
    }
}
