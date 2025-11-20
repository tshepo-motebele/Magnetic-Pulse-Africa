using System.ComponentModel.DataAnnotations;

namespace MagneticPulseAfrica.Models
{


    public class TestimonialModel
    {
        // These fields should not be required as they're set by the controller
        public string? Id { get; set; }
        public string? Uid { get; set; }
        public string? Email { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Testimonial text is required")]
        [StringLength(1000, ErrorMessage = "Testimonial cannot be longer than 1000 characters")]
        public string TestimonialText { get; set; }

        // VideoUrl is optional
        public string? VideoUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; }
    }

}
