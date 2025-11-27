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
                ProcedimientoLabel.Text = Receta.Procedimiento ?? "";
                ActualizarListaInsumos();

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

    private void ActualizarListaInsumos()
    {
        if (InsumosListLayout == null)
            return;

        InsumosListLayout.Children.Clear();

        if (Receta?.InsumosNecesarios == null || Receta.InsumosNecesarios.Count == 0)
        {
            InsumosListLayout.Children.Add(new Label
            {
                Text = "No se configuraron insumos específicos para esta receta.",
                FontSize = 12,
                TextColor = Colors.Gray
            });
            return;
        }

        foreach (var insumo in Receta.InsumosNecesarios)
        {
            InsumosListLayout.Children.Add(new Label
            {
                Text = $"{insumo.InsumoNombre}: {insumo.CantidadNecesaria:F2} {insumo.Unidad}",
                FontSize = 14
            });
        }
    }
}
