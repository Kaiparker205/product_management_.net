using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTestApp.Models
{
    public class Employer : IdentityUser
    {
       


       

        [Required]
        public string Address { get; set; }

        [Required]
        public string Profile { get; set; }
      
        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        [ValidateNever]
        public string Avatar { get; set; }

        [NotMapped]
        [ValidateNever]
        public IFormFile ClientFile { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ValidateNever]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ValidateNever]
        public DateTime? DeletedAt { get; set; } = null;
    }
}
