using Firebase.Auth;
using MagneticPulseAfrica.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json.Linq;


namespace MagneticPulseAfrica.Controllers
{
    public class AdminController : Controller
    {
        private readonly FirebaseAuthProvider auth;
        private readonly IFirebaseClient client;
        private readonly ILogger<AdminController> _logger;

        public AdminController()
        {
            auth = new FirebaseAuthProvider(
                new FirebaseConfig("AIzaSyDeQLsYKOx7FP8cqJWMwWLkE-wU5bNA0wQ"));
            var config = new FireSharp.Config.FirebaseConfig
            {
                AuthSecret = "54SNbFZHBWjRvEug2RZekfUC8BMxOj5P1FtPY9pD",
                BasePath = "https://magnetic-pulse-africa-c92c8-default-rtdb.firebaseio.com"
            };

            client = new FireSharp.FirebaseClient(config);
            
        
        }

        [Authorize]
        public IActionResult Index()
        {
            try
            {
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Unable to connect to the database.";
                    return View(new DashboardViewModel { Stats = new DashboardStats() }); // Initialize with empty stats
                }

                // Reuse the ViewData logic since it's already implemented
                return ViewData();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return View(new DashboardViewModel { Stats = new DashboardStats() }); // Initialize with empty stats
            }
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(model.Username, model.Password);
                string token = fbAuthLink.FirebaseToken;

                if (token != null)
                {
                    // Store the token in session
                    HttpContext.Session.SetString("_UserToken", token);

                    // Create authentication cookie
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, model.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Authentication failed" });
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                return Json(new { success = false, message = firebaseEx.error.message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }
        [Authorize]
        public IActionResult ViewData()
        {
            try
            {
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Unable to connect to the database.";
                    return View(new DashboardViewModel()); // Return empty view model
                }

                var viewModel = new DashboardViewModel();

                // Fetch Consultations
                FirebaseResponse consultationResponse = client.Get("Consultations");
                dynamic consultationData = JsonConvert.DeserializeObject<dynamic>(consultationResponse.Body);
                if (consultationData != null)
                {
                    foreach (var item in consultationData)
                    {
                        viewModel.Consultations.Add(JsonConvert.DeserializeObject<Consultation>(((JProperty)item).Value.ToString()));
                    }
                }

                // Fetch Testimonials
                FirebaseResponse testimonialsResponse = client.Get("Testimonials");
                dynamic testimonialData = JsonConvert.DeserializeObject<dynamic>(testimonialsResponse.Body);
                if (testimonialData != null)
                {
                    foreach (var item in testimonialData)
                    {
                        viewModel.Testimonials.Add(JsonConvert.DeserializeObject<TestimonialModel>(((JProperty)item).Value.ToString()));
                    }
                }

                // Calculate Dashboard Stats
                viewModel.Stats = new DashboardStats
                {
                    TotalBookings = viewModel.Consultations?.Count ?? 0,
                    TotalReviews = viewModel.Testimonials?.Count ?? 0,
                   
                    AverageRating = viewModel.Testimonials != null && viewModel.Testimonials.Any()
         ? Math.Round(viewModel.Testimonials.Average(t => t.Rating), 1)
         : 0.0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving data: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while fetching data.";
                return View(new DashboardViewModel()); // Return empty view model
            }
        }
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Unable to connect to the database.";
                    return RedirectToAction("Index");
                }

                FirebaseResponse response = await client.GetAsync($"Consultations/{id}");
                var consultation = JsonConvert.DeserializeObject<Consultation>(response.Body);

                if (consultation == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("Index");
                }

                return View(consultation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving booking details: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while fetching the booking details.";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = "Invalid booking ID.";
                    return RedirectToAction("Index");
                }

                if (client == null)
                {
                    TempData["ErrorMessage"] = "Unable to connect to the database.";
                    return RedirectToAction("Index");
                }

                // First check if the consultation exists
                var getResponse = await client.GetAsync($"Consultations/{id}");
                if (getResponse.Body == "null")
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("Index");
                }

                // Perform the deletion
                await client.DeleteAsync($"Consultations/{id}");

