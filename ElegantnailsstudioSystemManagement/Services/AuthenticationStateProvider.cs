using ElegantnailsstudioSystemManagement.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ElegantnailsstudioSystemManagement.Providers
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _authService;

        public CustomAuthenticationStateProvider(AuthService authService)
        {
            _authService = authService;
            Console.WriteLine("✅ CustomAuthenticationStateProvider creado");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // ESPERA a que se inicialice el AuthService
                await _authService.InitializeAsync().ConfigureAwait(false); // <-- Cambia esto

                if (_authService.IsLoggedIn && _authService.CurrentUser != null)
                {
                    var user = _authService.CurrentUser;

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("RolId", user.rolid.ToString())
            };

                    var identity = new ClaimsIdentity(claims, "postgresql_auth");
                    var principal = new ClaimsPrincipal(identity);

                    Console.WriteLine($"✅ Usuario autenticado en Provider: {user.Nombre}");
                    return new AuthenticationState(principal);
                }

                Console.WriteLine("🔒 Usuario NO autenticado en Provider");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GetAuthenticationStateAsync: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthenticationChanged()
        {
            Console.WriteLine("🔄 Notificando cambio de autenticación");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}







