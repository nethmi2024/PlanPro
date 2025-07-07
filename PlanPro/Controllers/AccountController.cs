using PlanPro.Helpers;
using PlanPro.Helpers;
using PlanPro.Models;
using PlanPro.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Security;


namespace PlanPro.Controllers
{
    public class AccountController : Controller
    {
        private Entities db = new Entities();

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Email already exists.";
                    return View(model);
                }

                var newUser = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = PasswordHelper.HashPassword(model.Password)
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful. Please login.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, bool rememberMe = false)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Please register first.";
                return View();
            }

            string hashedInputPassword = PasswordHelper.HashPassword(password);

            if (user.Password != hashedInputPassword)
            {
                TempData["ErrorMessage"] = "Incorrect password.";
                return View();
            }

            FormsAuthentication.SetAuthCookie(email, rememberMe);

            Session["UserId"] = user.Id;
            Session["FullName"] = user.FullName;

            return RedirectToAction("Index", "Home");
        }

        // GET: ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                string token = Guid.NewGuid().ToString();
                user.ResetToken = token;
                user.TokenExpiry = DateTime.Now.AddMinutes(15);
                db.SaveChanges();

                string link = Url.Action("ResetPassword", "Account", new { token = token }, Request.Url.Scheme);
                TempData["ResetLink"] = link;
            }
            else
            {
                TempData["ResetLink"] = "If the email exists, a link would appear here.";
            }

            return View();
        }

        // GET: ResetPassword
        public ActionResult ResetPassword(string token)
        {
            var user = db.Users.FirstOrDefault(u => u.ResetToken == token && u.TokenExpiry > DateTime.Now);
            if (user == null)
            {
                return Content("Invalid or expired token.");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // POST: ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.FirstOrDefault(u => u.ResetToken == model.Token && u.TokenExpiry > DateTime.Now);
            if (user == null)
            {
                return Content("Invalid or expired token.");
            }

            user.Password = PasswordHelper.HashPassword(model.NewPassword);
            user.ResetToken = null;
            user.TokenExpiry = null;

            db.SaveChanges();

            TempData["SuccessMessage"] = "Password reset successful. Please login.";
            return RedirectToAction("Login");
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

    }
}