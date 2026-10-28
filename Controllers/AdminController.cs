using Microsoft.AspNetCore.Mvc;
using Aashirwad.Services;
using Aashirwad.Models;
using MongoDB.Driver;
using BCrypt.Net;
using MongoDB.Bson;

namespace Aashirwad.Controllers
{
    /// <summary>
    /// Admin Controller - equivalent to Python's modules/admin.py
    /// Handles all admin routes: dashboard, patients, doctors, discharge, inventory, hospital settings
    /// </summary>
    public class AdminController : Controller
    {
        private readonly MongoDbService _db;

        public AdminController(MongoDbService db)
        {
            _db = db;
        }

        // ─── AUTH HELPERS ───────────────────────────────────────────────────────────

        /// <summary>
        /// Equivalent to Python's @login_required('admin') decorator
        /// </summary>
        private bool IsAdminLoggedIn() =>
            HttpContext.Session.GetString("username") != null &&
            HttpContext.Session.GetString("role") == "admin";

        private IActionResult RequireAdmin()
        {
            TempData["Error"] = "Please login as admin.";
            return RedirectToAction("Login");
        }

        // ─── LOGIN / LOGOUT ──────────────────────────────────────────────────────────

        // GET /admin_login
        // Equivalent to: @admin_blueprint.route('/admin_login', methods=["GET", "POST"])
        [HttpGet("/admin_login")]
        public IActionResult Login() => View("LoginAdmin");

        [HttpPost("/admin_login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var admin = await _db.Admins
                .Find(a => a.HospitalMail == username)
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Login");
            }

            // Equivalent to: bcrypt.check_password_hash(admin['hospital_password'], pa)
            if (!BCrypt.Net.BCrypt.Verify(password, admin.HospitalPassword))
            {
                TempData["Error"] = "Wrong Password";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("username", username);
            HttpContext.Session.SetString("role", "admin");
            HttpContext.Session.SetString("hospital_name", admin.HospitalName);

            return Redirect("/admin");
        }

        // ─── DASHBOARD ───────────────────────────────────────────────────────────────

        // GET /admin/
        // Equivalent to: @admin_blueprint.route('/admin/', methods=['GET', 'POST'])
        [HttpGet("/admin")]
        [HttpGet("/admin/")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var totalAppointment = await _db.Appointments
                .CountDocumentsAsync(a => a.HospitalName == hospitalName);

            var data = await _db.HospitalData
                .Find(h => h.HospitalName == hospitalName)
                .FirstOrDefaultAsync();

            if (data == null)
                return Redirect("/admin/add_detail");

            var totalPatient = await _db.Patients
                .CountDocumentsAsync(p => p.HospitalName == hospitalName);
            var totalDoc = await _db.Doctors
                .CountDocumentsAsync(d => d.HospitalName == hospitalName);

            ViewBag.Count = totalAppointment;
            ViewBag.GeneralTotal = data.NumberOfGeneralBeds;
            ViewBag.IcuTotal = data.NumberOfIcuBeds;
            ViewBag.VentilatorTotal = data.NumberOfVentilators;
            ViewBag.Patient = totalPatient;
            ViewBag.Doc = totalDoc;
            ViewBag.VacentGeneral = data.NumberOfGeneralBeds - data.OccupiedGeneral;
            ViewBag.VacentIcu = data.NumberOfIcuBeds - data.OccupiedIcu;
            ViewBag.VacentVentilator = data.NumberOfVentilators - data.OccupiedVentilator;
            ViewBag.HospitalName = hospitalName;
            ViewBag.Nurses = data.TotalNumberOfNurses;
            ViewBag.Staff = data.AdministrativeStaffCount;

            return View("AdminDashboard");
        }

        // ─── PATIENT MANAGEMENT ──────────────────────────────────────────────────────

        // GET & POST /add_patient
        // Equivalent to: @admin_blueprint.route('/add_patient', methods=['GET','POST'])
        [HttpGet("/add_patient")]
        public IActionResult AddPatient()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("AddPatient");
        }

        [HttpPost("/add_patient")]
        public async Task<IActionResult> AddPatient(
            string Name, string dob, string gender, string address,
            string phone, string email, string aadhaar, string bedtype, string bedno)
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";

            // Build bed number prefix just like Python code
            string bedPrefix = bedtype switch
            {
                "general" => "G",
                "icu" => "I",
                _ => "V"
            };

