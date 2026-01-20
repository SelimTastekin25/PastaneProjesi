using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaneProjesi.Models;
using System.Security.Claims;
using BCrypt.Net;

namespace PastaneProjesi.Controllers
{
    public class AccountController : Controller
    {
        private readonly PastaneContext _context;

        public AccountController(PastaneContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Product");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim("FullName", user.FullName ?? "")
                };

                if (user.IsAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redirect admin to dashboard, regular users to homepage
                if (user.IsAdmin)
                {
                    return RedirectToAction("Dashboard", "Product");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Hatalı kullanıcı adı veya şifre!";
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string fullName)
        {
            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Şifreler eşleşmiyor!";
                return View();
            }

            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.UserName == username))
            {
                ViewBag.ErrorMessage = "Bu kullanıcı adı zaten kullanılıyor!";
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.ErrorMessage = "Bu e-posta adresi zaten kayıtlı!";
                return View();
            }

            // Create new user
            var user = new User
            {
                UserName = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName,
                CreatedAt = DateTime.Now,
                IsAdmin = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kayıt işlemi başarılı! Lütfen giriş yapınız.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Update basic info
                bool infoChanged = false;
                if (user.FullName != model.FullName)
                {
                    user.FullName = model.FullName ?? user.FullName;
                    infoChanged = true;
                }

                // Password change logic
                if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
                {
                    if (BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        TempData["SuccessMessage"] = "Profiliniz ve şifreniz başarıyla güncellendi.";
                    }
                    else
                    {
                        ModelState.AddModelError("CurrentPassword", "Mevcut şifre hatalı.");
                        return View(model);
                    }
                }
                else if (infoChanged)
                {
                    TempData["SuccessMessage"] = "Profil bilgileriniz güncellendi.";
                }

                await _context.SaveChangesAsync();
                return View(model);
            }

            return View(model);
        }

        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    // Generate Token
                    var token = Guid.NewGuid().ToString();
                    user.PasswordResetToken = token;
                    user.PasswordResetTokenExpires = DateTime.Now.AddHours(24);
                    await _context.SaveChangesAsync();

                    // SIMULATION: Since we don't have email sender, we will show the link in tempdata
                    var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = model.Email }, Request.Scheme);
                    TempData["ResetLink"] = resetLink;
                    TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı oluşturuldu. (Email simülasyonu)";
                    return View(model);
                }
                
                // Don't reveal that user does not exist
                TempData["SuccessMessage"] = "Eğer kayıtlı bir e-posta adresi girdiyseniz, şifre sıfırlama bağlantısı gönderilmiştir.";
                return View(model);
            }
            return View(model);
        }

        // GET: /Account/ResetPassword
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null && user.PasswordResetToken == model.Token && user.PasswordResetTokenExpires > DateTime.Now)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpires = null;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ViewBag.ErrorMessage = "Geçersiz veya süresi dolmuş bağlantı.";
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
