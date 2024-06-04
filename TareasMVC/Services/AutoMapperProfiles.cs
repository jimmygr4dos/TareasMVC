using AutoMapper;
using TareasMVC.Entities;
using TareasMVC.Models;

namespace TareasMVC.Services
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles() 
        {
            CreateMap<Tarea, TareaDTO>()
            //Cantidad de pasos totales
            .ForMember(dto => dto.PasosTotal, ent => ent.MapFrom(x => x.Pasos.Count()))
            //Cantidad de pasos realizados (true)
            .ForMember(dto => dto.PasosRealizados, ent => ent.MapFrom(x => x.Pasos.Where(p => p.Realizado).Count()));
        }
    }
}
