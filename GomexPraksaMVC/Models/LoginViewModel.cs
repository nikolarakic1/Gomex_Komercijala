using System.ComponentModel.DataAnnotations;

namespace GomexPraksaMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [Display(Name = "Korisničko ime")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Zapamti me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}