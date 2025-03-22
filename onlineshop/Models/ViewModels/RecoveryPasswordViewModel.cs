using System.ComponentModel.DataAnnotations;

namespace onlineshop.Models.ViewModels
{
    public class RecoveryPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
