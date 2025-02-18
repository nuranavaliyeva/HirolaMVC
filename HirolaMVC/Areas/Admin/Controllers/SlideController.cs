using HirolaMVC.Areas.ViewModels;
using HirolaMVC.DAL;
using HirolaMVC.Extensions;
using HirolaMVC.Models;
using HirolaMVC.Utilities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HirolaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {

            _context = context;
            _env = env;
        }
        public async  Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
            if (!slideVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File type is incorrect");
                return View(); 
            }
            if(!slideVM.Photo.ValidateSize(Utilities.Enums.FileSize.MB, 10))
            {
                ModelState.AddModelError("Photo", "File Size must be less than 10 MB");
                return View();
            }
            Slide slide = new Slide
            {
                Title = slideVM.Title,
                SubTitle=slideVM.SubTitle,
                Order = slideVM.Order,
                Description = slideVM.Description,
                Image= await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                IsDeleted=false,
                CreatedAt = DateTime.Now
            };

     

            if (!ModelState.IsValid) return View();
            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);


            if (slide is null) return NotFound();
            UpdateSlideVM slideVM = new()
            {
                Title = slide.Title,
                SubTitle = slide.SubTitle,
                Order = slide.Order,
                Description = slide.Description,
                Image = slide.Image,

            };

            return View(slideVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateSlideVM SlideVM)
        {

            if (!ModelState.IsValid)
            {
                return View(SlideVM);
            }
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            if (SlideVM.Photo != null)
            {
                if (SlideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "type is incorrect");
                    return View(SlideVM);

                }
                if (!SlideVM.Photo.ValidateSize(FileSize.MB, 10))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "Size is Incorrect");
                    return View(SlideVM);
                }
                string fileName = await SlideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images");
                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image = fileName;
            }
            existed.Title = SlideVM.Title;
            existed.Description = SlideVM.Description;
            existed.SubTitle = SlideVM.SubTitle;
            existed.Order = SlideVM.Order;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);


            if (slide is null) return NotFound();

            slide.Image.DeleteFile(_env.WebRootPath, "assets", "images");
            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
