using Microsoft.AspNetCore.Mvc;
using Aashirwad.Services;
using Aashirwad.Models;
using MongoDB.Driver;
using BCrypt.Net;

namespace Aashirwad.Controllers
{
    /// <summary>
    /// User Controller - equivalent to Python's modules/user.py
    /// Handles: user login/register, appointments, doctor/specialization fetch, queue system
    /// </summary>
    public class UserController : Controller
    {
        private readonly MongoDbService _db;

        public UserController(MongoDbService db)
        {
            _db = db;
        }

        // ─── AUTH HELPER ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Equivalent to Python's @login_required('user') decorator
        /// </summary>
        private bool IsUserLoggedIn() =>
            HttpContext.Session.GetString("username") != null &&
            HttpContext.Session.GetString("role") == "user";

        private IActionResult RequireUser()
        {
            TempData["Error"] = "Please login.";
            return RedirectToAction("Login");
        }

        // ─── LOGIN ───────────────────────────────────────────────────────────────────

        // GET & POST /user_login
        // Equivalent to: @user_blueprint.route('/user_login', methods=['POST', 'GET'])
        [HttpGet("/user_login")]
        public IActionResult Login() => View("UserLoginRegister");

        [HttpPost("/user_login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _db.Users
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Login");
            }

            // Equivalent to: bcrypt.check_password_hash(user['password'], password)
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                TempData["Error"] = "Incorrect Password";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("username", username);
            HttpContext.Session.SetString("role", "user");

            return Redirect("/user_app");
        }

        // ─── REGISTER ────────────────────────────────────────────────────────────────

        // GET & POST /user_register
        // Equivalent to: @user_blueprint.route('/user_register', methods=['GET', 'POST'])
        [HttpGet("/user_register")]
        public IActionResult Register() => View("UserLoginRegister");

