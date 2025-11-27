namespace SaboresdeMama.Models
{
    public class Usuario
    {
        public string Id { get; set; } // ID generado por Firebase o GUID
        public string Nombre { get; set; } // Nombre completo del cliente
        public string Username { get; set; } // Correo o nombre de usuario
        public string Password { get; set; } // Contraseña
        public string TipoUsuario { get; set; } // "Cliente" o "Admin"
        public DateTime FechaRegistro { get; set; }
    }
}