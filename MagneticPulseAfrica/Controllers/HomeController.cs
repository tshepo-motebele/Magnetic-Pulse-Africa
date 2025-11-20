using FireSharp.Interfaces;
using FireSharp.Response;
using MagneticPulseAfrica.Models;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MagneticPulseAfrica.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFirebaseClient _client;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            var config = new FireSharp.Config.FirebaseConfig
            {
                AuthSecret = "54SNbFZHBWjRvEug2RZekfUC8BMxOj5P1FtPY9pD",
                BasePath = "https://magnetic-pulse-africa-c92c8-default-rtdb.firebaseio.com"
            };
            _client = new FireSharp.FirebaseClient(config);
        }
        public async Task<IActionResult> Services()
        {
            var viewModel = new ServicesViewModel();
            try
            {
                if (_client != null)
                {
                    FirebaseResponse response = await _client.GetAsync("Testimonials");
                    var data = JsonConvert.DeserializeObject<Dictionary<string, TestimonialModel>>(response.Body);
                    if (data != null)
                    {
                        viewModel.Testimonials = data
                            .Select(kvp =>
                            {
                                var testimonial = kvp.Value;
                                testimonial.Id = kvp.Key;
                                return testimonial;
                            })
                            .Where(t => t.IsApproved)
                            .OrderByDescending(t => t.CreatedAt)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching testimonials: {ex.Message}");
            }
            return View(viewModel);
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        

        public ActionResult Contact()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}