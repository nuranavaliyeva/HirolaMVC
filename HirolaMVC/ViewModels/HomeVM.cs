using HirolaMVC.Models;

namespace HirolaMVC.ViewModels
{
    public class HomeVM
    {
        public List<Slide> Slides { get; set; }
        public List<Product> Products { get; set; }
        public List<Product> NewProducts { get; set; }
        public List<Banner> Banners { get; set; }

    }
}
