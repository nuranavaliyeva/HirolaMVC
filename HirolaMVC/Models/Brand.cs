using HirolaMVC.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace HirolaMVC.Models
{
    public class Brand:BaseEntity
    {
        [MaxLength(50, ErrorMessage = "fghjhkjkl")]
        public string Name { get; set; }

        //relational
        public List<Product>? Products { get; set; }
    }
}
