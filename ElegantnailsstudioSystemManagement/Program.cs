using ElegantnailsstudioSystemManagement.Services;
using ElegantnailsstudioSystemManagement.Providers;
using Microsoft.AspNetCore.Components.Authorization;

using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using ElegantnailsstudioSystemManagement;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<ICupoService, CupoService>();
builder.Services.AddScoped<ILogoService, LogoService>();


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
app.UseStaticFiles();
app.UseRouting();


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





