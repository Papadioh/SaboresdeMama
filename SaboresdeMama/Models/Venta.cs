using SaboresdeMama.Services;

namespace SaboresdeMama.Models
{
    public class Venta : IHasId
    {
        public string Id { get; set; }

        public string PedidoId { get; set; }

        public string DescripcionProducto { get; set; }

        // ===================================
        // ===== NUEVA PROPIEDAD AÑADIDA =====
        public string ClienteNombre { get; set; }
        // ===================================

        public string ClienteTelefono { get; set; }

        public string ClienteDireccion { get; set; }

        public double Total { get; set; }

        public DateTime FechaCompletado { get; set; }
    }
}