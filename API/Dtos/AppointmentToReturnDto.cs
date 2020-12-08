using System;
using System.Collections.Generic;
namespace API.Dtos
{
    public class AppointmentToReturnDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedEndTime { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string AppUserName { get; set; }
        public string AppUserEmail { get; set; }
        public bool IsCancelled { get; set; }
        public bool Done { get; set; }
    }
}