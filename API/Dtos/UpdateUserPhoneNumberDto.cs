using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class UpdateUserPhoneNumberDto
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}