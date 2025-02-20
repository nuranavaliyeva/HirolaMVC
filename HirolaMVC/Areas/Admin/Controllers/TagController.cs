using HirolaMVC.Areas.ViewModels.Brands;
using HirolaMVC.Areas.ViewModels.Tags;
using HirolaMVC.DAL;
using HirolaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirolaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Tag> tags = await _context.Tags.Include(t => t.ProductTags).ToListAsync();
            return View(tags);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM createVm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Tags.AnyAsync(c => c.Name.Trim() == createVm.Name.Trim());
            if (result)
            {
                ModelState.AddModelError(nameof(createVm.Name), "Category is already existed");
                return View();
            }
            Tag tag = new()
            {
                Name = createVm.Name,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();
            Tag tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null) return NotFound();
            UpdateTagVM updateVM = new()
            {
                Name = tag.Name
            };
            return View(updateVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateTagVM updateVm)
        {
            if (id == null || id < 1) return BadRequest();
            Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (existed == null) return NotFound();
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (await _context.Tags.AnyAsync(c => c.Name.Trim() == updateVm.Name.Trim() && c.Id != id))
            {
                ModelState.AddModelError(nameof(updateVm.Name), "Category already exists");
                return View();
            }

            existed.Name = updateVm.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();
            Tag tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null) return NotFound();
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
