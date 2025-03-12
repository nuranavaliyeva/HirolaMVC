using System.ComponentModel.DataAnnotations;

namespace HirolaMVC.ViewModels
{
    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
       
        public string CurrentPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        [MinLength(8)]
        public string ConfirmNewPassword { get; set; }
    }
}
