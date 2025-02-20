using HirolaMVC.Models.Base;

namespace HirolaMVC.Models
{
    public class Tag:BaseEntity
    {
        public string Name { get; set; }

        //reletional
        public List<ProductTag> ProductTags { get; set; }
    }
}
