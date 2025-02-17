
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HirolaMVC.Controllers
{
    public class HomeController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }

     
    }
}
