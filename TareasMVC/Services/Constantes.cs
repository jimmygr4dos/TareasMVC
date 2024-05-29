using Microsoft.AspNetCore.Mvc.Rendering;

namespace TareasMVC.Services
{
    public class Constantes
    {
        public const string RolAdmin = "admin";

        public static readonly SelectListItem[] CulturasUISoportadas = new SelectListItem[]
        {
            new SelectListItem{ Value = "es", Text = "Español" },
            new SelectListItem{ Value = "en", Text = "English" }
        };
    }
}
