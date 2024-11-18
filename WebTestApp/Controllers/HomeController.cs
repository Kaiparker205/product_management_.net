using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebTestApp.Bl;
using WebTestApp.Data;
using WebTestApp.Models;

namespace WebTestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProductContext _context;

        public HomeController(ILogger<HomeController> logger, ProductContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var leastExpensiveProducts = await _context.Products
                .OrderBy(p => p.Price)
                .Take(3)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                LeastExpensiveProducts = leastExpensiveProducts
            };

            return View(viewModel);
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
    }
}
