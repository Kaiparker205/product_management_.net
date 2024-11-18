using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTestApp.Repository.Base;
using EmployerModel = WebTestApp.Models.Employer;


namespace WebTestApp.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize]

    public class EmployersController : Controller
    {
        private readonly IUnitOfWork myUnit;
        private readonly IWebHostEnvironment _host;

        public EmployersController(IUnitOfWork myUnit, IWebHostEnvironment host)
        {
            _host = host;
            this.myUnit = myUnit;
        }

        // GET: EmployersController/Employers
        public async Task<IActionResult> Index()
        {
            var employers = await myUnit.employers.FindAllAsync();
            return View(employers);
        }

        // GET: EmployersController/Employers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Employer ID cannot be null.";
                return NotFound();
            }

            var employer = await myUnit.employers.FindByIdAsync(id);
            if (employer == null)
            {
                TempData["ErrorMessage"] = "Employer not found.";
                return NotFound();
            }

            return View(employer);
        }

        // GET: EmployersController/Employers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmployersController/Employers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,UserName,Email,PhoneNumber,Address,Profile,Birthday,ClientFile")] EmployerModel  employer, IFormFile ClientFile)
        {
            if (ModelState.IsValid)
            {
                if (ClientFile != null)
                {
                    employer.Avatar = UploadFile(ClientFile);
                }
                employer.CreatedDate = DateTime.Now;
                myUnit.employers.AddOne(employer);
                TempData["SuccessMessage"] = "Employer created successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to create employer. Please check the form for errors.";
            return View(employer);
        }

        // GET: EmployersController/Employers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Employer ID cannot be null.";
                return NotFound();
            }

            var employer = await myUnit.employers.FindByIdAsync(id);
            if (employer == null)
            {
                TempData["ErrorMessage"] = "Employer not found.";
                return NotFound();
            }

            return View(employer);
        }

        // POST: EmployersController/Employers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, [Bind("Id,UserName,Email,PhoneNumber,Address,Profile,Birthday,ClientFile")] EmployerModel employer, IFormFile clientFile)
        {
            if (id != employer.Id)
            {
                TempData["ErrorMessage"] = "Employer ID mismatch.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (clientFile != null)
                    {
                        employer.Avatar = UploadFile(clientFile);
                    }
                    employer.UpdatedAt = DateTime.Now;
                    myUnit.employers.UpdateOne(employer);
                    TempData["SuccessMessage"] = "Employer updated successfully.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!myUnit.employers.FindAll().Any(e => e.Id == id))
                    {
                        TempData["ErrorMessage"] = "Employer not found.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"An error occurred while updating the employer: {ex.Message}";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to update employer. Please check the form for errors.";
            return View(employer);
        }

        // POST: EmployersController/Employers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var employer = await myUnit.employers.FindByIdAsync(id);
            if (employer != null)
            {
                myUnit.employers.DeleteOne(employer);
                TempData["SuccessMessage"] = "Employer deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Employer not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        private string UploadFile(IFormFile clientFile)
        {
            if (clientFile == null)
            {
                throw new ArgumentNullException(nameof(clientFile), "Client file cannot be null");
            }

            string fileName = Path.GetFileNameWithoutExtension(clientFile.FileName);
            string extension = Path.GetExtension(clientFile.FileName);
            string myUpload = Path.Combine(_host.WebRootPath, "avatars");

            if (!Directory.Exists(myUpload))
            {
                Directory.CreateDirectory(myUpload);
            }

            string now = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fullPath = Path.Combine(myUpload, $"{fileName}_{now}{extension}");
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                clientFile.CopyTo(fileStream);
            }

            return $"/avatars/{fileName}_{now}{extension}";
        }
    }
}
