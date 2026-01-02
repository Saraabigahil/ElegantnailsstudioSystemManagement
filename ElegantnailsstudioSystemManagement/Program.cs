using ElegantnailsstudioSystemManagement.Services;
using ElegantnailsstudioSystemManagement.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ElegantnailsstudioSystemManagement;

var builder = WebApplication.CreateBuilder(args);


var env = builder.Environment;
Console.WriteLine($"=== CONFIGURACIÓN DE RUTAS ===");
Console.WriteLine($"ContentRootPath: {env.ContentRootPath}");
Console.WriteLine($"WebRootPath: {env.WebRootPath}");
Console.WriteLine($"ApplicationName: {env.ApplicationName}");
Console.WriteLine($"EnvironmentName: {env.EnvironmentName}");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// CONFIGURACIÓN CRÍTICA PARA ARCHIVOS
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(300); 
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
    options.MaxBufferedUnacknowledgedRenderBatches = 100;
});

builder.Services.AddRazorPages();

// Configuración para manejar archivos grandes
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = 50 * 1024 * 1024; 
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; 
    options.MultipartHeadersLengthLimit = 1024 * 1024; 
});

// Configurar Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; 
});

// Configuración de base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

//servicios
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<ICupoService, CupoService>();
builder.Services.AddScoped<ILogoService, LogoService>();

// Servicio de Auth
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// CONFIGURACIÓN DE ARCHIVOS ESTÁTICOS
var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Storage");
var serviciosPath = Path.Combine(storagePath, "Servicios");

if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
    Console.WriteLine($"Carpeta Storage creada: {storagePath}");
}

if (!Directory.Exists(serviciosPath))
{
    Directory.CreateDirectory(serviciosPath);
    Console.WriteLine($"Carpeta Servicios creada: {serviciosPath}");
}

// Configurar acceso a imágenes almacenadas en Storage
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/Storage"
});

app.UseStaticFiles();


app.UseRouting();




app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");




// Agrega esto ANTES de app.Run();

// Middleware para manejar la subida del logo
app.Use(async (context, next) =>
{
    // Verificar si es la ruta de upload del logo
    if (context.Request.Path == "/uploadlogo" && context.Request.Method == "POST")
    {
        try
        {
            // Verificar que sea multipart/form-data
            if (!context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Formato inválido");
                return;
            }

            var form = await context.Request.ReadFormAsync();
            var file = form.Files["logoFile"];

            if (file == null || file.Length == 0)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("No se seleccionó archivo");
                return;
            }

            // Obtener el servicio desde DI
            var logoService = context.RequestServices.GetRequiredService<ILogoService>();

            // Guardar el logo
            var ruta = await logoService.GuardarLogoAsync(file);

            if (string.IsNullOrEmpty(ruta))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Error al guardar el logo");
                return;
            }

            // Redirigir de vuelta a la página del logo
            context.Response.Redirect("/admin/logo?success=true");
            return;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Error: {ex.Message}");
            return;
        }
    }

    await next();
});
app.Run();








