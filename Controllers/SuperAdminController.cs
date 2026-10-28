using Microsoft.AspNetCore.Mvc;
using Aashirwad.Services;
using Aashirwad.Models;
using MongoDB.Driver;
using BCrypt.Net;
using System.Net;
using System.Net.Mail;

namespace Aashirwad.Controllers
{
    /// <summary>
    /// SuperAdmin Controller - equivalent to Python's modules/superadmin.py
    /// Handles: superadmin login, system-wide dashboard, hospital management, email notifications
    /// </summary>
    public class SuperAdminController : Controller
    {
        private readonly MongoDbService _db;
        private readonly IConfiguration _config;

        // Email config — equivalent to Python's my_email and code variables from db.py
        private readonly string _myEmail = "prernaprem720@gmail.com";
        private readonly string _emailCode = "zuff vkvx pamt kdor";

        public SuperAdminController(MongoDbService db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ─── AUTH HELPER ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Equivalent to Python's @login_required('superadmin') decorator
        /// </summary>
        private bool IsSuperAdminLoggedIn() =>
            HttpContext.Session.GetString("username") != null &&
            HttpContext.Session.GetString("role") == "superadmin";

        private IActionResult RequireSuperAdmin()
        {
            TempData["Error"] = "Access Denied";
            return RedirectToAction("Login");
        }

        // ─── LOGIN ───────────────────────────────────────────────────────────────────

        // GET & POST /superadmin_login
        // Equivalent to: @superadmin_blueprint.route("/superadmin_login", methods=['GET', 'POST'])
        [HttpGet("/superadmin_login")]
        public IActionResult Login() => View("SuperAdminLogin");

        [HttpPost("/superadmin_login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Note: Python's superadmin uses plain-text password comparison (not bcrypt)
            // Equivalent to: superadmin_collection.find_one({"username": username, "password": password})
            var admin = await _db.SuperAdmins
                .Find(a => a.Username == username && a.Password == password)
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                TempData["Error"] = "Access Denied";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("username", username);
            HttpContext.Session.SetString("role", "superadmin");

            return Redirect("/superadmin");
        }

        // ─── DASHBOARD ───────────────────────────────────────────────────────────────

        // GET & POST /superadmin/
        // Equivalent to: @superadmin_blueprint.route('/superadmin/', methods=['GET', 'POST'])
        [HttpGet("/superadmin")]
        [HttpGet("/superadmin/")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsSuperAdminLoggedIn()) return RequireSuperAdmin();

            var allHospitals = await _db.HospitalData.Find(_ => true).ToListAsync();

            var noOfHospital = (await _db.HospitalData
                .DistinctAsync<string>("hospital_name", Builders<HospitalData>.Filter.Empty))
                .ToList().Count;

            var totalDoctor = (await _db.Doctors
                .DistinctAsync<string>("username", Builders<Doctor>.Filter.Empty))
                .ToList().Count;

            var activePatient = (await _db.Patients
                .DistinctAsync<string>("name", Builders<Patient>.Filter.Empty))
                .ToList().Count;

            // Aggregate bed stats — equivalent to Python's $group aggregations
            int totalBeds = allHospitals.Sum(h => h.NumberOfGeneralBeds);
            int occupiedBeds = allHospitals.Sum(h => h.OccupiedGeneral);
            int totalIcuBeds = allHospitals.Sum(h => h.NumberOfIcuBeds);
            int occupiedIcu = allHospitals.Sum(h => h.OccupiedIcu);
            int totalVentilators = allHospitals.Sum(h => h.NumberOfVentilators);
            int occupiedVentilators = allHospitals.Sum(h => h.OccupiedVentilator);
            int totalNurses = allHospitals.Sum(h => h.TotalNumberOfNurses);
            int totalAdminStaff = allHospitals.Sum(h => h.AdministrativeStaffCount);

            ViewBag.NoHospital = noOfHospital;
            ViewBag.Doctor = totalDoctor;
            ViewBag.Patient = activePatient;
            ViewBag.TotalBeds = totalBeds;
            ViewBag.AvailableBeds = totalBeds - occupiedBeds;
            ViewBag.TotalIcuBeds = totalIcuBeds;
            ViewBag.AvailableIcuBeds = totalIcuBeds - occupiedIcu;
            ViewBag.TotalVentilators = totalVentilators;
            ViewBag.AvailableVentilators = totalVentilators - occupiedVentilators;
            ViewBag.TotalAdminStaff = totalAdminStaff;
            ViewBag.TotalNurse = totalNurses;

            return View("SuperAdminDash");
        }

        // ─── ADD HOSPITAL ────────────────────────────────────────────────────────────

        // GET & POST /superadmin/addHospital
        // Equivalent to: @superadmin_blueprint.route('/superadmin/addHospital', methods=['GET', 'POST'])
        [HttpGet("/superadmin/addHospital")]
        public IActionResult AddHospital()
        {
            if (!IsSuperAdminLoggedIn()) return RequireSuperAdmin();
            return View("SuperAddHospital");
        }

        [HttpPost("/superadmin/addHospital")]
        public async Task<IActionResult> AddHospital(
            string hospitalName, string hospitalmail, string hospitalpass)
        {
            if (!IsSuperAdminLoggedIn()) return RequireSuperAdmin();

            // Check duplicates — equivalent to Python's admin_collection.find_one checks
            var existingHospital = await _db.Admins
                .Find(a => a.HospitalName == hospitalName).AnyAsync();
            var existingEmail = await _db.Admins
                .Find(a => a.HospitalMail == hospitalmail).AnyAsync();

            if (existingHospital)
                return Content("Username already exists. Please choose a different username.");
            if (existingEmail)
                return Content("Email already exists. Please use a different email address.");

            // Hash password — equivalent to Python's bcrypt.generate_password_hash
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(hospitalpass);

            var hospitalData = new Admin
            {
                HospitalName = hospitalName,
                HospitalMail = hospitalmail,
                HospitalPassword = hashedPassword
            };
            await _db.Admins.InsertOneAsync(hospitalData);

            // Send email — equivalent to Python's smtplib.SMTP email sending
            await SendHospitalCredentialEmail(hospitalmail, hospitalpass);

            return View("SuperAddHospital");
        }

        // ─── CHECK HOSPITAL STATUS ───────────────────────────────────────────────────

        // GET & POST /superadmin/checkHospitalStatus
        // Equivalent to: @superadmin_blueprint.route('/superadmin/checkHospitalStatus', ...)
        [HttpGet("/superadmin/checkHospitalStatus")]
        public async Task<IActionResult> CheckHospitalStatus()
        {
            if (!IsSuperAdminLoggedIn()) return RequireSuperAdmin();

            var hospitals = await _db.HospitalData.Find(_ => true).ToListAsync();
            ViewBag.Hospitals = hospitals.Select(h => h.HospitalName).ToList();
            return View("SuperAdminCheckHospital");
        }

        [HttpPost("/superadmin/checkHospitalStatus")]
        public async Task<IActionResult> CheckHospitalStatus(string hname)
        {
            if (!IsSuperAdminLoggedIn()) return RequireSuperAdmin();

            if (string.IsNullOrEmpty(hname))
                return BadRequest("Hospital name is missing");

            var data = await _db.HospitalData
                .Find(h => h.HospitalName == hname)
                .FirstOrDefaultAsync();

            if (data == null)
                return Content("No hospital found");

            var noDoc = await _db.Doctors
                .CountDocumentsAsync(d => d.HospitalName == hname);

            ViewBag.Data = data;
            ViewBag.NoDoc = noDoc;
            return View("SuperadminHospitalStatus");
        }

        // ─── FEEDBACK ────────────────────────────────────────────────────────────────

        // GET & POST /superadmin/feedback
        // Equivalent to: @superadmin_blueprint.route('/superadmin/feedback', methods=['GET', 'POST'])
        [HttpGet("/superadmin/feedback")]
        [HttpPost("/superadmin/feedback")]
        public IActionResult Feedback()
        {
            if (!IsSuperAdminLoggedIn()) return RequireSuperAdmin();
            return View("AdminFeedback");
        }

        // ─── EMAIL HELPER ────────────────────────────────────────────────────────────

        /// <summary>
        /// Equivalent to Python's smtplib email sending in add_hospital route
        /// Sends hospital credentials via Gmail SMTP
        /// </summary>
        private async Task SendHospitalCredentialEmail(string toEmail, string plainPassword)
        {
            try
            {
                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(_myEmail, _emailCode),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_myEmail),
                    Subject = "Hospital Account Credentials",
                    Body = $@"Dear,
Your Hospital Account (Email ID {toEmail}) Password is: {plainPassword}.

(Generated at {DateTime.Now})

********************************
This is an auto-generated email. Do not reply to this email.",
                    IsBodyHtml = false
                };
                message.To.Add(toEmail);

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }
    }
}