        [HttpPost("/user_register")]
        public async Task<IActionResult> Register(
            string name, string phone, string email, string username, string password)
        {
            // Check for existing username or email — equivalent to Python's find_one checks
            var existingUser = await _db.Users
                .Find(u => u.Username == username).AnyAsync();
            var existingEmail = await _db.Users
                .Find(u => u.Email == email).AnyAsync();

            if (existingUser)
                return Content("Username already exists. Please choose a different username.");
            if (existingEmail)
                return Content("Email already exists. Please use a different email address.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var userData = new User
            {
                Name = name,
                Username = username,
                Email = email,
                Number = phone,
                Password = hashedPassword
            };

            await _db.Users.InsertOneAsync(userData);
            return Redirect("/user_login");
        }

        // ─── USER DASHBOARD ──────────────────────────────────────────────────────────

        // GET & POST /user_app
        // Equivalent to: @user_blueprint.route('/user_app', methods=['GET', 'POST'])
        [HttpGet("/user_app")]
        [HttpPost("/user_app")]
        public async Task<IActionResult> UserApp()
        {
            if (!IsUserLoggedIn()) return RequireUser();

            var username = HttpContext.Session.GetString("username") ?? "";
            var userInfo = await _db.Users
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();

            var appointments = await _db.Appointments
                .Find(a => a.Username == username)
                .ToListAsync();

            ViewBag.UserInfo = userInfo;
            return View("UserApp", appointments);
        }

        // ─── APPOINTMENT BOOKING ─────────────────────────────────────────────────────

        // GET & POST /appointment
        // Equivalent to: @user_blueprint.route('/appointment', methods=['POST', 'GET'])
        [HttpGet("/appointment")]
        public async Task<IActionResult> AppointmentGet()
        {
            if (!IsUserLoggedIn()) return RequireUser();

            var hospitals = await _db.HospitalData.Find(_ => true).ToListAsync();
            var hospitalNames = hospitals.Select(h => h.HospitalName).ToList();

            ViewBag.Hospitals = hospitalNames;
            ViewBag.Today = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.MaxDate = DateTime.Today.AddDays(15).ToString("yyyy-MM-dd");

            return View("Appointment");
        }

        [HttpPost("/appointment")]
        public async Task<IActionResult> AppointmentPost(
            string name, string number, string email, string Address,
            string dat, string timeSlot, string specialization,
            string diseaseDescription, string hospital, string doctor)
        {
            if (!IsUserLoggedIn()) return RequireUser();

            var username = HttpContext.Session.GetString("username") ?? "";

            // Fetch doctor names — equivalent to Python's doctors_collection.find
            var doctorNames = await _db.Doctors
                .Find(d => d.HospitalName == hospital && d.Specialization == specialization)
                .ToListAsync();

            if (!doctorNames.Any())
            {
                TempData["Error"] = $"Doctor for the selected field is not available in {hospital}. Sorry for the inconvenience";
                return Redirect("/appointment");
            }

            // Check if slot is full — equivalent to Python's check_and_allocate_time_slot
            bool isSlotFull = await CheckAndAllocateTimeSlot(dat, timeSlot, hospital, specialization);
            if (isSlotFull)
            {
                TempData["Error"] = "The selected time slot is full. Please choose another time or date.";
                return Redirect("/appointment");
            }

            // Calculate queue number — equivalent to Python's calculate_queue_number
            int queueNumber = await CalculateQueueNumber(dat, timeSlot, hospital, specialization);

            var appointmentData = new Appointment
            {
                Name = name,
                Username = username,
                Number = number,
                Email = email,
                Address = Address,
                AppointmentDate = dat,
                TimeSlot = timeSlot,
                Speciality = specialization,
                DiseaseDescription = diseaseDescription,
                HospitalName = hospital,
                QueueNumber = queueNumber,
                AppointedDoc = doctor
            };

            await _db.Appointments.InsertOneAsync(appointmentData);
            return Redirect("/confirmation");
        }

        // ─── AJAX HELPERS ────────────────────────────────────────────────────────────

        // GET /get_doctors/<hospital>/<speciality>
        // Equivalent to: @user_blueprint.route('/get_doctors/<hospital>/<speciality>', methods=['GET'])
        [HttpGet("/get_doctors/{hospital}/{speciality}")]
        public async Task<IActionResult> GetDoctors(string hospital, string speciality)
        {
            if (!IsUserLoggedIn())
                return Json(new { doctors = new List<string>() });

            var doctors = await _db.Doctors
                .Find(d => d.HospitalName == hospital && d.Specialization == speciality)
                .ToListAsync();

            var doctorNames = doctors.Select(d => d.Name).ToList();
            return Json(new { doctors = doctorNames });
        }

        // GET /get_specializations?hospital_name=...
        // Equivalent to: @user_blueprint.route('/get_specializations', methods=['GET'])
        [HttpGet("/get_specializations")]
        public async Task<IActionResult> GetSpecializations(string hospital_name)
        {
            var hospitalData = await _db.HospitalData
                .Find(h => h.HospitalName == hospital_name)
                .FirstOrDefaultAsync();

            if (hospitalData == null)
                return Json(new { error = "Hospital not found" });

            return Json(new { specializations = hospitalData.Specialization });
        }

        // ─── QUEUEING SYSTEM ─────────────────────────────────────────────────────────

        /// <summary>
        /// Equivalent to Python's check_and_allocate_time_slot()
        /// Returns true if the slot is full (count >= 3 * doctorCount)
        /// </summary>
        private async Task<bool> CheckAndAllocateTimeSlot(
            string appointmentDate, string timeSlot, string hospitalName, string speciality)
        {
            var doctorCount = await _db.Doctors
                .CountDocumentsAsync(d => d.HospitalName == hospitalName && d.Specialization == speciality);

            var count = await _db.Appointments
                .CountDocumentsAsync(a =>
                    a.AppointmentDate == appointmentDate &&
                    a.TimeSlot == timeSlot &&
                    a.HospitalName == hospitalName &&
                    a.Speciality == speciality);

            return count >= 3 * doctorCount;
        }

        /// <summary>
        /// Equivalent to Python's calculate_queue_number()
        /// Returns the next queue number for the given slot
        /// </summary>
        private async Task<int> CalculateQueueNumber(
            string appointmentDate, string timeSlot, string hospitalName, string speciality)
        {
            var count = await _db.Appointments
                .CountDocumentsAsync(a =>
                    a.AppointmentDate == appointmentDate &&
                    a.TimeSlot == timeSlot &&
                    a.HospitalName == hospitalName &&
                    a.Speciality == speciality);

            return (int)count + 1;
        }
    }
}
