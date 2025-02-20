using HirolaMVC.DAL;
using HirolaMVC.Services.Interfaces;
using HirolaMVC.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HirolaMVC.Services.Implementations
{
    public class BasketService : IBasketService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly ClaimsPrincipal _user;

        public BasketService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
            _user = http.HttpContext.User;
        }
        public async Task<List<BasketItemVM>> GetBasketAsync()
        {
            List<BasketItemVM> basketVM = new();
            if (_user.Identity.IsAuthenticated)
            {

                basketVM = await _context.BasketItems.Where(bi => bi.AppUserId == _http.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
                    .Select(bi => new BasketItemVM
                    {
                        Count = bi.Count,
                        Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                        Name = bi.Product.Name,
                        Price = bi.Product.Price,
                        SubTotal = bi.Count * bi.Product.Price,
                        ProductId = bi.ProductId
                    }).ToListAsync();

            }
            else
            {
                List<BasketCookieItemVM> cookies;
                string cookie = _http.HttpContext.Request.Cookies["basket"];

                if (cookie is null)
                {
                    return basketVM;
                }

                cookies = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);

                basketVM = await _context.Products.Where(p => cookies.Select(c => c.ProductId).Contains(p.Id)).Select(p => new BasketItemVM
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image = p.ProductImages[0].Image,

                }).ToListAsync();

                basketVM.ForEach(async bi =>
                {
                    bi.Count = cookies.FirstOrDefault(c => c.ProductId == bi.ProductId).Count;
                    bi.SubTotal = bi.Price * bi.Count;
                });
            }

            return basketVM;



        
    }
    }
}
