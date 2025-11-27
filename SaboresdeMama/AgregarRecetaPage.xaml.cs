using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama;

// Aseg�rate de que dice "public PARTIAL class"
public partial class AgregarRecetaPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public AgregarRecetaPage(DatabaseService databaseService)
    {
        InitializeComponent();�
        _databaseService = databaseService;
    }

    private async void OnGuardarRecetaClicked(object sender, EventArgs e)
    {
        // Todos estos errores desaparecer�n
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
            string.IsNullOrWhiteSpace(IngredientesEditor.Text) ||
            string.IsNullOrWhiteSpace(ProcedimientoEditor.Text))
        {
            await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        var nuevaReceta = new Receta
        {
            Nombre = NombreEntry.Text,
            Ingredientes = IngredientesEditor.Text,
            Procedimiento = ProcedimientoEditor.Text
        };

        await _databaseService.AddRecetaAsync(nuevaReceta);

        await DisplayAlert("�xito", "Receta guardada correctamente.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}