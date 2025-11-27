using SaboresdeMama.Services;

namespace SaboresdeMama.Models
{
    public class Receta : IHasId
    {
        public string Id { get; set; }

        public string Nombre { get; set; }

        // Guardaremos los ingredientes como un solo bloque de texto.
        // El usuario puede usar saltos de línea.
        public string Ingredientes { get; set; }

        public string Procedimiento { get; set; }
    }
}