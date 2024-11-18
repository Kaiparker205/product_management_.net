using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTestApp.Models;
using WebTestApp.Repository.Base;

namespace WebTestApp.Controllers
{
    [Authorize]

    public class ProductsController : Controller
    {
        private readonly IUnitOfWork myUnit;
        private readonly IWebHostEnvironment _host;

        public ProductsController(IUnitOfWork _myUnit, IWebHostEnvironment host)
        {
            myUnit = _myUnit;
            _host = host;
        }

        // GET: Products
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var products = await myUnit.products.FindAllAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await myUnit.products.FindByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Category,Price,clientFile")] Product product, IFormFile clientFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (clientFile != null)
                    {
                        product.imagePath = UploadFile(clientFile);
                    }
                    await myUnit.products.AddOneAsync(product);
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to upload image. " + ex.Message);
                }
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await myUnit.products.FindByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Category,Price,clientFile")] Product product, IFormFile clientFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (clientFile != null)
                    {
                        product.imagePath = UploadFile(clientFile);
                    }
                    product.UpdatedAt = DateTime.Now;
                    await myUnit.products.UpdateOneAsync(product);
                    TempData["SuccessMessage"] = $"{product.Name} updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // When ModelState is not valid, redirect to the Details view of the product.
            TempData["ErrorMessage"] = "Please correct the errors below before resubmitting.";
            return RedirectToAction("Details", new { id = product.Id });
        }


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await myUnit.products.FindByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await myUnit.products.FindByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.imagePath))
            {
                var filePath = Path.Combine(_host.WebRootPath, product.imagePath.TrimStart('/'));
                DeleteFile(filePath);
            }

            await myUnit.products.DeleteOneAsync(product);
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return myUnit.products.Exists(id);
        }

        private string UploadFile(IFormFile clientFile)
        {
            if (clientFile == null)
            {
                throw new ArgumentNullException(nameof(clientFile), "Client file cannot be null");
            }

            string fileName = Path.GetFileNameWithoutExtension(clientFile.FileName);
            string extension = Path.GetExtension(clientFile.FileName);
            string myUpload = Path.Combine(_host.WebRootPath, "images");

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

            return $"/images/{fileName}_{now}{extension}";
        }

        private void DeleteFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (IOException ioExp)
                {
                    Console.WriteLine($"Error deleting file: {ioExp.Message}");
                }
            }
        }

        // Action to load dynamic table based on type
        //public async Task<IActionResult> LoadDynamicTable(string type)
        //{
        //    dynamic data = null;

        //    if (type == "products")
        //    {
        //        data = await myUnit.products.FindAllAsync();
        //    }
        //    else if (type == "employers")
        //    {
        //        data = await myUnit.employers.FindAllAsync();
        //    }
        //    //else if (type == "orders")
        //    //{
        //    //    data = await myUnit.orders.Include(o => o.Employer).Include(o => o.Product).ToListAsync();
        //    //}

        //    // Specify the full path to _DynamicTable in the Shared folder
        //    return PartialView("~/Views/Shared/_DynamicTable.cshtml", data);
        //}

    }
}