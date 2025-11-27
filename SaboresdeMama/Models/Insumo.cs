using SaboresdeMama.Services;

namespace SaboresdeMama.Models
{
    public class Insumo : IHasId
    {
        public string Id { get; set; }

        public string Nombre { get; set; }

        public double Cantidad { get; set; }

        public string Unidad { get; set; } // kg, gr, litros, unidades, etc.

        public DateTime FechaCreacion { get; set; }
    }
}

