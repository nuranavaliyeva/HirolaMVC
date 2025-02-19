using System.ComponentModel.DataAnnotations;

namespace HirolaMVC.ViewModels.Account
{
    public class LoginVM
    {
        [MaxLength(256)]
        [MinLength(4)]
        public string UsernameOrEmail { get; set; }
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsPersistent { get; set; }
    }
}
