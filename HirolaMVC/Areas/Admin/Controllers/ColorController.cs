using HirolaMVC.Areas.ViewModels.Colors;
using HirolaMVC.Areas.ViewModels.Tags;
using HirolaMVC.DAL;
using HirolaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirolaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [Authorize(Roles = "Admin,Moderator")]
    public class ColorController : Controller
    {
        
        private readonly AppDbContext _context;

        public ColorController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Color> colors = await _context.Colors.Include(t => t.ProductColors).ToListAsync();
            return View(colors);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateColorVM createVm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Colors.AnyAsync(c => c.Name.Trim() == createVm.Name.Trim());
            if (result)
            {
                ModelState.AddModelError(nameof(createVm.Name), "Color is already existed");
                return View();
            }
            Color color = new()
            {
                Name = createVm.Name,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
            };

            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();
            Color color = await _context.Colors.FirstOrDefaultAsync(t => t.Id == id);
            if (color == null) return NotFound();
            UpdateColorVM updateVM = new()
            {
                Name = color.Name
            };
            return View(updateVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateColorVM updateVm)
        {
            if (id == null || id < 1) return BadRequest();
            Color existed = await _context.Colors.FirstOrDefaultAsync(t => t.Id == id);
            if (existed == null) return NotFound();
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (await _context.Colors.AnyAsync(c => c.Name.Trim() == updateVm.Name.Trim() && c.Id != id))
            {
                ModelState.AddModelError(nameof(updateVm.Name), "Color already exists");
                return View();
            }

            existed.Name = updateVm.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();
            Color color = await _context.Colors.FirstOrDefaultAsync(t => t.Id == id);
            if (color == null) return NotFound();
            _context.Colors.Remove(color);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
