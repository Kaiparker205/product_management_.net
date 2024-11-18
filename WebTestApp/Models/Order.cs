using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTestApp.Models
{
    public enum OrderStatus
{
    Pending,  
    Accepted,   
    InProgress, 
    Delivered  
}

    public class Order
    {
        // Composite primary key will be defined in DbContext
        [ForeignKey("Employer")]
        public string EmployerId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public double Price { get; set; }

        public int Quantity { get; set; }

        [ValidateNever]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ValidateNever]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ValidateNever]
        public DateTime? DeletedAt { get; set; } = null;

        [ValidateNever]
        public Employer Employer { get; set; }

        [ValidateNever]
        public Product Product { get; set; }

        // Enum for Order Status
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // Default is "Pending"

        // Nullable property for delivery date
        [ValidateNever]
        public DateTime? DateLivraison { get; set; } = null; // Default is null
    }
}
