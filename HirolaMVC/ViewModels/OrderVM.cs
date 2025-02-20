using System.ComponentModel.DataAnnotations;

namespace HirolaMVC.ViewModels
{
    public class OrderVM
    {
        public string Address { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        public List<BasketInOrderVM>? BasketInOrderVMs { get; set; }
    }
}
