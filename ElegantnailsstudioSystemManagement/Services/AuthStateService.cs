using ElegantnailsstudioSystemManagement.Models;

namespace ElegantnailsstudioSystemManagement.Services
{
    public class AuthStateService
    {
        public Usuario? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.Rol == 1;
        public bool IsUsuario => CurrentUser?.Rol == 2;

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