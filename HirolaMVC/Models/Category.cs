using HirolaMVC.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace HirolaMVC.Models
{
    public class Category:BaseEntity    
    {
        [MaxLength(30, ErrorMessage = "Please enter the correct name")]
        public string Name { get; set; }
        //relational
        public List<Product>? Products { get; set; }
    }
}
