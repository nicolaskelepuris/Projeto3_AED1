using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class UpdateUserPasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [RegularExpression("^.{5,15}$", ErrorMessage = "Password must have minimum 5 character and maximum 15 characters")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}