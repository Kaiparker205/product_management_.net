using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebTestApp.Bl;
using WebTestApp.Models;
using System.IO;

namespace WebTestApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _host;
        private readonly UserManager<Employer> _employerManager;
        private readonly SignInManager<Employer> _employerSignInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<Employer> employerManager, SignInManager<Employer> employerSignInManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment host)
        {
            _employerManager = employerManager;
            _employerSignInManager = employerSignInManager;
            _roleManager = roleManager;
            _host = host;
        }

        // Register Employer
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Employer model, IFormFile? ClientFile, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employer = new Employer
            {
                UserName = model.UserName,
                Email = model.Email,
                Address = model.Address,
                Birthday = model.Birthday,
                Profile = model.Profile,
            };

            if (ClientFile != null)
            {
                employer.Avatar = UploadFile(ClientFile);
            }

            var result = await _employerManager.CreateAsync(employer, model.PasswordHash);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Employer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Employer"));
                }

                await _employerManager.AddToRoleAsync(employer, "Employer");
                await _employerSignInManager.SignInAsync(employer, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Edit Profile
        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = _employerManager.GetUserId(User);
            var user = _employerManager.FindByIdAsync(userId).Result;
            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Address = user.Address,
                Birthday = user.Birthday,
                AvatarPath = user.Avatar
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model, IFormFile? ClientFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _employerManager.GetUserId(User);
            var user = await _employerManager.FindByIdAsync(userId);

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Address = model.Address;
            user.Birthday = model.Birthday ?? DateTime.MinValue;

            if (ClientFile != null)
            {
                user.Avatar = UploadFile(ClientFile);
            }

            var result = await _employerManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Method to handle file upload and return the file path
        private string UploadFile(IFormFile ClientFile)
        {
            if (ClientFile == null)
            {
                throw new ArgumentNullException(nameof(ClientFile), "Client file cannot be null");
            }

            string fileName = Path.GetFileNameWithoutExtension(ClientFile.FileName);
            string extension = Path.GetExtension(ClientFile.FileName).ToLower();

            if (!extension.Equals(".jpg") && !extension.Equals(".jpeg") && !extension.Equals(".png"))
            {
                throw new InvalidDataException("Only .jpg, .jpeg, .png files are allowed.");
            }

            string myUpload = Path.Combine(_host.WebRootPath, "avatars");

            if (!Directory.Exists(myUpload))
            {
                Directory.CreateDirectory(myUpload);
            }

            string now = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fullPath = Path.Combine(myUpload, $"{fileName}_{now}{extension}");

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                ClientFile.CopyTo(fileStream);
            }

            return $"/avatars/{fileName}_{now}{extension}";
        }

        // Login for Employer
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _employerSignInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            var user = await _employerManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User with this email does not exist.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid password.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _employerSignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //// Change Password
        //[HttpGet]
        //public IActionResult ChangePassword()
        //{
        //    return View();
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _employerManager.GetUserId(User);
            var user = await _employerManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login");
            }

            var result = await _employerManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _employerSignInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Profile");
            }

            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction("Profile");
        }


        // View Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _employerManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Address = user.Address,
                Birthday = user.Birthday,
                AvatarPath = user.Avatar
            };

            return View(model);
        }

        // Delete Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile()
        {
            var user = await _employerManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _employerManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _employerSignInManager.SignOutAsync();
                return RedirectToAction("Index", "Home"); // Redirect to home page after deletion
            }

            // Handle errors if deletion fails
            ModelState.AddModelError(string.Empty, "Failed to delete the profile. Try again later.");
            return RedirectToAction("Profil");
        }

        // Redirect to local URL or home if invalid
        private IActionResult RedirectToLocal(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }
    }
}
