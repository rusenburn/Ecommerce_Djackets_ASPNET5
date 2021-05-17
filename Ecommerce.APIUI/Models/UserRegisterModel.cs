using System.ComponentModel.DataAnnotations;

namespace Ecommerce.APIUI.Models
{
    public class UserRegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}