using SaboresdeMama.Services;

namespace SaboresdeMama.Models
{
    public class Producto : IHasId
    {
        public string Id { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public double Precio { get; set; }

        public int Stock { get; set; }

        public string InformacionExtra { get; set; }

        public string Categoria { get; set; }
        public string ImageURL { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}

