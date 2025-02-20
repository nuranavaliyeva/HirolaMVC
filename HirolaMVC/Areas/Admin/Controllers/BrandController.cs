using HirolaMVC.Areas.ViewModels.Brands;
using HirolaMVC.Areas.ViewModels.Categories;
using HirolaMVC.DAL;
using HirolaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirolaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;

        public BrandController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var brandVMs = await _context.Brands.Where(c => !c.IsDeleted).Include(c => c.Products).Select(c => new GetBrandAdminVM
            {
                Id = c.Id,
                Name = c.Name,
                ProductCount = c.Products.Count()

            }).ToListAsync();



            return View(brandVMs);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Brands.AnyAsync(c => c.Name.Trim() == brand.Name.Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Brand already exist");
                return View();
            }
            brand.CreatedAt = DateTime.Now;
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            Brand brand = await _context.Brands.FirstOrDefaultAsync(c => c.Id == id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);

        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Brand brand)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            Brand existed = await _context.Brands.FirstOrDefaultAsync(c => c.Id == id);
            if (brand == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Brands.AnyAsync(c => c.Name.Trim() == brand.Name.Trim()) && brand.Id != id;
            if (result)
            {
                ModelState.AddModelError(nameof(Brand.Name), "Brand already exists");
                return View();
            }
            if (existed.Name == brand.Name)
            {
                return RedirectToAction(nameof(Index));
            }
            existed.Name = brand.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));



        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            Brand brand = await _context.Brands.FirstOrDefaultAsync(c => c.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            _context.Remove(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
