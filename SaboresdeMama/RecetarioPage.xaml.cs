using SaboresdeMama.Services;
using SaboresdeMama.Models;

namespace SaboresdeMama;

public partial class RecetarioPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public RecetarioPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRecetasAsync(RecetaSearchBar.Text);
    }

    private async Task LoadRecetasAsync(string searchTerm)
    {
        var recetas = await _databaseService.SearchRecetasAsync(searchTerm);
        RecetasCollectionView.ItemsSource = recetas;
    }

    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        await LoadRecetasAsync(RecetaSearchBar.Text);
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            await LoadRecetasAsync(string.Empty);
        }
    }

    private async void OnAddRecetaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AgregarRecetaPage));
    }

    private async void OnVerClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is Receta recetaSeleccionada)
        {
            var parametros = new Dictionary<string, object>
            {
                { "Receta", recetaSeleccionada }
            };
            await Shell.Current.GoToAsync(nameof(RecetaDetallePage), parametros);
        }
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is Receta receta)
        {
            bool confirmar = await DisplayAlert("Confirmar", $"¿Seguro que quieres borrar la receta '{receta.Nombre}'?", "Sí", "No");
            if (confirmar)
            {
                await _databaseService.DeleteRecetaAsync(receta);
                await LoadRecetasAsync(RecetaSearchBar.Text);
            }
        }
    }

    // ======================================================
    // ===== NUEVO MÉTODO AÑADIDO (PARA NAVEGAR A EDITAR) =====
    // ======================================================
    private async void OnEditarClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is Receta recetaSeleccionada)
        {
            var parametros = new Dictionary<string, object>
            {
                { "Receta", recetaSeleccionada }
            };
            // Navegamos a la nueva página de edición
            await Shell.Current.GoToAsync(nameof(EditarRecetaPage), parametros);
        }
    }
}