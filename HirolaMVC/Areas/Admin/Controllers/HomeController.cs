using HirolaMVC.Areas.ViewModels.Home;
using HirolaMVC.DAL;
using HirolaMVC.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirolaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
       

        public HomeController(AppDbContext context)
        {
            _context = context;
           
        }
        public async Task< IActionResult> Index()
        {
           
            HomeVM homeVM = new HomeVM()
            {
                Orders= await _context.Orders.Include(o=>o.AppUser).OrderByDescending(o=>o.CreatedAt).ToListAsync()
            };
            return View(homeVM);
        }
    }
}
