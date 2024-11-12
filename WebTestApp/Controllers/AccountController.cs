using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebTestApp.Bl;
using WebTestApp.Models;

namespace WebTestApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _host;
        private readonly UserManager<Employer> _employerManager;
        private readonly SignInManager<Employer> _employerSignInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<Employer> employerManager,SignInManager<Employer> employerSignInManager,RoleManager<IdentityRole> roleManager,IWebHostEnvironment host)
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

            // Create the employer user and set properties from the registration form
            var employer = new Employer
            {
                UserName = model.UserName,
                Email = model.Email,
                Address = model.Address,      
                Birthday = model.Birthday    ,
                Profile=model.Profile
                ,
                
            };

            // Handle file upload if clientFile is provided
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

            // Handle registration errors by adding them to ModelState
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
            string extension = Path.GetExtension(ClientFile.FileName);
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
        public IActionResult EmployerLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployerLogin(LoginViewModel model, string? returnUrl = null)
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

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // Login for Admin
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> AdminLogin(LoginViewModel model, string? returnUrl = null)
        //{
        //    returnUrl ??= Url.Content("~/");

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var result = await _adminSignInManager.PasswordSignInAsync(
        //        model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        //    if (result.Succeeded)
        //    {
        //        return RedirectToLocal(returnUrl);
        //    }

        //    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //    return View(model);
        //}

        private IActionResult RedirectToLocal(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }
    }
}
