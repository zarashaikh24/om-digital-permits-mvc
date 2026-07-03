using CleanValveManagement.ViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using MimeKit;

namespace CleanValveManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
public IActionResult Login()
{
    return View();
}
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("SMAConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ValidateLogin", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Emp_ID", SqlDbType.Int).Value = model.EmployeeId;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = model.Password.Trim();

                    con.Open();

                    int empId = 0;
                    string userType = "";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string status = reader["Status"].ToString().Trim();

                            if (status.Equals("Valid", StringComparison.OrdinalIgnoreCase))
                            {
                                empId = Convert.ToInt32(model.EmployeeId);
                                userType = reader["User_type"].ToString().Trim();
                            }
                        }
                    }

                    if (empId != 0)
                    {
                        HttpContext.Session.SetInt32("Emp_ID", empId);
                        HttpContext.Session.SetString("UserType", userType);
                        HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("hh:mm tt"));

                        using (SqlCommand empCmd = new SqlCommand(
                            "SELECT Name, Designation FROM Login_VC WHERE Emp_ID = @Emp_ID", con))
                        {
                            empCmd.Parameters.Add("@Emp_ID", SqlDbType.Int).Value = empId;

                            using (SqlDataReader empReader = empCmd.ExecuteReader())
                            {
                                if (empReader.Read())
                                {
                                    HttpContext.Session.SetString("EmpName", empReader["Name"].ToString());
                                    HttpContext.Session.SetString("Designation", empReader["Designation"].ToString());
                                }
                            }
                        }

                        return RedirectToAction("Dashboard", "Permit");
                    }
                }

                ModelState.AddModelError("", "Invalid Employee ID or Password.");
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string employeeId, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter your registered email.";
                return View();
            }

            var user = await _userManager.FindByNameAsync(employeeId);

            if (user == null || string.IsNullOrWhiteSpace(user.Email) ||
    !user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Invalid Employee ID or Email.";
                return View();
            }

            string otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("ResetEmail", email);
            HttpContext.Session.SetString("ResetOtp", otp);
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString());

            await SendOtpEmail(email, otp);

            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            string? savedOtp = HttpContext.Session.GetString("ResetOtp");
            string? otpTime = HttpContext.Session.GetString("OtpTime");

            if (savedOtp == null || otpTime == null)
            {
                ViewBag.Error = "OTP expired. Please try again.";
                return View();
            }

            DateTime generatedTime = DateTime.Parse(otpTime);

            if (DateTime.Now > generatedTime.AddMinutes(5))
            {
                ViewBag.Error = "OTP expired.";
                return View();
            }

            if (otp != savedOtp)
            {
                ViewBag.Error = "Invalid OTP.";
                return View();
            }

            HttpContext.Session.SetString("OtpVerified", "true");

            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetString("OtpVerified") != "true")
            {
                return RedirectToAction("ForgotPassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Password and Confirm Password do not match.";
                return View();
            }

            string? email = HttpContext.Session.GetString("ResetEmail");

            if (email == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                HttpContext.Session.Clear();
                TempData["Success"] = "Password reset successfully. Please login.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Error = "Password reset failed. Please follow password rules.";
            return View();
        }

        private async Task SendOtpEmail(string toEmail, string otp)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                "O&M Digital Permits",
                _configuration["SmtpSettings:Email"]));

            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Password Reset OTP";

            message.Body = new TextPart("plain")
            {
                Text = $"Your OTP for password reset is: {otp}\n\nThis OTP is valid for 5 minutes."
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _configuration["SmtpSettings:Host"],
                int.Parse(_configuration["SmtpSettings:Port"]!),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _configuration["SmtpSettings:Email"],
                _configuration["SmtpSettings:Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}