using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entities;
using TareasMVC.Models;
using TareasMVC.Services;

namespace TareasMVC.Controllers
{
    [Route("api/pasos")]
    public class PasosController: ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IServicioUsuarios _servicioUsuarios;

        public PasosController(ApplicationDBContext context,
                               IServicioUsuarios servicioUsuarios)
        {
            _context = context;
            _servicioUsuarios = servicioUsuarios;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<Paso>> Post(int tareaId, [FromBody] PasoCrearDTO pasoCrearDTO)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if(tarea is null)
            {
                return NotFound();
            }

            if(tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var existenPasos = await _context.Pasos.AnyAsync(p => p.TareaId == tareaId);

            var ordenMayor = 0;

            if(existenPasos)
            {
                ordenMayor = await _context.Pasos.Where(p => p.TareaId == tareaId)
                                                 .Select(p => p.Orden)
                                                 .MaxAsync();
            }

            var paso = new Paso();
            paso.TareaId = tareaId;
            paso.Orden = ordenMayor + 1;
            paso.Descripcion = pasoCrearDTO.Descripcion;
            paso.Realizado = pasoCrearDTO.Realizado;

            _context.Add(paso);
            await _context.SaveChangesAsync();
            
            return paso;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] PasoCrearDTO pasoCrearDTO)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            //Include para obtener la data de la Tarea asociada al Paso - JOIN
            var paso = await _context.Pasos.Include(p => p.Tarea)
                                           .FirstOrDefaultAsync(p => p.Id == id);

            if (paso is null)
            {
                return NotFound();
            }

            if (paso.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            paso.Descripcion = pasoCrearDTO.Descripcion;
            paso.Realizado = pasoCrearDTO.Realizado;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var paso = await _context.Pasos.Include(p => p.Tarea)
                                           .FirstOrDefaultAsync(p => p.Id == id);

            if(paso is null)
            {
                return NotFound();
            }

            if (paso.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            _context.Remove(paso);
            await _context.SaveChangesAsync();

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

            var pasos = await _context.Pasos.Where(p => p.TareaId == tareaId).ToListAsync();

            var pasosIds = pasos.Select(p => p.Id);

            var idsPasosNoPertenecenALaTarea = ids.Except(pasosIds).ToList();

            if(idsPasosNoPertenecenALaTarea.Any())
            {
                return BadRequest("No todos los pasos están presentes");
            }

            var pasosDiccionario = pasos.ToDictionary(p => p.Id);
            for (int i = 0; i < ids.Length; i++)
            {
                var pasoId = ids[i];
                var paso = pasosDiccionario[pasoId];
                paso.Orden = i + 1;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
