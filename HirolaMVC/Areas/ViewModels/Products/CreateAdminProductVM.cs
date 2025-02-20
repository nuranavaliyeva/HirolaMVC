using HirolaMVC.Models;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace HirolaMVC.Areas.ViewModels.Products
{
    public class CreateAdminProductVM
    {
        public IFormFile MainPhoto { get; set; }
        public IFormFile HoverPhoto { get; set; }
        public List<IFormFile>? AdditionalPhotos { get; set; }
        public string Name { get; set; }
        [Required]
        public decimal? Price { get; set; }
        public string ProductCode { get; set; }

        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        [Required(ErrorMessage = "Category daxil et")]
        public int? CategoryId { get; set; }
        public List<int>? TagIds { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<int>? ColorIds { get; set; }
        public List<Models.Color>? Colors { get; set; }
        public int? BrandIds { get; set; }
        public List<Brand>? Brands { get; set; }

    }
}
