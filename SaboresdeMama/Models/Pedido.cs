using SaboresdeMama.Services;
using Microsoft.Maui.Graphics;
using System; // Asegúrate de tener System para DateTime

namespace SaboresdeMama.Models
{
    public class Pedido : IHasId
    {
        public string Id { get; set; }

        public string DescripcionProducto { get; set; }

        public string ClienteNombre { get; set; }

        public string ClienteId { get; set; }

        public string ClienteTelefono { get; set; }

        public string ClienteDireccion { get; set; }

        public DateTime FechaEntrega { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string Estado { get; set; } // "Pendiente", "Aceptado", "Rechazado", "Completado"

        public double Total { get; set; }

        public string DetallesPedido { get; set; }

        public string RecetaId { get; set; } // ID de la receta asociada al pedido (opcional)

        // --- Propiedades Calculadas para el Dashboard ---

        public string HoraEntrega => FechaEntrega.ToString("HH:mm");

        public string ProductoResumen => DescripcionProducto;

        public Color EstadoColor
        {
            get
            {
                return Estado switch
                {
                    "Pendiente" => Color.FromArgb("#F08080"), // Coral Suave (Alerta)
                    "Aceptado" => Color.FromArgb("#B3E0DC"), // Menta Suave
                    "Rechazado" => Color.FromArgb("#F08080"),
                    "Completado" => Colors.DarkGreen,
                    _ => Colors.Gray,
                };
            }
        }
    }
}