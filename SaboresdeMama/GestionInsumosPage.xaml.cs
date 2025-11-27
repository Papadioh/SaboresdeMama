using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama
{
    public partial class GestionInsumosPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public GestionInsumosPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarInsumos();
        }

        private async Task CargarInsumos()
        {
            var insumos = await _databaseService.GetInsumosAsync();
            InsumosCollectionView.ItemsSource = insumos;
        }

        private async void OnSearchButtonPressed(object sender, EventArgs e)
        {
            await BuscarInsumos();
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            await BuscarInsumos();
        }

        private async Task BuscarInsumos()
        {
            var searchTerm = InsumoSearchBar.Text;
            var todosInsumos = await _databaseService.GetInsumosAsync();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                InsumosCollectionView.ItemsSource = todosInsumos;
                return;
            }

            var lowerSearchTerm = searchTerm.ToLower();
            var insumosFiltrados = todosInsumos
                .Where(i => i.Nombre?.ToLower().Contains(lowerSearchTerm) == true)
                .ToList();
            InsumosCollectionView.ItemsSource = insumosFiltrados;
        }

        private async void OnAddInsumoClicked(object sender, EventArgs e)
        {
            var nombre = await DisplayPromptAsync(
                "Nuevo Insumo",
                "Ingrese el nombre del insumo:",
                "Aceptar",
                "Cancelar",
                placeholder: "Ej: Harina, Azúcar, etc.");

            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var cantidadStr = await DisplayPromptAsync(
                "Cantidad",
                $"Ingrese la cantidad inicial de '{nombre}':",
                "Aceptar",
                "Cancelar",
                keyboard: Keyboard.Numeric,
                placeholder: "0");

            if (string.IsNullOrWhiteSpace(cantidadStr) || !double.TryParse(cantidadStr, out double cantidad))
            {
                await DisplayAlert("Error", "Debe ingresar una cantidad válida.", "OK");
                return;
            }

            var unidad = await DisplayActionSheet(
                "Seleccione la unidad:",
                "Cancelar",
                null,
                "kg", "gr", "litros", "ml", "unidades", "otra");

            if (unidad == "Cancelar" || string.IsNullOrEmpty(unidad))
                return;

            if (unidad == "otra")
            {
                unidad = await DisplayPromptAsync(
                    "Unidad personalizada",
                    "Ingrese la unidad:",
                    "Aceptar",
                    "Cancelar",
                    placeholder: "Ej: cucharadas, tazas, etc.");
            }

            if (string.IsNullOrWhiteSpace(unidad))
                return;

            var nuevoInsumo = new Insumo
            {
                Nombre = nombre,
                Cantidad = cantidad,
                Unidad = unidad
            };

            await _databaseService.AddInsumoAsync(nuevoInsumo);
            await CargarInsumos();
            await DisplayAlert("Éxito", $"Insumo '{nombre}' agregado correctamente.", "OK");
        }

        private async void OnEditarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Insumo insumo)
            {
                var nombre = await DisplayPromptAsync(
                    "Editar Nombre",
                    "Ingrese el nuevo nombre:",
                    "Aceptar",
                    "Cancelar",
                    initialValue: insumo.Nombre);

                if (string.IsNullOrWhiteSpace(nombre))
                    return;

                var cantidadStr = await DisplayPromptAsync(
                    "Editar Cantidad",
                    "Ingrese la nueva cantidad:",
                    "Aceptar",
                    "Cancelar",
                    keyboard: Keyboard.Numeric,
                    initialValue: insumo.Cantidad.ToString());

                if (string.IsNullOrWhiteSpace(cantidadStr) || !double.TryParse(cantidadStr, out double cantidad))
                {
                    await DisplayAlert("Error", "Debe ingresar una cantidad válida.", "OK");
                    return;
                }

                var unidad = await DisplayActionSheet(
                    "Seleccione la unidad:",
                    "Cancelar",
                    null,
                    "kg", "gr", "litros", "ml", "unidades", "otra");

                if (unidad == "Cancelar" || string.IsNullOrEmpty(unidad))
                    return;

                if (unidad == "otra")
                {
                    unidad = await DisplayPromptAsync(
                        "Unidad personalizada",
                        "Ingrese la unidad:",
                        "Aceptar",
                        "Cancelar",
                        initialValue: insumo.Unidad);
                }

                if (string.IsNullOrWhiteSpace(unidad))
                    return;

                insumo.Nombre = nombre;
                insumo.Cantidad = cantidad;
                insumo.Unidad = unidad;

                await _databaseService.UpdateInsumoAsync(insumo);
                await CargarInsumos();
                await DisplayAlert("Éxito", "Insumo actualizado correctamente.", "OK");
            }
        }

        private async void OnAgregarCantidadClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Insumo insumo)
            {
                var cantidadStr = await DisplayPromptAsync(
                    "Agregar Cantidad",
                    $"Ingrese la cantidad a agregar al insumo '{insumo.Nombre}' (cantidad actual: {insumo.Cantidad} {insumo.Unidad}):",
                    "Agregar",
                    "Cancelar",
                    keyboard: Keyboard.Numeric);

                if (!string.IsNullOrWhiteSpace(cantidadStr) && double.TryParse(cantidadStr, out double cantidad))
                {
                    await _databaseService.ActualizarCantidadInsumoAsync(insumo.Id, cantidad);
                    await CargarInsumos();
                    await DisplayAlert("Éxito", $"Se agregaron {cantidad} {insumo.Unidad} al inventario", "OK");
                }
            }
        }

        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Insumo insumo)
            {
                bool confirmar = await DisplayAlert(
                    "Confirmar Borrado",
                    $"¿Seguro que quieres eliminar el insumo '{insumo.Nombre}'?",
                    "Sí",
                    "No");

                if (confirmar)
                {
                    await _databaseService.DeleteInsumoAsync(insumo);
                    await CargarInsumos();
                    await DisplayAlert("Éxito", "Insumo eliminado correctamente.", "OK");
                }
            }
        }
    }
}

