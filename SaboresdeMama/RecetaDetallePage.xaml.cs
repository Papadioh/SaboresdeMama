using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama;

[QueryProperty(nameof(Receta), "Receta")]
// Asegúrate de que dice "public PARTIAL class"
public partial class RecetaDetallePage : ContentPage
{
    Receta _receta;
    private readonly DatabaseService _databaseService;

    public Receta Receta
    {
        get => _receta;
        set
        {
            _receta = value;
            OnPropertyChanged();
            CargarDatosReceta();
        }
    }

    public RecetaDetallePage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private async void CargarDatosReceta()
    {
        if (Receta != null && NombreLabel != null)
        {
            try
            {
                // Todos estos errores desaparecerán
                NombreLabel.Text = Receta.Nombre ?? "";
                IngredientesLabel.Text = Receta.Ingredientes ?? "";
                ProcedimientoLabel.Text = Receta.Procedimiento ?? "";

                // Verificar disponibilidad de insumos
                if (DisponibilidadLabel != null)
                {
                    try
                    {
                        var (disponible, insumosFaltantes) = await _databaseService.VerificarDisponibilidadInsumosAsync(Receta);
                        
                        if (Receta.InsumosNecesarios == null || Receta.InsumosNecesarios.Count == 0)
                        {
                            DisponibilidadLabel.Text = "ADVERTENCIA: No hay insumos definidos para esta receta";
                            DisponibilidadLabel.TextColor = Colors.Orange;
                        }
                        else if (disponible)
                        {
                            DisponibilidadLabel.Text = "OK: Hay suficientes insumos disponibles";
                            DisponibilidadLabel.TextColor = Colors.Green;
                        }
                        else
                        {
                            DisponibilidadLabel.Text = "ERROR: Faltan insumos:\n" + string.Join("\n", insumosFaltantes);
                            DisponibilidadLabel.TextColor = Colors.Red;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error verificando insumos: {ex.Message}");
                        DisponibilidadLabel.Text = "ADVERTENCIA: No se pudo verificar la disponibilidad de insumos";
                        DisponibilidadLabel.TextColor = Colors.Orange;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos de receta: {ex.Message}");
            }
        }
    }
}
