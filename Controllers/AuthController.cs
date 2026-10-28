using Microsoft.AspNetCore.Mvc;

namespace Aashirwad.Controllers
{
    /// <summary>
    /// Auth Controller - equivalent to Python's modules/log_out.py
    /// Handles all logout routes for user, admin, superadmin, doctor
    /// </summary>
    public class AuthController : Controller
    {
        // GET /user_logout
        // Equivalent to: @logout_bp.route('/user_logout')
        [HttpGet("/user_logout")]
        public IActionResult UserLogout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have logged out";
            return Redirect("/");
        }

        // GET /admin_logout
        // Equivalent to: @logout_bp.route('/admin_logout')
        [HttpGet("/admin_logout")]
        public IActionResult AdminLogout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have logged out";
            return Redirect("/");
        }

        // GET /superadmin_logout
        // Equivalent to: @logout_bp.route('/superadmin_logout')
        [HttpGet("/superadmin_logout")]
        public IActionResult SuperAdminLogout()
        {
            HttpContext.Session.Clear();
            return Redirect("/superadmin");
        }

        // GET /doctor_logout
        // Equivalent to: @logout_bp.route('/doctor_logout')
        [HttpGet("/doctor_logout")]
        public IActionResult DoctorLogout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have logged out";
            return Redirect("/");
        }
    }
}
