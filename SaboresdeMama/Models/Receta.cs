using SaboresdeMama.Services;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        // Lista de insumos necesarios para la receta (guardada como JSON string en Firebase)
        public string InsumosNecesariosJson { get; set; }

        // Propiedad calculada para obtener la lista de insumos
        public List<InsumoReceta> InsumosNecesarios
        {
            get
            {
                if (string.IsNullOrWhiteSpace(InsumosNecesariosJson))
                    return new List<InsumoReceta>();

                try
                {
                    return JsonConvert.DeserializeObject<List<InsumoReceta>>(InsumosNecesariosJson) ?? new List<InsumoReceta>();
                }
                catch
                {
                    return new List<InsumoReceta>();
                }
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    InsumosNecesariosJson = string.Empty;
                }
                else
                {
                    InsumosNecesariosJson = JsonConvert.SerializeObject(value);
                }
            }
        }
    }
}