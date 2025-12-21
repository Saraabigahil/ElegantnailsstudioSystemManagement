using Npgsql;
using ElegantnailsstudioSystemManagement.Models;

namespace ElegantnailsstudioSystemManagement.Services
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

    public class UsuarioService : IUsuarioService, IDisposable
    {
        private readonly string _connectionString;
        private static bool _serviceInitialized = false;
        public UsuarioService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine("📌 UsuarioService constructor ejecutado");
        }


        public async Task<bool> ValidateUsuarioAsync(string email, string password)
        {
            Console.WriteLine($"\n\n=== 🔍 VALIDATEUSUARIOASYNC ===");
            Console.WriteLine($"📧 Email recibido: '{email}'");
            Console.WriteLine($"🔑 Password recibido: '{password}' (longitud: {password?.Length})");

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                Console.WriteLine("✅ Conexión a PostgreSQL establecida");

                var query = @"SELECT * FROM ""Usuarios"" WHERE LOWER(""Email"") = LOWER(@email)";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email.Trim().ToLower());

                Console.WriteLine("🔍 Ejecutando consulta...");
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var userId = reader.GetInt32(reader.GetOrdinal("Id"));
                    var userName = reader.GetString(reader.GetOrdinal("Nombre"));
                    var userEmail = reader.GetString(reader.GetOrdinal("Email"));
                    var storedPassword = reader.GetString(reader.GetOrdinal("Password"));
                    var rolId = reader.GetInt32(reader.GetOrdinal("rolid"));

                    Console.WriteLine($"✅ USUARIO ENCONTRADO:");
                    Console.WriteLine($"   ID: {userId}");
                    Console.WriteLine($"   Nombre: {userName}");
                    Console.WriteLine($"   Email: '{userEmail}'");
                    Console.WriteLine($"   Password almacenada: '{storedPassword}'");
                    Console.WriteLine($"   Rol ID: {rolId}");

                   
                    bool passwordsMatch = storedPassword == password;
                    Console.WriteLine($"\n🔍 COMPARANDO:");
                    Console.WriteLine($"   Almacenada: '{storedPassword}' (longitud: {storedPassword.Length})");
                    Console.WriteLine($"   Recibida: '{password}' (longitud: {password.Length})");
                    Console.WriteLine($"   Coinciden?: {passwordsMatch}");

                    return passwordsMatch;
                }
                else
                {
                    Console.WriteLine($"❌ NO SE ENCONTRÓ USUARIO CON EMAIL: '{email}'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR en ValidateUsuarioAsync: {ex.Message}");
                Console.WriteLine($"📋 StackTrace: {ex.StackTrace}");
                return false;
            }
        }
        public async Task<Usuario?> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT * FROM ""Usuarios"" WHERE ""Email"" = @email";

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var usuario = new Usuario
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        telefono = reader.IsDBNull(reader.GetOrdinal("telefono"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("telefono")),
                        rolid = reader.GetInt32(reader.GetOrdinal("rolid"))
                    };
                    return usuario;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetUsuarioByEmailAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateUsuarioAsync(Usuario usuario)
        {
            Console.WriteLine("\n=== 🚀 CREATEUSUARIOASYNC INICIADO ===");
            Console.WriteLine($"📦 Datos recibidos:");
            Console.WriteLine($"   Nombre: '{usuario.Nombre}'");
            Console.WriteLine($"   Email: '{usuario.Email}'");
            Console.WriteLine($"   Password: '{usuario.Password}' (length: {usuario.Password?.Length})");
            Console.WriteLine($"   Teléfono: '{usuario.telefono}'");
            Console.WriteLine($"   RolId recibido: {usuario.rolid}");

           
            Console.WriteLine($"\n🔥 DEBUG - Estado del objeto usuario:");
            Console.WriteLine($"   Nombre no vacío: {!string.IsNullOrWhiteSpace(usuario.Nombre)}");
            Console.WriteLine($"   Email no vacío: {!string.IsNullOrWhiteSpace(usuario.Email)}");
            Console.WriteLine($"   Password no vacío: {!string.IsNullOrWhiteSpace(usuario.Password)}");

            
            usuario.rolid = 2;
            Console.WriteLine($"   RolId forzado a: {usuario.rolid} (cliente)");

            
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                Console.WriteLine("❌ Nombre está vacío o nulo");
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                Console.WriteLine("❌ Email está vacío o nulo");
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario.Password))
            {
                Console.WriteLine("❌ Password está vacío o nulo");
                return false;
            }

            if (usuario.Password.Length < 6)
            {
                Console.WriteLine($"❌ Password demasiado corto: {usuario.Password.Length} caracteres");
                return false;
            }

            try
            {
            
                Console.WriteLine("🔌 Conectando a la base de datos...");
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                Console.WriteLine("✅ Conexión establecida");

               
                Console.WriteLine("🔍 Verificando si el email ya existe...");
                var checkQuery = @"SELECT COUNT(1) FROM ""Usuarios"" WHERE LOWER(""Email"") = LOWER(@email)";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@email", usuario.Email.Trim().ToLower());

                var existingCount = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
                Console.WriteLine($"📊 Resultado verificación: {existingCount} registros encontrados");

                if (existingCount > 0)
                {
                    Console.WriteLine($"❌ Email ya existe en la base de datos: {usuario.Email}");
                    return false;
                }

                Console.WriteLine("💾 Insertando nuevo usuario...");
                var query = @"
            INSERT INTO ""Usuarios"" 
            (""Nombre"", ""Email"", ""Password"", ""telefono"", ""rolid"")
            VALUES (@nombre, @email, @password, @telefono, @rolid)
            RETURNING ""Id""";

                using var cmd = new NpgsqlCommand(query, connection);

                
                cmd.Parameters.AddWithValue("@nombre",
                    string.IsNullOrWhiteSpace(usuario.Nombre) ? (object)DBNull.Value : usuario.Nombre.Trim());
                cmd.Parameters.AddWithValue("@email",
                    string.IsNullOrWhiteSpace(usuario.Email) ? (object)DBNull.Value : usuario.Email.Trim().ToLower());
                cmd.Parameters.AddWithValue("@password",
                    string.IsNullOrWhiteSpace(usuario.Password) ? (object)DBNull.Value : usuario.Password.Trim());

                
                if (string.IsNullOrWhiteSpace(usuario.telefono))
                {
                    cmd.Parameters.AddWithValue("@telefono", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@telefono", usuario.telefono.Trim());
                }

                cmd.Parameters.AddWithValue("@rolid", usuario.rolid);

                Console.WriteLine("⚡ Ejecutando comando SQL...");
                var result = await cmd.ExecuteScalarAsync();

                if (result != null)
                {
                    Console.WriteLine($"🎉✅ Usuario creado exitosamente con ID: {result}");

                    
                    var verifyQuery = @"SELECT COUNT(1) FROM ""Usuarios"" WHERE ""Id"" = @id";
                    using var verifyCmd = new NpgsqlCommand(verifyQuery, connection);
                    verifyCmd.Parameters.AddWithValue("@id", result);
                    var verifyCount = Convert.ToInt64(await verifyCmd.ExecuteScalarAsync());
                    Console.WriteLine($"🔍 Verificación: {verifyCount} registro(s) con ID {result}");

                    return true;
                }
                else
                {
                    Console.WriteLine("❌ No se pudo crear el usuario (resultado null)");
                    return false;
                }
            }
            catch (PostgresException ex)
            {
                Console.WriteLine($"❌❌ ERROR PostgreSQL en CreateUsuarioAsync:");
                Console.WriteLine($"   Código: {ex.SqlState}");
                Console.WriteLine($"   Mensaje: {ex.Message}");
                Console.WriteLine($"   Detalle: {ex.Detail}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥💥 ERROR CRÍTICO en CreateUsuarioAsync: {ex.Message}");
                Console.WriteLine($"   Tipo: {ex.GetType().Name}");
                Console.WriteLine($"📋 StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            try
            {
                var usuarios = new List<Usuario>();

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT * FROM ""Usuarios"" ORDER BY ""Id""";

                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    usuarios.Add(new Usuario
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        telefono = reader.IsDBNull(reader.GetOrdinal("telefono"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("telefono")),
                        rolid = reader.GetInt32(reader.GetOrdinal("rolid"))
                    });
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetUsuariosAsync: {ex.Message}");
                return new List<Usuario>();
            }
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT * FROM ""Usuarios"" WHERE ""Id"" = @id";

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Usuario
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        telefono = reader.IsDBNull(reader.GetOrdinal("telefono"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("telefono")),
                        rolid = reader.GetInt32(reader.GetOrdinal("rolid"))
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetUsuarioByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                
                var checkQuery = @"SELECT COUNT(1) FROM ""Usuarios"" WHERE ""Id"" = @id";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", usuario.Id);

                var exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0;
                if (!exists)
                {
                    return false;
                }

               
                var emailCheckQuery = @"SELECT COUNT(1) FROM ""Usuarios"" WHERE ""Email"" = @email AND ""Id"" != @id";
                using var emailCheckCmd = new NpgsqlCommand(emailCheckQuery, connection);
                emailCheckCmd.Parameters.AddWithValue("@email", usuario.Email);
                emailCheckCmd.Parameters.AddWithValue("@id", usuario.Id);

                var emailExists = Convert.ToInt64(await emailCheckCmd.ExecuteScalarAsync()) > 0;
                if (emailExists)
                {
                    return false;
                }

                var query = @"
                    UPDATE ""Usuarios"" 
                    SET ""Nombre"" = @nombre, 
                        ""Email"" = @email, 
                        ""telefono"" = @telefono, 
                        ""rolid"" = @rolid
                    WHERE ""Id"" = @id";

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@email", usuario.Email);
                cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(usuario.telefono)
                    ? (object)DBNull.Value : usuario.telefono);
                cmd.Parameters.AddWithValue("@rolid", usuario.rolid);
                cmd.Parameters.AddWithValue("@id", usuario.Id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR UpdateUsuarioAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"DELETE FROM ""Usuarios"" WHERE ""Id"" = @id";

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR DeleteUsuarioAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Usuario>> GetUsuariosByRolAsync(string rolNombre)
        {
            try
            {
                var usuarios = new List<Usuario>();

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

               
                var rolQuery = @"SELECT ""Id"" FROM ""Roles"" WHERE ""Nombre"" = @rolNombre";
                using var rolCmd = new NpgsqlCommand(rolQuery, connection);
                rolCmd.Parameters.AddWithValue("@rolNombre", rolNombre);

                var rolIdObj = await rolCmd.ExecuteScalarAsync();
                if (rolIdObj == null)
                {
                    return usuarios;
                }

                var rolId = Convert.ToInt32(rolIdObj);

                
                var query = @"SELECT * FROM ""Usuarios"" WHERE ""rolid"" = @rolId ORDER BY ""Nombre""";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@rolId", rolId);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    usuarios.Add(new Usuario
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        telefono = reader.IsDBNull(reader.GetOrdinal("telefono"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("telefono")),
                        rolid = reader.GetInt32(reader.GetOrdinal("rolid"))
                    });
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetUsuariosByRolAsync: {ex.Message}");
                return new List<Usuario>();
            }
        }

        public void Dispose()
        {
          
        }
    }
}






