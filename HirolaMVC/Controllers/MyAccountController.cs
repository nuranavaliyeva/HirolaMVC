using HirolaMVC.DAL;
using HirolaMVC.Models;
using HirolaMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HirolaMVC.Controllers
{
    [Authorize]
    public class MyAccountController : Controller
    {
        private readonly AppDbContext _context;

        public MyAccountController(AppDbContext context)
        {
            _context = context;
        }
        public async Task< IActionResult> Index()
        {
            MyAccountVM accountVM = new MyAccountVM()
            {
                Orders = await _context.Orders.Include(o => o.AppUser).Where(o => o.AppUserId == User.FindFirst(ClaimTypes.NameIdentifier).Value).ToListAsync(),
                AppUsers = await _context.AppUsers.Where(o=>o.Id==User.FindFirst(ClaimTypes.NameIdentifier).Value).ToListAsync()
            };
            return View(accountVM);
        }
    }
}
