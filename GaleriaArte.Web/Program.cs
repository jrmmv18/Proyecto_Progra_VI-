using GaleriaArte.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Servicios MVC
builder.Services.AddControllersWithViews();

// Servicios de acceso a datos
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ArtistaRepository>();
builder.Services.AddScoped<ObraRepository>();
builder.Services.AddScoped<ProveedorRepository>();
builder.Services.AddScoped<ProductoRepository>();

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