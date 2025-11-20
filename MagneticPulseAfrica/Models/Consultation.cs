using System.ComponentModel.DataAnnotations;

namespace MagneticPulseAfrica.Models
{
    public class Consultation
    {

        public string? Id { get; set; }
        [Required(ErrorMessage = "Contact Number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Surname must be between 2 and 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Physical Address is required")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Physical Address must be between 10 and 200 characters")]
        [Display(Name = "Physical Address")]
        public string PhysicalAddress { get; set; }

        [Required(ErrorMessage = "Reason for Consultation is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters")]
        [Display(Name = "Reason for Consultation")]
        public string ConsultationReason { get; set; }

        [Required]
        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; }
        
        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [Required]
        [Display(Name = "Status")]
        public BookingStatus Status { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled,
        Rescheduled
    }
}