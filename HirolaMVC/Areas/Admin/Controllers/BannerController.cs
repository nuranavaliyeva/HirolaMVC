using HirolaMVC.Areas.ViewModels;
using HirolaMVC.Areas.ViewModels.Banners;
using HirolaMVC.DAL;
using HirolaMVC.Extensions;
using HirolaMVC.Models;
using HirolaMVC.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirolaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [Authorize(Roles = "Admin,Moderator")]
    public class BannerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public BannerController(AppDbContext context, IWebHostEnvironment env)
        {

            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {

            List<Banner> banners = await _context.Banners.ToListAsync();
            return View(banners);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateBannerVM bannerVM)
        {

            if (!bannerVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File type is incorrect");
                return View();
            }
            if (!bannerVM.Photo.ValidateSize(Utilities.Enums.FileSize.MB, 10))
            {
                ModelState.AddModelError("Photo", "File Size must be less than 10 MB");
                return View();
            }
            Banner banner = new Banner
            {
                Title = bannerVM.Title,
                SubTitle = bannerVM.SubTitle,
                Order = bannerVM.Order,
                Description = bannerVM.Description,
                Image = await bannerVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            if (!ModelState.IsValid) return View();
            await _context.Banners.AddAsync(banner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Banner banner = await _context.Banners.FirstOrDefaultAsync(s => s.Id == id);


            if (banner is null) return NotFound();
            UpdateBannerVM bannerVM = new()
            {
                Title = banner.Title,
                SubTitle = banner.SubTitle,
                Order = banner.Order,
                Description = banner.Description,
                Image = banner.Image,

            };

            return View(bannerVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateBannerVM BannerVM)
        {

            if (!ModelState.IsValid)
            {
                return View(BannerVM);
            }
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            if (BannerVM.Photo != null)
            {
                if (BannerVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "type is incorrect");
                    return View(BannerVM);

                }
                if (!BannerVM.Photo.ValidateSize(FileSize.MB, 10))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "Size is Incorrect");
                    return View(BannerVM);
                }
                string fileName = await BannerVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images");
                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images");
                existed.Image = fileName;
            }
            existed.Title = BannerVM.Title;
            existed.Description = BannerVM.Description;
            existed.SubTitle = BannerVM.SubTitle;
            existed.Order = BannerVM.Order;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Banner banner = await _context.Banners.FirstOrDefaultAsync(s => s.Id == id);


            if (banner is null) return NotFound();

            banner.Image.DeleteFile(_env.WebRootPath, "assets", "images");
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
