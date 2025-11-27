using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama
{
    public partial class GestionProductosPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public GestionProductosPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarProductos();
        }

        private async Task CargarProductos()
        {
            var productos = await _databaseService.GetProductosAsync();
            ProductosCollectionView.ItemsSource = productos;
        }

        private async void OnSearchButtonPressed(object sender, EventArgs e)
        {
            await BuscarProductos();
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            await BuscarProductos();
        }

        private async Task BuscarProductos()
        {
            var searchTerm = ProductoSearchBar.Text;
            var productos = await _databaseService.SearchProductosAsync(searchTerm);
            ProductosCollectionView.ItemsSource = productos;
        }

        private async void OnAddProductoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AgregarProductoPage));
        }

        private async void OnEditarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Producto producto)
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "Producto", producto }
                };
                await Shell.Current.GoToAsync(nameof(EditarProductoPage), navigationParameter);
            }
        }

        private async void OnGestionarStockClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Producto producto)
            {
                var action = await DisplayActionSheet(
                    $"Gestionar Stock: {producto.Nombre}",
                    "Cancelar",
                    null,
                    "Agregar Stock",
                    "Reducir Stock",
                    "Ver Detalles");

                if (action == "Agregar Stock")
                {
                    var cantidad = await DisplayPromptAsync(
                        "Agregar Stock",
                        $"Ingrese la cantidad a agregar al stock actual ({producto.Stock}):",
                        "Agregar",
                        "Cancelar",
                        keyboard: Keyboard.Numeric);

                    if (!string.IsNullOrWhiteSpace(cantidad) && int.TryParse(cantidad, out int cant))
                    {
                        await _databaseService.ActualizarStockAsync(producto.Id, cant);
                        await CargarProductos();
                        await DisplayAlert("Éxito", $"Se agregaron {cant} unidades al stock", "OK");
                    }
                }
                else if (action == "Reducir Stock")
                {
                    var cantidad = await DisplayPromptAsync(
                        "Reducir Stock",
                        $"Ingrese la cantidad a reducir del stock actual ({producto.Stock}):",
                        "Reducir",
                        "Cancelar",
                        keyboard: Keyboard.Numeric);

                    if (!string.IsNullOrWhiteSpace(cantidad) && int.TryParse(cantidad, out int cant))
                    {
                        await _databaseService.ActualizarStockAsync(producto.Id, -cant);
                        await CargarProductos();
                        await DisplayAlert("Éxito", $"Se redujeron {cant} unidades del stock", "OK");
                    }
                }
                else if (action == "Ver Detalles")
                {
                    await MostrarDetallesProducto(producto);
                }
            }
        }

        private async Task MostrarDetallesProducto(Producto producto)
        {
            var detalles = $"Nombre: {producto.Nombre}\n\n" +
                          $"Descripción: {producto.Descripcion}\n\n" +
                          $"Precio: ${producto.Precio:F2}\n\n" +
                          $"Stock: {producto.Stock}\n\n" +
                          $"Categoría: {producto.Categoria ?? "Sin categoría"}\n\n" +
                          $"Información Extra: {producto.InformacionExtra ?? "N/A"}\n\n" +
                          $"Fecha Creación: {producto.FechaCreacion:dd/MM/yyyy}";

            await DisplayAlert("Detalles del Producto", detalles, "OK");
        }

        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Producto producto)
            {
                bool confirmar = await DisplayAlert(
                    "Confirmar Borrado",
                    $"¿Seguro que quieres eliminar el producto '{producto.Nombre}'?",
                    "Sí",
                    "No");

                if (confirmar)
                {
                    await _databaseService.DeleteProductoAsync(producto);
                    await CargarProductos();
                }
            }
        }
    }
}