                TempData["SuccessMessage"] = "Booking deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting booking: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the booking.";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [Route("Admin/TestimonialDetails/{id}")]
        public async Task<IActionResult> TestimonialDetails(string id)
        {
            try
            {
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Unable to connect to the database.";
                    return RedirectToAction("Index");
                }

                FirebaseResponse response = await client.GetAsync($"Testimonials/{id}");
                var testimonial = JsonConvert.DeserializeObject<TestimonialModel>(response.Body);

                if (testimonial == null)
                {
                    TempData["ErrorMessage"] = "Testimonial not found.";
                    return RedirectToAction("Index");
                }

                // Be more explicit about the view path
                return View("~/Views/Admin/TestimonialDetails.cshtml", testimonial);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving testimonial details: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while fetching the testimonial details.";
                return RedirectToAction("Index");
            }
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTestimonial(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid testimonial ID." });
                }

                if (client == null)
                {
                    return Json(new { success = false, message = "Unable to connect to the database." });
                }

                // Get the testimonial to check if it exists and get the video URL
                var getResponse = await client.GetAsync($"Testimonials/{id}");
                var testimonial = JsonConvert.DeserializeObject<TestimonialModel>(getResponse.Body);

                if (testimonial == null)
                {
                    return Json(new { success = false, message = "Testimonial not found." });
                }

                // Delete the video from storage if it exists
                if (!string.IsNullOrEmpty(testimonial.VideoUrl))
                {
                    // Extract the file name from the URL and delete from storage
                    var fileName = Path.GetFileName(testimonial.VideoUrl);
                    // Add your storage deletion logic here
                }

                // Delete the testimonial from the database
                await client.DeleteAsync($"Testimonials/{id}");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting testimonial: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting the testimonial." });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditTestimonial(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid testimonial ID." });
                }

                if (client == null)
                {
                    return Json(new { success = false, message = "Unable to connect to the database." });
                }

                // Get the current testimonial
                FirebaseResponse response = await client.GetAsync($"Testimonials/{id}");
                var testimonial = JsonConvert.DeserializeObject<TestimonialModel>(response.Body);

                if (testimonial == null)
                {
                    return Json(new { success = false, message = "Testimonial not found." });
                }

                return View(testimonial);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting testimonial: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while getting the testimonial." });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTestimonial(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid testimonial ID." });
                }

                if (client == null)
                {
                    return Json(new { success = false, message = "Unable to connect to the database." });
                }

                // Get the current testimonial
                FirebaseResponse getResponse = await client.GetAsync($"Testimonials/{id}");
                var testimonial = JsonConvert.DeserializeObject<TestimonialModel>(getResponse.Body);

                if (testimonial == null)
                {
                    return Json(new { success = false, message = "Testimonial not found." });
                }

                // Update the isApproved status
                testimonial.IsApproved = true;

                // Use SetAsync instead of UpdateAsync to ensure the entire object is updated
                SetResponse setResponse = await client.SetAsync($"Testimonials/{id}", testimonial);

                if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Testimonial approved successfully",
                        isApproved = testimonial.IsApproved
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to approve testimonial." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving testimonial: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while approving the testimonial." });
            }
        }
        private string GetStatusClass(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "bg-warning",
                BookingStatus.Confirmed => "bg-primary",
                BookingStatus.Completed => "bg-success",
                BookingStatus.Cancelled => "bg-danger",
                BookingStatus.Rescheduled => "bg-info",
                _ => "bg-secondary"
            };
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromForm] string id, [FromForm] int status)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { success = false, message = "Invalid booking ID." });
                }

                if (client == null)
                {
                    return StatusCode(500, new { success = false, message = "Database connection error." });
                }

                // Get the current booking
                FirebaseResponse getResponse = await client.GetAsync($"Consultations/{id}");

                if (getResponse.Body == "null")
                {
                    return NotFound(new { success = false, message = "Booking not found." });
                }

                var booking = JsonConvert.DeserializeObject<Consultation>(getResponse.Body);

                if (booking == null)
                {
                    return StatusCode(500, new { success = false, message = "Error retrieving booking details." });
                }

                // Validate the status value
                if (!Enum.IsDefined(typeof(BookingStatus), status))
                {
                    return BadRequest(new { success = false, message = "Invalid status value." });
                }

                // Update the booking
                booking.Status = (BookingStatus)status;
                booking.LastUpdated = DateTime.UtcNow;

                // Save to Firebase
                var setResponse = await client.SetAsync($"Consultations/{id}", booking);

                if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Status updated successfully",
                        newStatus = booking.Status.ToString(),
                        statusClass = GetStatusClass(booking.Status),
                        lastUpdated = booking.LastUpdated?.ToString("MMM dd, yyyy HH:mm")
                    });
                }

                return StatusCode(500, new { success = false, message = "Failed to save changes." });
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error updating booking status: {ex.Message}", ex);
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("_UserToken");
            return RedirectToAction("Login");
        }
    }
}
