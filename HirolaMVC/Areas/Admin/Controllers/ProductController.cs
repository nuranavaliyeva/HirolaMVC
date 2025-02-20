using HirolaMVC.Areas.ViewModels.Products;
using HirolaMVC.Areas.ViewModels;
using HirolaMVC.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using HirolaMVC.Extensions;
using HirolaMVC.Models;
using HirolaMVC.Utilities.Enums;

namespace HirolaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin,Moderator")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1) return BadRequest();
            int count = await _context.Products.CountAsync();
            double totalPage = (int)Math.Ceiling(count / 5d);
            if (page > ViewBag.TotalPage) return BadRequest();

            PaginatedVM<GetAdminProductVM> productVMs = new()
            {
                TotalPage = totalPage,
                CurrentPage = page,
                ItemVMs = await _context.Products
            .Skip((page - 1) * 5)
            .Take(5)
            .Include(p => p.Category)
            .Include(p=>p.Brand)
            .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
            .Select(p => new GetAdminProductVM
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name,
               
                Image = p.ProductImages[0].Image,

            })
            .ToListAsync()
            };
            return View(productVMs);
        }
        public async Task<IActionResult> Create()
        {
            CreateAdminProductVM productVM = new CreateAdminProductVM
            {
                Tags = await _context.Tags.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                Colors = await _context.Colors.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
            };

            return View(productVM);
        }
        [HttpPost]

        public async Task<IActionResult> Create(CreateAdminProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.Colors = await _context.Colors.ToListAsync();
            productVM.Brands= await _context.Brands.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("MainPhoto", "File type is incorrect");
                return View(productVM);
            }
            if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError("MainPhoto", "File type is incorrect");
                return View(productVM);
            }

            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("HoverPhoto", "File type is incorrect");
                return View(productVM);
            }
            if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError("HoverPhoto", "File type is incorrect");
                return View(productVM);
            }


            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateAdminProductVM.CategoryId), "Category does not exist");
                return View(productVM);
            }
            bool brandResult = productVM.Brands.Any(c => c.Id == productVM.BrandIds);
            if (!brandResult)
            {
                ModelState.AddModelError(nameof(CreateAdminProductVM.BrandIds), "Brand does not exist");
                return View(productVM);
            }




            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));

                if (tagResult)
                {
                    ModelState.AddModelError(nameof(CreateAdminProductVM.TagIds), "Tags are wrong");

                    return View(productVM);
                }
            }
            if (productVM.ColorIds is not null)
            {
                bool colorResult = productVM.ColorIds.Any(cId => !productVM.Colors.Exists(c => c.Id == cId));

                if (colorResult)
                {
                    ModelState.AddModelError(nameof(CreateAdminProductVM.ColorIds), "colors are wrong");

                    return View(productVM);
                }
            }
          

            ProductImage main = new()
            {
                Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                IsPrimary = true,
                CreatedAt = DateTime.Now,
                IsDeleted = false

            };
            ProductImage hover = new()
            {
                Image = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                IsPrimary = false,
                CreatedAt = DateTime.Now,
                IsDeleted = false,

            };

            Product product = new()
            {
                Name = productVM.Name,
                ISAvailable = productVM.IsAvailable,
                ProductCode = productVM.ProductCode,
                CategoryId = productVM.CategoryId.Value,
                BrandId = productVM.BrandIds.Value,
                Description = productVM.Description,
                Price = productVM.Price.Value,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                ProductImages = new List<ProductImage> { main, hover }
            };

            if (productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tId => new ProductTag { TagId = tId }).ToList();
            }
            if (productVM.ColorIds is not null)
            {
                product.ProductColors = productVM.ColorIds.Select(cId => new ProductColor { ColorId = cId }).ToList();
            }

            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} type was not correct</p>";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} size was not correct</p>";
                        continue;
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images" ),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null,

                    });
                }

                TempData["FileWarning"] = text;
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        //[Authorize(Roles = "Admin")]

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Product product = await _context.Products.Include(p => p.ProductImages).Include(p => p.ProductTags).Include(p=>p.ProductColors).FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            UpdateProductVM productVM = new()
            {
                Name = product.Name,
                ProductCode = product.ProductCode,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Price = product.Price,
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                ProductImages = product.ProductImages,
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                ColorIds=product.ProductColors.Select(pc => pc.ColorId).ToList(),
                Colors = await _context.Colors.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                BrandId = product.BrandId

            };

            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            if (id == null || id < 1) return BadRequest();
            Product existed = await _context.Products.Include(p => p.ProductImages).Include(p => p.ProductTags).Include(p=>p.ProductColors).FirstOrDefaultAsync(p => p.Id == id);

            if (existed is null) return NotFound();

            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.ProductImages = existed.ProductImages;
            productVM.Brands = await _context.Brands.ToListAsync();
            productVM.Colors = await _context.Colors.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }



            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("MainPhoto", "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError("MainPhoto", "File type is incorrect");
                    return View(productVM);
                }

            }

            if (productVM.HoverPhoto is not null)
            {
                if (!productVM.HoverPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("HoverPhoto", "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError("HoverPhoto", "File type is incorrect");
                    return View(productVM);
                }

            }

            if (existed.CategoryId != productVM.CategoryId)
            {
                bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
                if (!result)
                {

                    return View(productVM);
                }
            }
            if (existed.BrandId != productVM.BrandId)
            {
                bool result = productVM.Brands.Any(c => c.Id == productVM.BrandId);
                if (!result)
                {

                    return View(productVM);
                }
            }

            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));

                if (tagResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.TagIds), "Tags are wrong");

                    return View(productVM);
                }
            }



            if (productVM.TagIds is null)
            {
                productVM.TagIds = new();
            }
            else
            {
                productVM.TagIds = productVM.TagIds.Distinct().ToList();
            }
            _context.ProductTags.RemoveRange(existed.ProductTags
            .Where(pTag => !productVM.TagIds.Exists(tId => tId == pTag.TagId))
            .ToList());

            _context.ProductTags.AddRange(productVM.TagIds
           .Where(tId => !existed.ProductTags.Exists(pTag => pTag.TagId == tId))
           .ToList()
           .Select(tId => new ProductTag { TagId = tId, ProductId = existed.Id }));






            if (productVM.ColorIds is not null)
            {
                bool colorResult = productVM.ColorIds.Any(cId => !productVM.Colors.Exists(c => c.Id == cId));

                if (colorResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.ColorIds), "colors are wrong");

                    return View(productVM);
                }
            }



            if (productVM.ColorIds is null)
            {
                productVM.ColorIds = new();
            }
            else
            {
                productVM.ColorIds = productVM.ColorIds.Distinct().ToList();
            }
            _context.ProductColors.RemoveRange(existed.ProductColors
            .Where(pColor => !productVM.ColorIds.Exists(cId => cId == pColor.ColorId))
            .ToList());

            _context.ProductColors.AddRange(productVM.ColorIds
           .Where(cId => !existed.ProductColors.Exists(cColor => cColor.ColorId == cId))
           .ToList()
           .Select(cId => new ProductColor { ColorId = cId, ProductId = existed.Id }));


            if (productVM.MainPhoto is not null)
            {
                string fileName = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images");

                ProductImage main = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == true);
                main.Image.DeleteFile(_env.WebRootPath, "assets", "images");
                existed.ProductImages.Remove(main);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = true,
                    Image = fileName
                });
            }

            if (productVM.HoverPhoto is not null)
            {
                string fileName = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images");
                ProductImage hover = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == false);
                hover.Image.DeleteFile(_env.WebRootPath, "assets", "images");
                existed.ProductImages.Remove(hover);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = false,
                    Image = fileName
                });
            }


            if (productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }
            var deletedImages = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == null).ToList();

            deletedImages.ForEach(di => di.Image.DeleteFile(_env.WebRootPath, "assets", "images"));


            _context.ProductImages.RemoveRange(deletedImages);

            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} type was not correct</p>";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} size was not correct</p>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null,

                    });
                }

                TempData["FileWarning"] = text;
            }

            
            existed.ProductCode = productVM.ProductCode;
            existed.Price = productVM.Price.Value;
            existed.CategoryId = productVM.CategoryId.Value;
            existed.BrandId = productVM.BrandId.Value;
            existed.Description = productVM.Description;
            existed.Name = productVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


        }

    }
}
