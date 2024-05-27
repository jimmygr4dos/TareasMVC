using Microsoft.EntityFrameworkCore;

namespace TareasMVC.Entities
{
    public class ArchivoAdjunto
    {
        public Guid Id { get; set; }

        [Unicode]
        public string Url { get; set; }
        
        public string Titulo { get; set; }
        
        public int Orden { get; set; }

        public DateTime FechaCreacion { get; set; }


        public int TareaId { get; set; } //La convención para el FK es [NombreClase]Id

        //Propiedad de Navegación
        public Tarea Tarea { get; set; } //Con esta propiedad obtenemos la data de la clase relacionada
    }
}
