using System.ComponentModel.DataAnnotations;

namespace CleanValveManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Employee ID")]
        public int EmployeeId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}