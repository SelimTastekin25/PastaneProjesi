using System.ComponentModel.DataAnnotations;

namespace PastaneProjesi.Models
{
    public class ProfileViewModel
    {
        public string? FullName { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        public string? UserName { get; set; }

        // Password Change
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
