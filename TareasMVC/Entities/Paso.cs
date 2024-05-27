namespace TareasMVC.Entities
{
    public class Paso
    {
        public Guid Id { get; set; }
        
        public string Descripcion { get; set; }
        
        public bool Realizado { get; set; }
        
        public int Orden { get; set; }

        public int TareaId { get; set; } //La convención para el FK es [NombreClase]Id

        //Propiedad de Navegación
        public Tarea Tarea { get; set; } //Con esta propiedad obtenemos la data de la clase relacionada
    }
}