            var patient = new Patient
            {
                Name = Name,
                Dob = dob,
                Gender = gender,
                Address = address,
                Phone = phone,
                Email = email,
                Aadhaar = aadhaar,
                BedType = bedtype,
                BedNo = bedPrefix + bedno,
                HospitalName = hospitalName
            };

            await _db.Patients.InsertOneAsync(patient);

            // Increment occupied bed count — equivalent to $inc in Python
            var update = Builders<HospitalData>.Update.Inc(
                bedtype == "general" ? h => h.OccupiedGeneral :
                bedtype == "icu" ? h => h.OccupiedIcu : h => h.OccupiedVentilator, 1);
            await _db.HospitalData.UpdateOneAsync(h => h.HospitalName == hospitalName, update);

            return Redirect("/confirmation");
        }

        // GET /admin/patient_details
        // Equivalent to: @admin_blueprint.route('/admin/patient_details', methods=['GET', 'POST'])
        [HttpGet("/admin/patient_details")]
        public async Task<IActionResult> PatientDetails()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var patients = await _db.Patients
                .Find(p => p.HospitalName == hospitalName)
                .ToListAsync();

            return View("ManagePatient", patients);
        }

        // ─── DOCTOR MANAGEMENT ───────────────────────────────────────────────────────

        // GET & POST /add_doc
        // Equivalent to: @admin_blueprint.route('/add_doc', methods=['POST', 'GET'])
        [HttpGet("/add_doc")]
        public IActionResult AddDoctor()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("AddDoc");
        }

        [HttpPost("/add_doc")]
        public async Task<IActionResult> AddDoctor(
            string doctor_name, string specialization, string qualification,
            string email, string username, string password, string phone, string aadhaar)
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var adminEmail = HttpContext.Session.GetString("username") ?? "";
            var adminData = await _db.Admins
                .Find(a => a.HospitalMail == adminEmail)
                .FirstOrDefaultAsync();
            var hospitalName = adminData?.HospitalName ?? "";
            HttpContext.Session.SetString("hospital_name", hospitalName);

            // Check duplicate — equivalent to Python's doctors_collection.find_one check
            var existsByUsername = await _db.Doctors
                .Find(d => d.Username == username).AnyAsync();
            var existsByEmail = await _db.Doctors
                .Find(d => d.Email == email).AnyAsync();

            if (existsByUsername || existsByEmail)
                return Redirect("/add_doc");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var doctor = new Doctor
            {
                Name = doctor_name,
                Specialization = specialization,
                Qualification = qualification,
                Email = email,
                Username = username,
                Password = hashedPassword,
                Phone = phone,
                Aadhar = aadhaar,
                HospitalName = hospitalName
            };

            await _db.Doctors.InsertOneAsync(doctor);
            return Redirect("/admin");
        }

        // ─── APPOINTMENT MANAGEMENT ──────────────────────────────────────────────────

        // GET /admin/manage_appointment
        // Equivalent to: @admin_blueprint.route('/admin/manage_appointment', methods=['GET', 'POST'])
        [HttpGet("/admin/manage_appointment")]
        public async Task<IActionResult> ManageAppointments()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var appointments = await _db.Appointments
                .Find(a => a.HospitalName == hospitalName)
                .ToListAsync();

            return View("ManageAppointment", appointments);
        }

        // ─── HOSPITAL DETAILS ────────────────────────────────────────────────────────

        // GET & POST /admin/add_detail
        // Equivalent to: @admin_blueprint.route('/admin/add_detail', methods=['GET', 'POST'])
        [HttpGet("/admin/add_detail")]
        public IActionResult AddDetails()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("HospitalDetails");
        }

        [HttpPost("/admin/add_detail")]
        public async Task<IActionResult> AddDetails(
            string hospitalName, string hospitalID, string addressLine1,
            string city, string stateProvince, string postalCode,
            string contactNumber, string emergencyContactNumber, string emailAddress,
            string websiteURL, int numberOfBeds, int Beds_occupied,
            int numberOfICUBeds, int icu_occupied, int numberOfVentilators,
            int ventilator_occupied, string emergencyDepartment,
            List<string>? specialization, string hospitalOperatingHours,
            string visitingHours, string pharmacyOnSite,
            int totalNumberOfNurses, int administrativeStaffCount,
            string ambulanceServices, string bloodBank, string diagnosticServices)
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var data = new HospitalData
            {
                HospitalName = hospitalName,
                HospitalId = hospitalID,
                AddressLine1 = addressLine1,
                City = city,
                State = stateProvince,
                PostalCode = postalCode,
                ContactNumber = contactNumber,
                EmergencyContactNumber = emergencyContactNumber,
                EmailAddress = emailAddress,
                WebsiteUrl = websiteURL,
                NumberOfGeneralBeds = numberOfBeds,
                OccupiedGeneral = Beds_occupied,
                NumberOfIcuBeds = numberOfICUBeds,
                OccupiedIcu = icu_occupied,
                NumberOfVentilators = numberOfVentilators,
                OccupiedVentilator = ventilator_occupied,
                EmergencyDepartment = emergencyDepartment,
                Specialization = specialization ?? new List<string>(),
                HospitalOperatingHours = hospitalOperatingHours,
                VisitingHours = visitingHours,
                PharmacyOnSite = pharmacyOnSite,
                TotalNumberOfNurses = totalNumberOfNurses,
                AdministrativeStaffCount = administrativeStaffCount,
                AmbulanceServices = ambulanceServices,
                BloodBank = bloodBank,
                DiagnosticServices = diagnosticServices
            };

            await _db.HospitalData.InsertOneAsync(data);
            return Redirect("/admin");
        }

        // ─── SETTINGS ────────────────────────────────────────────────────────────────

        // GET & POST /admin_settings
        // Equivalent to: @admin_blueprint.route('/admin_settings', methods=['POST','GET'])
        [HttpGet("/admin_settings")]
        public async Task<IActionResult> Settings()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var data = await _db.HospitalData
                .Find(h => h.HospitalName == hospitalName)
                .FirstOrDefaultAsync();

            return View("SuperadminHospitalStatus", data);
        }

        [HttpPost("/admin_settings")]
        public async Task<IActionResult> Settings(
            string hospital_name, string address, string contact_number,
            string emergency_contact_number, string email, string website,
            int no_beds, int general_beds_occupied, string icu_beds,
            string icu_beds_occupied, string ventilators, string ventilators_occupied,
            string emergency_department, string specialization, string operating_hours,
            string visiting_hours, string pharmacy_on_site, string total_doctor,
            string total_nurses, string administrative_staff,
            string total_inventory_distributors, string ambulance_services,
            string blood_bank, string diagnostic_services)
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var currentHospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var hospitalData = await _db.HospitalData
                .Find(h => h.HospitalName == currentHospitalName)
                .FirstOrDefaultAsync();

            var update = Builders<HospitalData>.Update
                .Set(h => h.HospitalName, hospital_name)
                .Set(h => h.HospitalId, hospitalData?.HospitalId ?? "")
                .Set(h => h.AddressLine1, address)
                .Set(h => h.ContactNumber, contact_number)
                .Set(h => h.EmergencyContactNumber, emergency_contact_number)
                .Set(h => h.EmailAddress, email)
                .Set(h => h.WebsiteUrl, website)
                .Set(h => h.NumberOfGeneralBeds, no_beds)
                .Set(h => h.OccupiedGeneral, general_beds_occupied);

            await _db.HospitalData.UpdateOneAsync(
                h => h.HospitalName == currentHospitalName, update);

            return RedirectToAction("Settings");
        }

        // ─── INVENTORY ───────────────────────────────────────────────────────────────

        // GET /admin/inv_admin
        // Equivalent to: @admin_blueprint.route('/admin/inv_admin', methods=['GET', 'POST'])
        [HttpGet("/admin/inv_admin")]
        public IActionResult InvAdmin()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("InvAdmin");
        }

        // GET & POST /admin/inv_med_order
        // Equivalent to: @admin_blueprint.route('/admin/inv_med_order', methods=['GET', 'POST'])
        [HttpGet("/admin/inv_med_order")]
        public IActionResult InvMedOrder()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("InvMedOrder");
        }

        [HttpPost("/admin/inv_med_order")]
        public async Task<IActionResult> InvMedOrder(
            string medicine_name, string medicine_composition,
            int medicine_quantity, string order_comment)
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var order = new InventoryOrder
            {
                MedicineName = medicine_name,
                MedicineComposition = medicine_composition,
                MedicineQuantity = medicine_quantity,
                OrderComment = order_comment,
                HospitalName = hospitalName
            };
            await _db.Inventory.InsertOneAsync(order);
            return View("InvMedOrder");
        }

        // GET /admin/inv_order_status
        // Equivalent to: @admin_blueprint.route('/admin/inv_order_status', methods=['GET', 'POST'])
        [HttpGet("/admin/inv_order_status")]
        public async Task<IActionResult> OrderStatus()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "";
            var orders = await _db.Inventory
                .Find(o => o.HospitalName == hospitalName)
                .ToListAsync();

            return View("InvOrderStatus", orders);
        }

        // GET /admin/inv_stock_product
        // Equivalent to: @admin_blueprint.route('/admin/inv_stock_product', methods=['GET', 'POST'])
        [HttpGet("/admin/inv_stock_product")]
        public IActionResult StockDetails()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("InvStockProduct");
        }

        // ─── PATIENT DISCHARGE WITH PDF ──────────────────────────────────────────────

        // GET & POST /admin/discharge
        // Equivalent to: @admin_blueprint.route('/admin/discharge', methods=['POST', 'GET'])
        [HttpGet("/admin/discharge")]
        public IActionResult Discharge()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("PatientDischarge");
        }

        [HttpPost("/admin/discharge")]
        public async Task<IActionResult> Discharge(
            string patient_id, string patient_name, string admission_date,
            string discharge_date, string diagnosis, string treatment,
            string doctor_name, string discharge_summary, string follow_up_instructions,
            string medications, string contact_info, string gender,
            string address, string bedtype)
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var hospitalName = HttpContext.Session.GetString("hospital_name") ?? "City Hospital";

            var patientData = new DischargedPatient
            {
                PatientId = patient_id,
                PatientName = patient_name,
                AdmissionDate = admission_date,
                DischargeDate = discharge_date,
                Diagnosis = diagnosis,
                Treatment = treatment,
                DoctorName = doctor_name,
                DischargeSummary = discharge_summary,
                FollowUpInstructions = follow_up_instructions,
                Medications = medications,
                ContactInfo = contact_info,
                Gender = gender,
                Address = address,
                BedType = bedtype
            };

            // Save to MongoDB
            await _db.HospitalDischarge.InsertOneAsync(patientData);

            // Decrement occupied bed count — equivalent to Python's $inc: -1
            var bedField = bedtype == "general" ? nameof(HospitalData.OccupiedGeneral) :
                           bedtype == "icu" ? nameof(HospitalData.OccupiedIcu) :
                           nameof(HospitalData.OccupiedVentilator);

            var update = Builders<HospitalData>.Update.Inc(
                bedtype == "general" ? h => h.OccupiedGeneral :
                bedtype == "icu" ? h => h.OccupiedIcu : h => h.OccupiedVentilator, -1);
            await _db.HospitalData.UpdateOneAsync(h => h.HospitalName == hospitalName, update);

            // Generate PDF — delegate to PdfService (equivalent to Python's reportlab block)
            var pdfBytes = PdfService.GenerateDischargeSummaryPdf(patientData, hospitalName);

            return File(pdfBytes, "application/pdf", "Patient_Discharge_Summary.pdf");
        }

        // ─── CONTACT / FEEDBACK ──────────────────────────────────────────────────────

        // GET /admin/contact-us
        // Equivalent to: @admin_blueprint.route("/admin/contact-us")
        [HttpGet("/admin/contact-us")]
        public async Task<IActionResult> AdminContactUs()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();

            var contacts = await _db.Contacts.Find(_ => true).ToListAsync();
            return View("ManageAppointment", contacts);
        }

        // GET /admin_feedback
        // Equivalent to: @admin_blueprint.route('/admin_feedback', methods=['POST','GET'])
        [HttpGet("/admin_feedback")]
        [HttpPost("/admin_feedback")]
        public IActionResult AdminFeedback()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("Feedback");
        }

        // GET /admin_appointment
        // Equivalent to: @admin_blueprint.route('/admin_appointment', methods=['POST','GET'])
        [HttpGet("/admin_appointment")]
        [HttpPost("/admin_appointment")]
        public IActionResult AdminAppointment()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("AddAppointment");
        }

        // GET /admin/confirmation
        // Equivalent to: @admin_blueprint.route('/admin/confirmation')
        [HttpGet("/admin/confirmation")]
        public IActionResult AdminConfirmation()
        {
            if (!IsAdminLoggedIn()) return RequireAdmin();
            return View("SuccessAdmin");
        }
    }
}
