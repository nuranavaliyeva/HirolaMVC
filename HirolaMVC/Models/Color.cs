using HirolaMVC.Models.Base;

namespace HirolaMVC.Models
{
    public class Color:BaseEntity
    {
        public string Name { get; set; }
        public List<ProductColor> ProductColors { get; set; }
    }
}
