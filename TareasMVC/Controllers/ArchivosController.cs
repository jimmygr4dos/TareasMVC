using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entities;
using TareasMVC.Services;

namespace TareasMVC.Controllers
{
    [Route("api/archivos")]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly string _contenedor = "archivosadjuntos";

        public ArchivosController(ApplicationDBContext context,
                                  IServicioUsuarios servicioUsuarios,
                                  IAlmacenadorArchivos almacenadorArchivos)
        {
            _context = context;
            _servicioUsuarios = servicioUsuarios;
            _almacenadorArchivos = almacenadorArchivos;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<IEnumerable<ArchivoAdjunto>>> Post(int tareaId,
                                                                          [FromForm] IEnumerable<IFormFile> archivos)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea is null)
            {
                return NotFound();
            }

            if (tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var existenArchivosAdjuntos = await _context.ArchivosAdjuntos.AnyAsync(a => a.TareaId == tareaId);

            var ordenMayor = 0;
            if (existenArchivosAdjuntos)
            {
                ordenMayor = await _context.ArchivosAdjuntos.Where(a => a.TareaId == tareaId)
                                                            .Select(a => a.Orden).MaxAsync();
            }

            var resultados = await _almacenadorArchivos.Almacenar(_contenedor, archivos);

            var archivosAdjuntos = resultados.Select((resultado, indice) => new ArchivoAdjunto
            {
                TareaId = tareaId,
                FechaCreacion = DateTime.UtcNow,
                Url = resultado.URL,
                Titulo = resultado.Titulo,
                Orden = ordenMayor + indice + 1
            }).ToList();

            _context.AddRange(archivosAdjuntos);
            await _context.SaveChangesAsync();

            return archivosAdjuntos.ToList();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] string titulo)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var archivoAdjunto = await _context.ArchivosAdjuntos
                                                .Include(a => a.Tarea)
                                                .FirstOrDefaultAsync(a => a.Id == id);

            if (archivoAdjunto is null)
            {
                return NotFound();
            }

            if (archivoAdjunto.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            archivoAdjunto.Titulo = titulo;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var archivoAdjunto = await _context.ArchivosAdjuntos
                                                .Include(a => a.Tarea)
                                                .FirstOrDefaultAsync(a => a.Id == id);

            if (archivoAdjunto is null)
            {
                return NotFound();
            }

            if (archivoAdjunto.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            _context.Remove(archivoAdjunto);
            await _context.SaveChangesAsync();

            await _almacenadorArchivos.Borrar(archivoAdjunto.Url, _contenedor);

            return Ok();
        }

        [HttpPost("ordenar/{tareaId:int}")]
        public async Task<IActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId &&
                                                                  t.UsuarioCreacionId == usuarioId);
            if (tarea is null)
            {
                return NotFound();
            }

            var archivosAdjuntos = await _context.ArchivosAdjuntos.Where(a => a.TareaId == tareaId).ToListAsync();

            var archivosIds = archivosAdjuntos.Select(a => a.Id);

            var idsArchivosNoPertenecenALaTarea = ids.Except(archivosIds).ToList();

            if(idsArchivosNoPertenecenALaTarea.Any())
            {
                return BadRequest("No todos los archivos están presentes");
            }

            var archivosAdjuntosDiccionario = archivosAdjuntos.ToDictionary(a => a.Id);
            for (int i = 0; i < ids.Length; i++)
            {
                var archivoAdjuntoId = ids[i];
                var archivoAdjunto = archivosAdjuntosDiccionario[archivoAdjuntoId];
                archivoAdjunto.Orden = i + 1;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
