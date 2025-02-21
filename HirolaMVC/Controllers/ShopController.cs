//using HirolaMVC.DAL;
//using HirolaMVC.Models;
//using HirolaMVC.Utilities.Enums;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

//namespace HirolaMVC.Controllers
//{
//    public class ShopController : Controller
//    {
//        private readonly AppDbContext _context;

//        public ShopController(AppDbContext context)
//        {
//            _context = context;
//        }
//        public IActionResult Index(string? search, int? categoryId, int? colorId, int key = 1, int page = 1)
//        {
//            IQueryable<Product> query = _context.Products.Include(p => p.ProductColors).Include(p => p.ProductTags).Include(p => p.ProductImages);
//            if (!string.IsNullOrEmpty(search))
//            {
//                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower())).Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null));
//            }
//            if (categoryId != null && categoryId > 0)
//            {
//                query = query.Where(p => p.CategoryId == categoryId);
//            }
//            if (colorId != null && colorId > 0)
//            {
//                query = query.Where(p => p.ProductColors.Exists(pc => pc.Id == colorId));
//            }
//            switch (key)
//            {
//                case (int)SortType.Name:
//                    query = query.OrderBy(p => p.Name);
//                    break;
//                case (int)SortType.Price:
//                    query = query.OrderByDescending(p => p.Price);
//                    break;
//                case (int)SortType.Date:
//                    query = query.OrderByDescending(p => p.CreatedAt);
//                    break;
//                default:
//                    break;
//            }
//            int count = query.Count();
//            double total = Math.Ceiling((double)count / 6);
//            query = query.Skip((page - 1) * 6).Take(6);
//            ShopVM vm = new ShopVM()
//            {
//                Products = await query.Select(p => new GetProductVM
//                {
//                    Id = p.Id,
//                    Name = p.Name,
//                    Image = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).ImageURL,
//                    SecondaryImg = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false).ImageURL,
//                    Price = p.Price

//                }).ToListAsync(),
//                Categories = await _context.Categories.Select(c => new GetCategoryVM
//                {
//                    Id = c.Id,
//                    Name = c.Name,
//                    ProductCount = c.Products.Count(),
//                }).ToListAsync(),
//                Search = search,
//                CategoryId = categoryId,
//                Key = key,
//                TotalPage = total,
//                CurrentPage = page,
//                Colors = await _context.Colors.Select(c => new GetColorVM
//                {
//                    Id = c.Id,
//                    Name = c.Name,
//                }).ToListAsync(),
//                ColorId = colorId,
//            };
//            return View(vm);
//        }
//    }
//}
