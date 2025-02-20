namespace HirolaMVC.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal SubTotal { get; set; }
        //relational
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
