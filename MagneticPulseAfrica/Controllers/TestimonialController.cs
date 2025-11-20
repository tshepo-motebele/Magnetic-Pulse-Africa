using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using MagneticPulseAfrica.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MagneticPulseAfrica.Controllers
{
    public class TestimonialController : Controller
    {
        private readonly IFirebaseClient client;
        private readonly ILogger<TestimonialController> _logger;
        private readonly string _uploadDirectory;

        public TestimonialController(ILogger<TestimonialController> logger)
        {
            _logger = logger;
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");

            var config = new FirebaseConfig
            {
                AuthSecret = "54SNbFZHBWjRvEug2RZekfUC8BMxOj5P1FtPY9pD",
                BasePath = "https://magnetic-pulse-africa-c92c8-default-rtdb.firebaseio.com"
            };

            try
            {
                client = new FireSharp.FirebaseClient(config);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Firebase initialization error: {ex.Message}");
                client = null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([FromForm] string Name, [FromForm] int Rating,
            [FromForm] string TestimonialText, IFormFile? videoFile)
        {
            try
            {
                if (client == null)
                {
                    return Json(new { success = false, message = "Database connection error" });
                }

                var testimonial = new TestimonialModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Name,
                    Rating = Rating,
                    TestimonialText = TestimonialText,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false // Set to true if you want immediate approval
                };

                // Handle video upload if present
                if (videoFile != null && videoFile.Length > 0)
                {
                    var (success, message, videoUrl) = await HandleVideoUpload(videoFile, testimonial.Id);
                    if (!success)
                    {
                        return Json(new { success = false, message });
                    }
                    testimonial.VideoUrl = videoUrl;
                }

                // Save to Firebase
                SetResponse response = await client.SetAsync($"Testimonials/{testimonial.Id}", testimonial);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Json(new { success = true, message = "Testimonial submitted successfully" });
                }

                return Json(new { success = false, message = "Failed to save testimonial" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting testimonial: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while submitting testimonial" });
            }
        }

        private async Task<(bool success, string message, string videoUrl)> HandleVideoUpload(IFormFile videoFile, string testimonialId)
        {
            try
            {
                if (videoFile.Length > 100 * 1024 * 1024)
                {
                    return (false, "File size exceeds 100MB limit", null);
                }

                var allowedTypes = new[] { "video/mp4", "video/quicktime", "video/x-msvideo" };
                if (!allowedTypes.Contains(videoFile.ContentType.ToLower()))
                {
                    return (false, "Invalid file type", null);
                }

                var fileName = $"{testimonialId}_{Path.GetFileName(videoFile.FileName)}";
                var filePath = Path.Combine(_uploadDirectory, fileName);

                Directory.CreateDirectory(_uploadDirectory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }

                return (true, "File uploaded successfully", $"/videos/{fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading video: {ex.Message}");
                return (false, "Error uploading video", null);
            }
        }
    }
}
