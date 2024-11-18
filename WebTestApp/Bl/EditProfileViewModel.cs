using System.ComponentModel.DataAnnotations;

namespace WebTestApp.Bl
{
    public class EditProfileViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Birthday")]
        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }

        public string? AvatarPath { get; set; } 
        public IFormFile? ProfileImage { get; set; } 
    }
}
