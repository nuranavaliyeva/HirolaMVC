using HirolaMVC.DAL;
using HirolaMVC.Models;
using HirolaMVC.Services.Interfaces;
using HirolaMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using System.Security.Claims;

namespace HirolaMVC.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBasketService _basketService;
        private readonly IEmailService _emailService;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager, IBasketService basketService, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _basketService = basketService;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _basketService.GetBasketAsync());
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null) return BadRequest();
            bool result = await _context.Products.AnyAsync(p => p.Id == id);
            if (!result) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users.
                    Include(u => u.BasketItems).
                    FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                if (item is null)
                {
                    user.BasketItems.Add(new BasketItem
                    {
                        ProductId = id.Value,
                        Count = 1,
                    });
                }
                else
                {
                    item.Count++;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;

                string cookies = Request.Cookies["basket"];
                if (cookies != null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);
                    BasketCookieItemVM itemVM = basket.FirstOrDefault(b => b.ProductId == id);
                    if (itemVM != null)
                    {
                        itemVM.Count++;
                    }
                    else
                    {
                        basket.Add(new()
                        {
                            ProductId = id.Value,
                            Count = 1
                        });
                    }
                }
                else
                {
                    basket = new();
                    basket.Add(new()
                    {
                        ProductId = id.Value,
                        Count = 1
                    });
                }
                string json = JsonConvert.SerializeObject(basket);
                Response.Cookies.Append("basket", json);
            }
            //return RedirectToAction(nameof(GetBasket));
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> GetBasket()
        {
            return PartialView("BasketPartialView", await _basketService.GetBasketAsync());
        }
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Checkout()
        {

            OrderVM orderVM = new()
            {
                BasketInOrderVMs = await _context.BasketItems
                .Where(bi => bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(bi => new BasketInOrderVM
                {
                    Count = bi.Count,
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    SubTotal = bi.Product.Price * bi.Count
                }).ToListAsync()
            };
            return View(orderVM);
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(OrderVM orderVM, string stripeEmail, string stripeToken)
        {
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            List<BasketItem> basketItems = await _context.BasketItems
                .Where(bi => bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Include(bi => bi.Product)
               .ToListAsync();
            if (!ModelState.IsValid)
            {
                orderVM.BasketInOrderVMs = basketItems
                .Select(bi => new BasketInOrderVM
                {
                    Count = bi.Count,
                    Name = bi.Product.Name,
                    Price = bi.Product.Price,
                    SubTotal = bi.Product.Price * bi.Count
                }).ToList();
                return View(orderVM);
            }
           
            Order order = new Order
            {
                Address = orderVM.Address,
                Phone = orderVM.Phone,
                Email = orderVM.Email,
                Status = null,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                OrderItems = basketItems.Select(bi => new OrderItem
                {
                    AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Count = bi.Count,
                    Price = bi.Product.Price,
                    SubTotal = bi.Product.Price * bi.Count,
                    ProductId = bi.ProductId,
                }).ToList(),
                Total = basketItems.Sum(bi => bi.Product.Price * bi.Count)
            };
            decimal total = order.Total *100;

            var optionCust = new CustomerCreateOptions
            {
                Email = stripeEmail,
                Name = user.Name + " " + user.Surname,
                Phone = user.PhoneNumber
            };
            var serviceCust = new CustomerService();
            Customer customer = serviceCust.Create(optionCust);

            

            var optionsCharge = new ChargeCreateOptions
            {

                Amount = (long)total,
                Currency = "USD",
                Description = "Product Selling amount",
                Source = stripeToken,
                ReceiptEmail = stripeEmail


            };
            var serviceCharge = new ChargeService();
            Charge charge = serviceCharge.Create(optionsCharge);
            if (charge.Status != "succeeded")
            {
                ViewBag.BasketItems = basketItems;
                ModelState.AddModelError("Address", "Odenishde problem var");
                return View();
            }

            await _context.Orders.AddAsync(order);
            _context.BasketItems.RemoveRange(basketItems);
            await _context.SaveChangesAsync();

            string body = @"
<p>Your order succesfully placed<p>
                                <table border=""1"">
                                <thead>
                                    <tr>
                                        <th>Name</th>
                                        <th>Price</th>
                                        <th>Count</th>
                                    </tr>
                                </thead>
                                <tbody>";
            foreach (var item in basketItems)
            {
                body += @$"<tr>
                            <td>{item.Product.Name}</td>
                            <td>{item.Product.Price}</td>
                            <td>{item.Count}</td>
                          </tr>";
            }
            body += @"</tbody>
                    </table>";
            await _emailService.SendMailAsync(user.Email, "Your Order",body,true);

            return RedirectToAction("Index", "Home");

        }
        public async Task<IActionResult> Remove(int? id, string? returnurl)
        {
            if (id == null || id < 1) return BadRequest();
            if (User.Identity.IsAuthenticated)
            {

                BasketItem item = await _context.BasketItems.FirstOrDefaultAsync(bi => bi.ProductId == id);
                if (item == null) return NotFound();
                _context.BasketItems.RemoveRange(item);
                await _context.SaveChangesAsync();
            }
            else
            {

                string cookies = Request.Cookies["basket"];

                List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);
                BasketCookieItemVM item = basket.FirstOrDefault(b => b.ProductId == id);
                basket.Remove(item);
                cookies = JsonConvert.SerializeObject(basket);
                Response.Cookies.Append("basket", cookies);
            }
            if (returnurl is not null) return Redirect(returnurl);
            return RedirectToAction(nameof(Index));
        }
    }
}
