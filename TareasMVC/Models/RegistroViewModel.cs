using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Models
{
    public class RegistroViewModel
    {
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //[EmailAddress(ErrorMessage = "El campo debe ser un correo electrónico válido")]
        [Required(ErrorMessage = "Error.Requerido")]
        [EmailAddress(ErrorMessage = "Error.Email")]
        public string Email { get; set; }

        //[Required(ErrorMessage = "El campo {0} es requerido")]
        [Required(ErrorMessage = "Error.Requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
