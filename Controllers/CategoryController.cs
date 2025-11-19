using login.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using login.Models;

namespace login.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        public IActionResult Show(int id)
        {
            var category = _context.Categories.Include(c => c.Products).FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }
    }
}