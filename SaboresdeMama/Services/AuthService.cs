using SaboresdeMama.Models;

namespace SaboresdeMama.Services
{
    public class AuthService
    {
        private static Usuario? _usuarioActual;
        private readonly DatabaseService _databaseService;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public static Usuario? UsuarioActual => _usuarioActual;
        public static bool EstaAutenticado => _usuarioActual != null;
        public static bool EsAdmin => _usuarioActual?.TipoUsuario == "Admin";
        public static bool EsCliente => _usuarioActual?.TipoUsuario == "Cliente";

        public static void SetUsuarioActual(Usuario usuario)
        {
            _usuarioActual = usuario;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var usuario = await _databaseService.GetUsuarioByUsernameAsync(username);
                
                if (usuario != null && usuario.Password == password)
                {
                    _usuarioActual = usuario;
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en LoginAsync: {ex.Message}");
                throw;
            }
        }

        public void Logout()
        {
            _usuarioActual = null;
        }
    }
}

