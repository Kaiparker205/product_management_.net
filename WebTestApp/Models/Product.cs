using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTestApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        //[ValidateNever]
        public string? imagePath { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Please upload a product image.")]
        //[ValidateNever]
        public IFormFile clientFile { get; set; }

        [ValidateNever]

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [ValidateNever]

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [ValidateNever]

        public DateTime? DeletedAt { get; set; } = null;
    }
}
