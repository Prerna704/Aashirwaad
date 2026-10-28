using Microsoft.AspNetCore.Mvc;
using Aashirwad.Services;
using Aashirwad.Models;
using MongoDB.Driver;
using BCrypt.Net;

namespace Aashirwad.Controllers
{
    /// <summary>
    /// Doctor Controller - equivalent to Python's modules/doctor.py
    /// Handles: doctor login, dashboard, bed status, feedback, video call
    /// </summary>
    public class DoctorController : Controller
    {
        private readonly MongoDbService _db;

        public DoctorController(MongoDbService db)
        {
            _db = db;
        }

        // ─── AUTH HELPER ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Equivalent to Python's @login_required('doctor') decorator
        /// </summary>
        private bool IsDoctorLoggedIn() =>
            HttpContext.Session.GetString("username") != null &&
            HttpContext.Session.GetString("role") == "doctor";

        private IActionResult RequireDoctor()
        {
            TempData["Error"] = "Please login as doctor.";
            return RedirectToAction("Login");
        }

        // ─── LOGIN ───────────────────────────────────────────────────────────────────

        // GET & POST /doctor_login
        // Equivalent to: @doctor_blueprint.route('/doctor_login', methods=['POST', 'GET'])
        [HttpGet("/doctor_login")]
        public IActionResult Login() => View("DoctorLogin");

        [HttpPost("/doctor_login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var doctor = await _db.Doctors
                .Find(d => d.Username == username)
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                TempData["Error"] = "Username Not Found";
                return Redirect("/user_login");
            }

            // Equivalent to: bcrypt.check_password_hash(doctor['password'], password)
            if (!BCrypt.Net.BCrypt.Verify(password, doctor.Password))
            {
                TempData["Error"] = "Wrong Password";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("username", username);
            HttpContext.Session.SetString("hospital_name", doctor.HospitalName);
            HttpContext.Session.SetString("specialization", doctor.Specialization);
            HttpContext.Session.SetString("role", "doctor");

            return Redirect("/doctor_app");
        }

        // ─── DASHBOARD ───────────────────────────────────────────────────────────────

        // GET & POST /doctor_app
        // Equivalent to: @doctor_blueprint.route('/doctor_app', methods=["POST", "GET"])
        [HttpGet("/doctor_app")]
        [HttpPost("/doctor_app")]
        public async Task<IActionResult> DoctorApp()
        {
            if (!IsDoctorLoggedIn()) return RequireDoctor();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var specialization = HttpContext.Session.GetString("specialization") ?? "";
            var username = HttpContext.Session.GetString("username") ?? "";

            var appointments = await _db.Appointments
                .Find(a => a.HospitalName == hospitalName && a.Speciality == specialization)
                .ToListAsync();

            var docDetail = await _db.Doctors
                .Find(d => d.Username == username)
                .FirstOrDefaultAsync();

            var appointmentCount = await _db.Appointments
                .CountDocumentsAsync(a => a.HospitalName == hospitalName);

            ViewBag.Doctor = docDetail;
            ViewBag.TotalAppointments = appointmentCount;

            return View("DoctorDash", appointments);
        }

        // ─── BED STATUS ──────────────────────────────────────────────────────────────

        // GET & POST /bed_status
        // Equivalent to: @doctor_blueprint.route('/bed_status', methods=['GET', 'POST'])
        [HttpGet("/bed_status")]
        [HttpPost("/bed_status")]
        public async Task<IActionResult> BedStatus(string? hs_name)
        {
            var allHospitals = await _db.HospitalData.Find(_ => true).ToListAsync();
            var hospitalNames = allHospitals.Select(h => h.HospitalName).ToList();

            var noOfHospital = (await _db.HospitalData
                .DistinctAsync<string>("hospital_name", Builders<HospitalData>.Filter.Empty))
                .ToList().Count;

            var totalDoctor = (await _db.Doctors
                .DistinctAsync<string>("username", Builders<Doctor>.Filter.Empty))
                .ToList().Count;

            var activePatient = (await _db.Patients
                .DistinctAsync<string>("name", Builders<Patient>.Filter.Empty))
                .ToList().Count;

            // Filter hospitals based on selected hospital name
            IEnumerable<HospitalData> filtered = string.IsNullOrEmpty(hs_name)
                ? allHospitals
                : allHospitals.Where(h => h.HospitalName == hs_name);

            // Aggregate totals — equivalent to Python's MongoDB $group aggregation
            int totalBeds = filtered.Sum(h => h.NumberOfGeneralBeds);
            int occupiedBeds = filtered.Sum(h => h.OccupiedGeneral);
            int totalIcuBeds = filtered.Sum(h => h.NumberOfIcuBeds);
            int occupiedIcu = filtered.Sum(h => h.OccupiedIcu);
            int totalVentilators = filtered.Sum(h => h.NumberOfVentilators);
            int occupiedVentilators = filtered.Sum(h => h.OccupiedVentilator);

            ViewBag.Hospitals = hospitalNames;
            ViewBag.NoHospital = noOfHospital;
            ViewBag.Doctor = totalDoctor;
            ViewBag.Patient = activePatient;
            ViewBag.TotalGeneralBeds = totalBeds;
            ViewBag.AvailableBeds = totalBeds - occupiedBeds;
            ViewBag.TotalIcuBeds = totalIcuBeds;
            ViewBag.AvailableIcuBeds = totalIcuBeds - occupiedIcu;
            ViewBag.TotalVentilators = totalVentilators;
            ViewBag.AvailableVentilators = totalVentilators - occupiedVentilators;

            return View("BedStatus");
        }

        // ─── FEEDBACK ────────────────────────────────────────────────────────────────

        // GET & POST /doctor_feedback
        // Equivalent to: @doctor_blueprint.route('/doctor_feedback', methods=['GET','POST'])
        [HttpGet("/doctor_feedback")]
        public IActionResult DoctorFeedbackGet()
        {
            if (!IsDoctorLoggedIn()) return RequireDoctor();
            return View("Feedback");
        }

        [HttpPost("/doctor_feedback")]
        public async Task<IActionResult> DoctorFeedback(
            string uname, string email, string phone,
            string satisfy, string msg)
        {
            if (!IsDoctorLoggedIn()) return RequireDoctor();

            var feedbackData = new Feedback
            {
                Name = uname,
                Email = email,
                City = "",
                NearestHospital = "",
                Message = msg
            };
            await _db.Feedback.InsertOneAsync(feedbackData);
            return View("Feedback");
        }

        // ─── VIDEO CALL ──────────────────────────────────────────────────────────────

        // GET /video_call
        // Equivalent to: @doctor_blueprint.route('/video_call', methods=['GET'])
        [HttpGet("/video_call")]
        public IActionResult VideoCall()
        {
            return View("EmergencyScheduling");
        }
    }
}
