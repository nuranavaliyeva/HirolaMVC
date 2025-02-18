
using HirolaMVC.DAL;
using HirolaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HirolaMVC.Controllers
{
    public class HomeController : Controller
    {
        public readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;

        }

        public async Task< IActionResult >Index()
        {
            HomeVM homeVM = new HomeVM
            {
                Slides = await _context.Slides
                .OrderBy(s => s.Order)
                .Take(2)
                .ToListAsync(),

                NewProducts = await _context.Products
                .Take(8)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync()
            };

            return View(homeVM);
        }
        public IActionResult Error(string errorMessage)
        {
            return View(model: errorMessage);
        }


    }
}
