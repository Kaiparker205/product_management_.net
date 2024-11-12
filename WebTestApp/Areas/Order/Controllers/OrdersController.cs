using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebTestApp.Bl;
using WebTestApp.Repository.Base;
using OrderModel = WebTestApp.Models.Order;

namespace WebTestApp.Areas.Order.Controllers
{
    [Area("Order")]
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork myUnit;
        private readonly IConverter _converter;
        private readonly ICompositeViewEngine _viewEngine;

        public OrdersController(IUnitOfWork myUnit, IConverter converter, ICompositeViewEngine viewEngine)
        {
            this.myUnit = myUnit;
            _converter = converter;
            _viewEngine = viewEngine;
        }

        // GET: Order/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await myUnit.orders.FindAllAsync("Employer", "Product");
            return View(orders);
        }

        // GET: Order/Orders/Create
        public IActionResult Create()
        {
            ViewData["EmployerId"] = new SelectList(myUnit.employers.FindAll(), "Id", "Address");
            ViewData["ProductId"] = new SelectList(myUnit.products.FindAll(), "Id", "Category");
            return View();
        }

        // POST: Order/Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployerId,ProductId,Price,Quantity")] OrderModel order)
        {
            if (ModelState.IsValid)
            {
                order.Employer = await myUnit.employers.FindByIdAsync(order.EmployerId);
                order.Product = await myUnit.products.FindByIdAsync(order.ProductId);

                if (order.Employer == null || order.Product == null)
                {
                    ModelState.AddModelError("", "Invalid Employer or Product");
                    TempData["ErrorMessage"] = "Failed to create order: Invalid Employer or Product.";
                    ViewData["EmployerId"] = new SelectList(myUnit.employers.FindAll(), "Id", "Address", order.EmployerId);
                    ViewData["ProductId"] = new SelectList(myUnit.products.FindAll(), "Id", "Category", order.ProductId);
                    return View(order);
                }

                // Check for duplicate order
                var existingOrder = (await myUnit.orders.FindAsync(e => e.EmployerId == order.EmployerId && e.ProductId == order.ProductId)).FirstOrDefault();
                if (existingOrder != null)
                {
                    ModelState.AddModelError("", "An order with this Employer and Product already exists.");
                    TempData["ErrorMessage"] = "Failed to create order: Duplicate order.";
                    ViewData["EmployerId"] = new SelectList(myUnit.employers.FindAll(), "Id", "Address", order.EmployerId);
                    ViewData["ProductId"] = new SelectList(myUnit.products.FindAll(), "Id", "Category", order.ProductId);
                    return View(order);
                }

                order.CreatedDate = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                await myUnit.orders.AddOneAsync(order);
                TempData["SuccessMessage"] = "Order created successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to create order. Please check the form for errors.";
            ViewData["EmployerId"] = new SelectList(myUnit.employers.FindAll(), "Id", "Address", order.EmployerId);
            ViewData["ProductId"] = new SelectList(myUnit.products.FindAll(), "Id", "Category", order.ProductId);
            return View(order);
        }

        // GET: Order/Orders/Edit/5
        public async Task<IActionResult> Edit(string? idEmp, int? idPr)
        {
            if (idEmp == null || idPr == null)
            {
                return NotFound();
            }

            var order = (await myUnit.orders.FindAsync(e => e.EmployerId == idEmp && e.ProductId == idPr)).FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }

            ViewData["EmployerId"] = new SelectList(myUnit.employers.FindAll(), "Id", "Address", order.EmployerId);
            ViewData["ProductId"] = new SelectList(myUnit.products.FindAll(), "Id", "Category", order.ProductId);
            return View(order);
        }

        // POST: Order/Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("EmployerId,ProductId,Price,Quantity,CreatedDate,UpdatedAt,DeletedAt")] OrderModel order)
        {
            if (order.EmployerId == null || order.ProductId == null)
            {
                TempData["ErrorMessage"] = "Invalid order ID.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingOrder = (await myUnit.orders.FindAsync(e => e.EmployerId == order.EmployerId && e.ProductId == order.ProductId)).FirstOrDefault();
                if (existingOrder == null)
                {
                    TempData["ErrorMessage"] = "Order does not exist.";
                    return NotFound();
                }

                existingOrder.Price = order.Price;
                existingOrder.Quantity = order.Quantity;
                existingOrder.UpdatedAt = DateTime.Now;

                await myUnit.orders.UpdateOneAsync(existingOrder);
                TempData["SuccessMessage"] = "Order updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["EmployerId"] = new SelectList(myUnit.employers.FindAll(), "Id", "Address", order.EmployerId);
            ViewData["ProductId"] = new SelectList(myUnit.products.FindAll(), "Id", "Category", order.ProductId);
            return View(order);
        }

        // POST: Order/Orders/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int idPr, string idEmp)
        {
            var order = (await myUnit.orders.FindAsync(e => e.EmployerId == idEmp && e.ProductId == idPr)).FirstOrDefault();

            if (order != null)
            {
                await myUnit.orders.DeleteOneAsync(order);
                TempData["SuccessMessage"] = "Order deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete order: Order not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        // PDF Generation
        [HttpPost]
        public IActionResult Pdf(string serializedItem)
        {
            try
            {
                var order = JsonConvert.DeserializeObject<OrderModel>(serializedItem);
                return View(order);
            }
            catch (JsonException ex)
            {
                TempData["ErrorMessage"] = $"Failed to generate PDF: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> OrderExists(int id)
        {
            return await myUnit.orders.FindByIdAsync(id) != null;
        }

        // Statistics Method
        public async Task<IActionResult> Statistics(DateTime? startDate, DateTime? endDate)
        {
            // Get an IQueryable collection of orders
            var ordersQuery = myUnit.orders.Find().AsQueryable();

            // Filter orders by start and end dates if provided
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate <= endDate.Value);
            }

            // Group, select, and order data asynchronously
            var orders = await ordersQuery
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(o => (decimal)(o.Price * o.Quantity)),
                    TotalQuantity = g.Sum(o => o.Quantity)
                })
                .OrderBy(stat => stat.Date)
                .ToListAsync();

            // Prepare view model for statistics
            var viewModel = new StatisticsViewModel
            {
                Labels = orders.Select(o => o.Date.ToString("yyyy-MM-dd")).ToList(),
                TotalRevenue = orders.Select(o => o.TotalRevenue).ToList(),
                TotalQuantity = orders.Select(o => o.TotalQuantity).ToList(),
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MaxValue
            };

            return View(viewModel);
        }


    }
}
