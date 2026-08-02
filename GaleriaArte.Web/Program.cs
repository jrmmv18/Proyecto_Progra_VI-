using GaleriaArte.Web.Data;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Servicios MVC
builder.Services.AddControllersWithViews();

// Servicios de acceso a datos
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ArtistaRepository>();
builder.Services.AddScoped<ObraRepository>();
builder.Services.AddScoped<ProveedorRepository>();
builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<VisitaRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<FacturaRepository>();
builder.Services.AddScoped<ClienteRepository>();

// NUEVO: Registro ordenado para módulo de mantenimiento (HU-11)
builder.Services.AddScoped<GaleriaArte.Web.Data.IMantenimientoRepository, GaleriaArte.Web.Data.MantenimientoRepository>();

// Autenticación mediante cookies
builder.Services.AddAuthentication("GaleriaArteCookie")
    .AddCookie("GaleriaArteCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configuración del manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Primero autenticación y después autorización
app.UseAuthentication();
app.UseAuthorization();

// Ruta predeterminada
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
