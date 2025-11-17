using ElegantNailsStudioSystemManagement.Models;

namespace ElegantNailsStudioSystemManagement.Services
{
    public class AuthStateService
    {
        public Usuario? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.Rol?.Nombre == "Admin";
        public bool IsUsuario => CurrentUser?.Rol?.Nombre == "Usuario";

        public event Action? OnChange;

        public void Login(Usuario user)
        {
            CurrentUser = user;
            NotifyStateChanged();
        }

        public void Logout()
        {
            CurrentUser = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}