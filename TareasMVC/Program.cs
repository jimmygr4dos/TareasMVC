using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;
using TareasMVC;
using TareasMVC.Services;

var builder = WebApplication.CreateBuilder(args);

//Política para requerir que los usuarios estén autenticados de manera global
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix) //Para cambiar el idioma en una Vista
  .AddDataAnnotationsLocalization(options =>
  {
      //Aquí indicamos que la clase RecursoCompartido se instanciará como factory y podrá compartir sus .resx
      options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(RecursoCompartido));
  }).AddJsonOptions(options =>
  {
      //Para ignorar las referencias cíclicas
      options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
  });

builder.Services.AddDbContext<ApplicationDBContext>
                    (options => options.UseSqlServer("name=DefaultConnection"));

//Configura el Authentication
//Se agregó la autenticación por Microsoft Account usando User Secrets
builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["MicrosoftClientId"];
    options.ClientSecret = builder.Configuration["MicrosoftSecretId"];
});

//Configura Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();

//Definimos nuestra propia vista de login
builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/usuarios/login";
    options.AccessDeniedPath = "/usuarios/login";
});

//Para Internacionalización con IStringLocalizer y los archivos de Recursos
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

//Para soportar diferentes idiomas
//var culturasUISoportadas = new[] { "es", "en" };
app.UseRequestLocalization(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es");
    //options.SupportedUICultures = culturasUISoportadas.Select(cultura => new CultureInfo(cultura)).ToList();
    options.SupportedUICultures = Constantes.CulturasUISoportadas
                                                .Select(cultura => new CultureInfo(cultura.Value)).ToList();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//Antes de Authorization
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
