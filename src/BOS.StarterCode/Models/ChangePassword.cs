using System.ComponentModel.DataAnnotations;
namespace BOS.StarterCode.Models
{
    public class ChangePassword
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,20}$", ErrorMessage = "The Password must be between 8 to 20 characters long and must contain atleast one upper case, one lower case, one numeric and one special character(!@.,#$%^&*)")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        public string UserId { get; set; }
    }
}
