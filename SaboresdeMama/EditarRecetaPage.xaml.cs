using SaboresdeMama.Models;
using SaboresdeMama.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SaboresdeMama;

[QueryProperty(nameof(Receta), "Receta")]
public partial class EditarRecetaPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Receta _receta;
    private List<Insumo> _insumosDisponibles = new();
    private readonly ObservableCollection<InsumoReceta> _insumosSeleccionados = new();

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
        InsumosCollectionView.ItemsSource = _insumosSeleccionados;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarInsumosDisponiblesAsync();
        SincronizarInsumosConReceta();
    }

    private async Task CargarInsumosDisponiblesAsync()
    {
        _insumosDisponibles = await _databaseService.GetInsumosAsync();
        InsumoPicker.ItemsSource = _insumosDisponibles;
    }

    // Carga los datos de la receta en los campos del formulario
    private void CargarDatosReceta()
    {
        if (Receta != null)
        {
            NombreEntry.Text = Receta.Nombre;
            ProcedimientoEditor.Text = Receta.Procedimiento;
            SincronizarInsumosConReceta();
        }
    }

    private void SincronizarInsumosConReceta()
    {
        if (Receta?.InsumosNecesarios == null)
        {
            _insumosSeleccionados.Clear();
            return;
        }

        _insumosSeleccionados.Clear();
        foreach (var insumo in Receta.InsumosNecesarios)
        {
            _insumosSeleccionados.Add(new InsumoReceta
            {
                InsumoId = insumo.InsumoId,
                InsumoNombre = insumo.InsumoNombre,
                CantidadNecesaria = insumo.CantidadNecesaria,
                Unidad = insumo.Unidad
            });
        }
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

    private async void OnGuardarCambiosClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
            string.IsNullOrWhiteSpace(ProcedimientoEditor.Text))
        {
            await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        // Actualiza el objeto Receta existente
        Receta.Nombre = NombreEntry.Text;
        Receta.Procedimiento = ProcedimientoEditor.Text;
        Receta.InsumosNecesarios = _insumosSeleccionados.ToList();

        await _databaseService.UpdateRecetaAsync(Receta);

        await DisplayAlert("Exito", "Receta actualizada correctamente.", "OK");

        await Shell.Current.GoToAsync("..");
    }
}