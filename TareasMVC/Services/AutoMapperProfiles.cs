using AutoMapper;
using TareasMVC.Entities;
using TareasMVC.Models;

namespace TareasMVC.Services
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles() 
        {
            CreateMap<Tarea, TareaDTO>().ReverseMap();
        }
    }
}
