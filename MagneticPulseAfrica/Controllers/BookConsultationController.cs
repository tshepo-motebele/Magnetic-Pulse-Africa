using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using MagneticPulseAfrica.Models;
using MagneticPulseAfrica.Services;
using Microsoft.AspNetCore.Mvc;

namespace MagneticPulseAfrica.Controllers
{
    public class BookConsultationController : Controller
    {
        private readonly IFirebaseClient client;
        private readonly ISmsService _smsService;

        public BookConsultationController(ISmsService smsService)
        {

            _smsService = smsService;

            var config = new FirebaseConfig
            {
                AuthSecret = "54SNbFZHBWjRvEug2RZekfUC8BMxOj5P1FtPY9pD",
                BasePath = "https://magnetic-pulse-africa-c92c8-default-rtdb.firebaseio.com"
            };

            try
            {
                client = new FireSharp.FirebaseClient(config);
            }
            catch (Exception)
            {
                // Log the error
                client = null;
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(Consultation consultation)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", consultation);
            }

            try
            {
                if (client == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to connect to the database");
                    return View("Index", consultation);
                }

                // Generate a timestamp-based ID
                consultation.Id = DateTime.UtcNow.Ticks.ToString();

                // Push to Firebase
                var response = await Task.Run(() => client.Set($"Consultations/{consultation.Id}", consultation));

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await _smsService.SendConsultationNotificationAsync(consultation);
                    TempData["SuccessMessage"] = "Your consultation has been booked successfully!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "Failed to save consultation. Please try again.");
                return View("Index", consultation);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while booking your consultation. Please try again.");
                // Log the actual exception details
                return View("Index", consultation);
            }
        }
    }
}