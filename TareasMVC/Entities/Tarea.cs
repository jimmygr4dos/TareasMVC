using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Entities
{
    public class Tarea
    {
        public int Id { get; set; }

        [StringLength(250)]
        [Required]
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public int Orden { get; set; }

        public DateTime FechaCreacion { get; set; }


        public string UsuarioCreacionId { get; set; }

        //Propiedad de Navegación
        public IdentityUser UsuarioCreacion { get; set; }


        //Propiedad de Navegación
        public List<Paso> Pasos { get; set; } //Con esta propiedad obtenemos la data de la clase relacionada

        //Propiedad de Navegación
        public List<ArchivoAdjunto> ArchivosAdjuntos { get; set; } //Con esta propiedad obtenemos la data de la clase relacionada
    }
}
