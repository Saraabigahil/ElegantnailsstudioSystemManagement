using ElegantnailsstudioSystemManagement.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json; 

namespace ElegantnailsstudioSystemManagement.Services
{
    public class AuthService
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ProtectedSessionStorage _sessionStorage;
        private Usuario _currentUser = null;

        public AuthService(IUsuarioService usuarioService, ProtectedSessionStorage sessionStorage)
        {
            _usuarioService = usuarioService;
            _sessionStorage = sessionStorage;
            Console.WriteLine($"🎯 AuthService inicializado - Instancia: {this.GetHashCode()}");
        }

        public bool IsLoggedIn => _currentUser != null;
        public Usuario CurrentUser => _currentUser;
        public bool IsAdmin => _currentUser?.rolid == 1;


        public async Task<bool> LoginAsync(string email, string password)
        {
            Console.WriteLine($"\n🔐 LOGIN ASYNC INICIADO");
            Console.WriteLine($"📧 Email recibido: {email}");
            Console.WriteLine($"🔑 Password recibido: {password}");

            try
            {
                var isValid = await _usuarioService.ValidateUsuarioAsync(email, password).ConfigureAwait(false);
                Console.WriteLine($"✅ Validación en PostgreSQL: {isValid}");

                if (!isValid)
                {
                    Console.WriteLine("❌ Credenciales incorrectas en BD");
                    return false;
                }

                var usuario = await _usuarioService.GetUsuarioByEmailAsync(email).ConfigureAwait(false);
                if (usuario == null)
                {
                    Console.WriteLine("❌ Usuario no encontrado en BD");
                    return false;
                }

                Console.WriteLine($"✅ Usuario obtenido de BD: {usuario.Nombre}");

                _currentUser = usuario;

                try
                {
                    var usuarioData = new
                    {
                        Id = usuario.Id,
                        Nombre = usuario.Nombre,
                        Email = usuario.Email,
                        RolId = usuario.rolid,
                        FechaLogin = DateTime.Now
                    };

                    await _sessionStorage.SetAsync("usuario_autenticado", JsonSerializer.Serialize(usuarioData)).ConfigureAwait(false);
                    Console.WriteLine("💾 Usuario guardado en sessionStorage");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ No se pudo guardar en sessionStorage: {ex.Message}");
                }

                Console.WriteLine("🎉✅ LOGIN EXITOSO CON POSTGRESQL");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR en LoginAsync: {ex.Message}");
                Console.WriteLine($"📋 StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            Console.WriteLine("🚪 CERRANDO SESIÓN...");
            _currentUser = null;

            try
            {
                await _sessionStorage.DeleteAsync("usuario_autenticado");
                Console.WriteLine("✅ SessionStorage limpiado");
            }
            catch
            {
               
            }

            await Task.CompletedTask;
        }


        public async Task InitializeAsync()
        {
            Console.WriteLine("🔄 AuthService InitializeAsync");

            try
            {
                var result = await _sessionStorage.GetAsync<string>("usuario_autenticado").ConfigureAwait(false);

                if (result.Success && !string.IsNullOrEmpty(result.Value))
                {
                    var usuarioData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.Value);

                    _currentUser = new Usuario
                    {
                        Id = usuarioData["Id"].GetInt32(),
                        Nombre = usuarioData["Nombre"].GetString(),
                        Email = usuarioData["Email"].GetString(),
                        rolid = usuarioData["RolId"].GetInt32()
                    };
                    Console.WriteLine($"✅ Sesión recuperada: {_currentUser.Nombre}");
                }
                else
                {
                    Console.WriteLine("📭 No hay sesión guardada");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error recuperando sesión: {ex.Message}");
            }
        }
    }
}






