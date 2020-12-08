using System;
using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class CreateOrUpdateAppointmentDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime EstimatedStartTime { get; set; }

        [Required]
        public DateTime EstimatedEndTime { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string AppUserName { get; set; }

        [Required]
        [EmailAddress]
        public string AppUserEmail { get; set; }
        [Required]
        public bool IsCancelled { get; set; }
        [Required]
        public bool Done { get; set; }
    }
}