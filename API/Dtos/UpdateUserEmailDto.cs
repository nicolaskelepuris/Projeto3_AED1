using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class UpdateUserEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}