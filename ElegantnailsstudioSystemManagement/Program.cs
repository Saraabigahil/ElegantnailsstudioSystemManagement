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

// Configuracion
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; 
});

// Configuración de base de datos
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


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

// Configuracion acceso a imágenes almacenadas en Storage
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/Storage"
});

app.UseStaticFiles();


app.UseRouting();




app.UseAuthentication();
app.UseAuthorization();


app.Use(async (context, next) =>
{
    
    if (context.Request.Path == "/uploadlogo" && context.Request.Method == "POST")
    {
        try
        {
            
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

            
            var logoService = context.RequestServices.GetRequiredService<ILogoService>();

            
            var ruta = await logoService.GuardarLogoAsync(file);

            if (string.IsNullOrEmpty(ruta))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Error al guardar el logo");
                return;
            }

            
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

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/uploadservicio" &&
        context.Request.Method == "POST")
    {
        try
        {
            Console.WriteLine("📸 Iniciando subida de imagen para servicio...");

            if (!context.Request.HasFormContentType)
            {
                Console.WriteLine("❌ No es form-data");
                context.Response.StatusCode = 400;
                return;
            }

            var form = await context.Request.ReadFormAsync();
            var file = form.Files["file"];
            var servicioIdStr = form["servicioId"].ToString();

            Console.WriteLine($"📋 Servicio ID recibido: {servicioIdStr}");
            Console.WriteLine($"📁 Archivo recibido: {(file != null ? file.FileName : "NULL")}");

            if (file == null || file.Length == 0)
            {
                Console.WriteLine("❌ Archivo vacío o nulo");
                context.Response.StatusCode = 400;
                return;
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            Console.WriteLine($"📄 Extensión: {ext}");

            if (!allowed.Contains(ext))
            {
                Console.WriteLine("❌ Extensión no permitida");
                context.Response.StatusCode = 400;
                return;
            }

            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Storage",
                "Servicios"
            );

            Console.WriteLine($"📂 Carpeta destino: {folder}");

            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(folder, fileName);

            Console.WriteLine($"💾 Guardando archivo: {path}");

            await using var fs = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fs);

            var url = $"/Storage/Servicios/{fileName}";
            Console.WriteLine($"🌐 URL generada: {url}");

         
            if (!string.IsNullOrEmpty(servicioIdStr) && int.TryParse(servicioIdStr, out int servicioId))
            {
                Console.WriteLine($"🔍 Buscando servicio ID {servicioId} en BD...");

                using var scope = context.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var servicio = await dbContext.Servicios.FindAsync(servicioId);
                if (servicio != null)
                {
                    Console.WriteLine($"✅ Servicio encontrado: {servicio.Nombre}");
                    servicio.ImagenUrl = url;
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"🖼️ Imagen actualizada en BD para servicio ID: {servicioId}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Servicio ID {servicioId} no encontrado en BD");
                }
            }

           
            Console.WriteLine($"🔄 Redirigiendo a /admin/servicios con imagen: {url}");
            context.Response.Redirect($"/admin/servicios?img={url}&servicioId={servicioIdStr}&updated=true");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERROR en uploadservicio: {ex.Message}");
            Console.WriteLine($"💥 StackTrace: {ex.StackTrace}");
            context.Response.StatusCode = 500;
            return;
        }
    }

    await next();
});


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();








