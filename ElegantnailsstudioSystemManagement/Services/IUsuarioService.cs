using ElegantNailsStudioSystemManagement.Models;

namespace ElegantNailsStudioSystemManagement.Services
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> GetUsuariosAsync();
        Task<Usuario?> GetUsuarioByIdAsync(int id);
        Task<Usuario?> GetUsuarioByEmailAsync(string email);
        Task<bool> CreateUsuarioAsync(Usuario usuario);
        Task<bool> UpdateUsuarioAsync(Usuario usuario);
        Task<bool> DeleteUsuarioAsync(int id);
        Task<bool> ValidateUsuarioAsync(string email, string password);
        Task<List<Usuario>> GetUsuariosByRolAsync(string rolNombre);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly List<Usuario> _usuarios = new();
        private readonly List<Rol> _roles = new();
        private int _nextUsuarioId = 1;

        public UsuarioService()
        {
            // Inicializar roles
            _roles.AddRange(new[]
            {
                new Rol { Id = 1, Nombre = "Admin" },
                new Rol { Id = 2, Nombre = "Usuario" }
            });

            // Usuario admin por defecto
            _usuarios.Add(new Usuario
            {
                Id = _nextUsuarioId++,
                Nombre = "Administrador",
                Email = "admin@elegantnails.com",
                Password = "admin123",
                Telefono = "76872677",
                RolId = 1,
                FechaRegistro = DateTime.Now
            });

            // Usuario cliente de ejemplo
            _usuarios.Add(new Usuario
            {
                Id = _nextUsuarioId++,
                Nombre = "Cliente Ejemplo",
                Email = "cliente@ejemplo.com",
                Password = "cliente123",
                Telefono = "1234-5678",
                RolId = 2,
                FechaRegistro = DateTime.Now
            });
        }

        public Task<List<Usuario>> GetUsuariosAsync()
        {
            var usuarios = _usuarios.Select(u => {
                u.Rol = _roles.FirstOrDefault(r => r.Id == u.RolId);
                return u;
            }).ToList();

            return Task.FromResult(usuarios);
        }

        public Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario != null)
            {
                usuario.Rol = _roles.FirstOrDefault(r => r.Id == usuario.RolId);
            }
            return Task.FromResult(usuario);
        }

        public Task<Usuario?> GetUsuarioByEmailAsync(string email)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario != null)
            {
                usuario.Rol = _roles.FirstOrDefault(r => r.Id == usuario.RolId);
            }
            return Task.FromResult(usuario);
        }

        public Task<bool> CreateUsuarioAsync(Usuario usuario)
        {
            if (_usuarios.Any(u => u.Email == usuario.Email))
                return Task.FromResult(false);

            usuario.Id = _nextUsuarioId++;
            usuario.RolId = 2; 
            usuario.FechaRegistro = DateTime.Now;
            _usuarios.Add(usuario);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateUsuarioAsync(Usuario usuario)
        {
            var existing = _usuarios.FirstOrDefault(u => u.Id == usuario.Id);
            if (existing != null)
            {
                existing.Nombre = usuario.Nombre;
                existing.Email = usuario.Email;
                existing.Telefono = usuario.Telefono;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteUsuarioAsync(int id)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario != null)
            {
                _usuarios.Remove(usuario);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ValidateUsuarioAsync(string email, string password)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Email == email && u.Password == password);
            return Task.FromResult(usuario != null);
        }

        public Task<List<Usuario>> GetUsuariosByRolAsync(string rolNombre)
        {
            var rol = _roles.FirstOrDefault(r => r.Nombre == rolNombre);
            if (rol == null) return Task.FromResult(new List<Usuario>());

            var usuarios = _usuarios
                .Where(u => u.RolId == rol.Id)
                .Select(u => {
                    u.Rol = rol;
                    return u;
                })
                .ToList();

            return Task.FromResult(usuarios);
        }
    }
}