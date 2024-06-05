using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entities;
using TareasMVC.Models;
using TareasMVC.Services;

namespace TareasMVC.Controllers
{
    [Route("api/tareas")]
    public class TareasController: ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IMapper _mapper;

        public TareasController(ApplicationDBContext context, 
                                IServicioUsuarios servicioUsuarios,
                                IMapper mapper)
        {
            _context = context;
            _servicioUsuarios = servicioUsuarios;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<TareaDTO>>> Get()
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var tareas = await _context.Tareas
                                 .Where(t => t.UsuarioCreacionId == usuarioId)
                                 .OrderBy(t => t.Orden)
                                 //ProjectDTO me sirve para mapear una clase DTO
                                 //.Select(t => new TareaDTO
                                 //{
                                 //    Id = t.Id,
                                 //    Titulo = t.Titulo
                                 //})
                                 //En el AutoMapperProfile se configuró Pasos Totales y Pasos Realizados
                                 .ProjectTo<TareaDTO>(_mapper.ConfigurationProvider)
                                 .ToListAsync();
            return tareas;
        }

        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var existenTareas = await _context.Tareas.AnyAsync(t => t.UsuarioCreacionId == usuarioId);
            var ordenMayor = 0;
            if (existenTareas)
            {
                ordenMayor = await _context.Tareas.Where(t => t.UsuarioCreacionId == usuarioId)
                                                  .Select(t => t.Orden).MaxAsync();
            }

            var tarea = new Tarea
            {
                Titulo = titulo,
                UsuarioCreacionId = usuarioId,
                FechaCreacion = DateTime.UtcNow,
                Orden = ordenMayor + 1
            };

            _context.Add(tarea);
            await _context.SaveChangesAsync();

            return tarea;
        }

        [HttpPost("ordenar")]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var tareas = await _context.Tareas.Where(t => t.UsuarioCreacionId == usuarioId).ToListAsync();
            var tareasId = tareas.Select(t => t.Id);
            var idsTareasNoPertenecenAlUsuario = ids.Except(tareasId).ToList();
            
            if(idsTareasNoPertenecenAlUsuario.Any())
            {
                return Forbid();
            }

            var tareasDiccionario = tareas.ToDictionary(t => t.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var tarea = tareasDiccionario[id];
                tarea.Orden = i + 1;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            
            //Include para incluir los Pasos de la Tarea y OrderBy para ordenarlos
            var tarea = await _context.Tareas.Include(t => t.Pasos.OrderBy(p => p.Orden))
                                             .Include(t => t.ArchivosAdjuntos.OrderBy(a => a.Orden))
                                             .FirstOrDefaultAsync(t => t.Id == id && 
                                                                       t.UsuarioCreacionId == usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }

            return tarea;
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarTarea(int id, [FromBody] TareaEditarDTO tareaEditarDTO)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && 
                                                                       t.UsuarioCreacionId == usuarioId);
            if (tarea is null)
            {
                return NotFound();
            }

            tarea.Titulo = tareaEditarDTO.Titulo;
            tarea.Descripcion = tareaEditarDTO.Descripcion;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id &&
                                                                       t.UsuarioCreacionId == usuarioId);
            if (tarea is null)
            {
                return NotFound();
            }

            _context.Remove(tarea);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
