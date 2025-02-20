using HirolaMVC.Models.Base;

namespace HirolaMVC.Models
{
    public class Order:BaseEntity
    {
        public decimal Total { get; set; }
        public bool? Status { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        //relational
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}
