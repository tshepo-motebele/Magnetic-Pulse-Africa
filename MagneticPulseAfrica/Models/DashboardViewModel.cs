using System.Collections.Generic;

namespace MagneticPulseAfrica.Models
{
    public class DashboardViewModel
    {

        public List<Consultation> Consultations { get; set; } = new List<Consultation>();
        public List<TestimonialModel> Testimonials { get; set; } = new List<TestimonialModel>();
        public DashboardStats Stats { get; set; }
    }
}
