using System;
using System.ComponentModel.DataAnnotations;
using Core.Entities.Identity;

namespace Core.Entities
{
    public class Appointment : BaseEntity
    {
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