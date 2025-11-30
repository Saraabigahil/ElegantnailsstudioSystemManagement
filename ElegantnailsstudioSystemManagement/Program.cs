using ElegantnailsstudioSystemManagement;
using ElegantnailsstudioSystemManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()  // ? CAMBIA ESTO
    .AddInteractiveServerComponents(); // ? Y AÑADE ESTO

// Registrar servicios personalizados
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICupoService, CupoService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<AuthStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ? USA ESTO EN VEZ DE MapBlazorHub()
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();