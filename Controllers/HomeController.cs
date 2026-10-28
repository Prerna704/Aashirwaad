using Microsoft.AspNetCore.Mvc;
using Aashirwad.Services;
using Aashirwad.Models;
using MongoDB.Driver;

namespace Aashirwad.Controllers
{
    /// <summary>
    /// Home Controller - equivalent to the top-level routes in app.py
    /// Handles: /, /contact, /all_doctor, /confirmation, /stock_detail, /home_feedback, /blog
    /// </summary>
    public class HomeController : Controller
    {
        private readonly MongoDbService _db;

        public HomeController(MongoDbService db)
        {
            _db = db;
        }

        // GET /
        // Equivalent to: @app.route('/', methods=['GET'])
        [HttpGet("")]
        [HttpGet("home")]
        public IActionResult Index()
        {
            return View("Index");
        }

        // GET & POST /contact
        // Equivalent to: @app.route('/contact', methods=['GET', 'POST'])
        [HttpGet("contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost("contact")]
        public async Task<IActionResult> Contact(
            string name, string email, string phone, string city,
            string nearest_hospital, string subject, string message)
        {
            var contactData = new Contact
            {
                Name = name,
                Email = email,
                Number = phone,
                City = city,
                NearestHospital = nearest_hospital,
                Message = message,
                Subject = subject
            };
            await _db.Contacts.InsertOneAsync(contactData);
            return View();
        }

        // GET /all_doctor
        // Equivalent to: @app.route('/all_doctor')
        [HttpGet("all_doctor")]
        public IActionResult AllDoctor()
        {
            return View("Doctor");
        }

        // GET /confirmation
        // Equivalent to: @app.route('/confirmation', methods=['POST', 'GET'])
        [HttpGet("confirmation")]
        [HttpPost("confirmation")]
        public IActionResult Confirmation()
        {
            return View("Conformation");
        }

        // GET /stock_detail
        // Equivalent to: @app.route('/stock_detail')
        [HttpGet("stock_detail")]
        public IActionResult StockDetail()
        {
            return View("InvStockProduct");
        }

        // POST /home_feedback
        // Equivalent to: @app.route('/home_feedback', methods=['POST'])
        [HttpPost("home_feedback")]
        public async Task<IActionResult> HomeFeedback(
            string name, string email, string city,
            string nearest_hospital, string message)
        {
            var data = new Feedback
            {
                Name = name,
                Email = email,
                City = city,
                NearestHospital = nearest_hospital,
                Message = message
            };
            await _db.Feedback.InsertOneAsync(data);
            return Redirect("/");
        }

        // GET /blog
        // Equivalent to: @app.route('/blog', methods=['GET', 'POST'])
        [HttpGet("blog")]
        [HttpPost("blog")]
        public IActionResult Blog()
        {
            return View("BlogSingle");
        }

        // 404 Error Handler
        // Equivalent to: @app.errorhandler(404)
        [Route("error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
                return View("404");
            return View("Error");
        }
    }
}
