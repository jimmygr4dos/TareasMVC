using System.Security.Claims;

namespace TareasMVC.Services
{
    public interface IServicioUsuarios
    {
        string ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private HttpContext _context;
        public ServicioUsuarios(IHttpContextAccessor httpContextAccesor)
        {
            _context = httpContextAccesor.HttpContext;
        }
        public string ObtenerUsuarioId()
        {
            if (_context.User.Identity.IsAuthenticated)
            {
                var idClaim = _context.User.Claims
                                           .Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                return idClaim.Value;
            } 
            else
            {
                throw new Exception("El usuario no está autenticado");
            }
        }
    }
}
