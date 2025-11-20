namespace MagneticPulseAfrica.Models
{
    public class ServicesViewModel
    {
        public IEnumerable<TestimonialModel> Testimonials { get; set; }
        public TestimonialModel NewTestimonial { get; set; }

        public ServicesViewModel()
        {
            Testimonials = new List<TestimonialModel>();
            NewTestimonial = new TestimonialModel();
        }
    }
}
