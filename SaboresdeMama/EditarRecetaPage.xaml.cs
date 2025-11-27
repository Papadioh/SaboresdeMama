using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama;

[QueryProperty(nameof(Receta), "Receta")]
public partial class EditarRecetaPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Receta _receta;

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

    public EditarRecetaPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    // Carga los datos de la receta en los campos del formulario
    private void CargarDatosReceta()
    {
        if (Receta != null)
        {
            NombreEntry.Text = Receta.Nombre;
            IngredientesEditor.Text = Receta.Ingredientes;
            ProcedimientoEditor.Text = Receta.Procedimiento;
        }
    }

    private async void OnGuardarCambiosClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
            string.IsNullOrWhiteSpace(IngredientesEditor.Text) ||
            string.IsNullOrWhiteSpace(ProcedimientoEditor.Text))
        {
            await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        // Actualiza el objeto Receta existente
        Receta.Nombre = NombreEntry.Text;
        Receta.Ingredientes = IngredientesEditor.Text;
        Receta.Procedimiento = ProcedimientoEditor.Text;

        // Llama al método de actualización en la base de datos
        await _databaseService.UpdateRecetaAsync(Receta);

        await DisplayAlert("Éxito", "Receta actualizada correctamente.", "OK");

        // Regresa a la página anterior (el recetario)
        await Shell.Current.GoToAsync("..");
    }
}