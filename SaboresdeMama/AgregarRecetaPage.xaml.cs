using SaboresdeMama.Models;
using SaboresdeMama.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SaboresdeMama;

// Asegurate de que dice "public PARTIAL class"
public partial class AgregarRecetaPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private List<Insumo> _insumosDisponibles = new();
    private readonly ObservableCollection<InsumoReceta> _insumosSeleccionados = new();

    public AgregarRecetaPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        InsumosCollectionView.ItemsSource = _insumosSeleccionados;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarInsumosDisponiblesAsync();
    }

    private async Task CargarInsumosDisponiblesAsync()
    {
        _insumosDisponibles = await _databaseService.GetInsumosAsync();
        InsumoPicker.ItemsSource = _insumosDisponibles;
    }

    private void OnInsumoSeleccionadoChanged(object sender, EventArgs e)
    {
        if (InsumoPicker.SelectedItem is Insumo insumo)
        {
            UnidadSeleccionadaLabel.Text = $"Unidad: {insumo.Unidad}";
        }
        else
        {
            UnidadSeleccionadaLabel.Text = "Unidad: -";
        }
    }

    private async void OnAgregarInsumoNecesarioClicked(object sender, EventArgs e)
    {
        if (InsumoPicker.SelectedItem is not Insumo insumo)
        {
            await DisplayAlert("Error", "Seleccione un insumo.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(CantidadInsumoEntry.Text) ||
            !double.TryParse(CantidadInsumoEntry.Text, out double cantidad) ||
            cantidad <= 0)
        {
            await DisplayAlert("Error", "Ingrese una cantidad válida.", "OK");
            return;
        }

        var existente = _insumosSeleccionados.FirstOrDefault(i => i.InsumoId == insumo.Id);
        if (existente != null)
        {
            existente.CantidadNecesaria += cantidad;
            // Forzar refresco
            _insumosSeleccionados[_insumosSeleccionados.IndexOf(existente)] = existente;
        }
        else
        {
            _insumosSeleccionados.Add(new InsumoReceta
            {
                InsumoId = insumo.Id,
                InsumoNombre = insumo.Nombre,
                Unidad = insumo.Unidad,
                CantidadNecesaria = cantidad
            });
        }

        CantidadInsumoEntry.Text = string.Empty;
        InsumoPicker.SelectedIndex = -1;
        UnidadSeleccionadaLabel.Text = "Unidad: -";
    }

    private void OnEliminarInsumoNecesarioClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is InsumoReceta insumo)
        {
            _insumosSeleccionados.Remove(insumo);
        }
    }

    private async void OnGuardarRecetaClicked(object sender, EventArgs e)
    {
        // Validar campos
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
            string.IsNullOrWhiteSpace(ProcedimientoEditor.Text))
        {
            await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        var nuevaReceta = new Receta
        {
            Nombre = NombreEntry.Text,
            Ingredientes = string.Empty,
            Procedimiento = ProcedimientoEditor.Text,
            InsumosNecesarios = _insumosSeleccionados.ToList()
        };

        await _databaseService.AddRecetaAsync(nuevaReceta);

        await DisplayAlert("Exito", "Receta guardada correctamente.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
