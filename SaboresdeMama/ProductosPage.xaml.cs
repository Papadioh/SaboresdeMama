using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama
{
    public partial class ProductosPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private List<Producto> _todosLosProductos;

        private static Dictionary<string, List<CarritoItem>> _carritosPorUsuario = new Dictionary<string, List<CarritoItem>>();

        public ProductosPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _todosLosProductos = new List<Producto>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarProductos();
        }

        private async Task CargarProductos()
        {
            _todosLosProductos = await _databaseService.GetProductosDisponiblesAsync();
            var productosSinId = _todosLosProductos.Where(p => string.IsNullOrEmpty(p.Id)).ToList();
            if (productosSinId.Any())
            {
                System.Diagnostics.Debug.WriteLine($"ADVERTENCIA: Se encontraron {productosSinId.Count} productos sin ID:");
                foreach (var p in productosSinId)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {p.Nombre}");
                }
            }
            
            ProductosCollectionView.ItemsSource = _todosLosProductos;
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
            productos = productos.Where(p => p.Stock > 0).ToList();
            ProductosCollectionView.ItemsSource = productos;
        }

        private async void OnVerDetallesClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Producto producto)
            {
                var detalles = $"Nombre: {producto.Nombre}\n\n" +
                              $"Descripción: {producto.Descripcion}\n\n" +
                              $"Precio: ${producto.Precio:F2}\n\n" +
                              $"Stock disponible: {producto.Stock}\n\n" +
                              $"Categoría: {producto.Categoria ?? "Sin categoría"}\n\n" +
                              $"Información Extra: {producto.InformacionExtra ?? "N/A"}";

                await DisplayAlert("Detalles del Producto", detalles, "OK");
            }
        }

        private void OnAgregarAlCarritoClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Producto producto)
            {
                if (string.IsNullOrEmpty(producto.Id))
                {
                    DisplayAlert("Error", $"El producto '{producto.Nombre}' no tiene un ID válido. Por favor, recargue la página.", "OK");
                    System.Diagnostics.Debug.WriteLine($"ERROR: Producto sin ID al agregar al carrito: {producto.Nombre}");
                    return;
                }

                if (producto.Stock <= 0)
                {
                    DisplayAlert("Error", "Este producto no está disponible en stock", "OK");
                    return;
                }

                var carritoActual = GetCarritoDelUsuarioActual();

                var itemExistente = carritoActual.FirstOrDefault(c => c.Producto.Id == producto.Id);

                if (itemExistente != null)
                {
                    if (itemExistente.Cantidad >= producto.Stock)
                    {
                        DisplayAlert("Error", $"No hay suficiente stock. Disponible: {producto.Stock}", "OK");
                        return;
                    }
                    itemExistente.Cantidad++;
                }
                else
                {
                    var productoParaCarrito = new Producto
                    {
                        Id = producto.Id,
                        Nombre = producto.Nombre,
                        Descripcion = producto.Descripcion,
                        Precio = producto.Precio,
                        Stock = producto.Stock,
                        InformacionExtra = producto.InformacionExtra,
                        Categoria = producto.Categoria,
                        ImageURL = producto.ImageURL,
                        FechaCreacion = producto.FechaCreacion
                    };

                    carritoActual.Add(new CarritoItem
                    {
                        Producto = productoParaCarrito,
                        Cantidad = 1
                    });
                }

                System.Diagnostics.Debug.WriteLine($"Producto agregado al carrito: {producto.Nombre} (ID: {producto.Id})");
                DisplayAlert("Éxito", $"{producto.Nombre} agregado al carrito", "OK");
            }
        }

        private async void OnCarritoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CarritoPage));
        }

        private static List<CarritoItem> GetCarritoDelUsuarioActual()
        {
            var userId = AuthService.UsuarioActual?.Id ?? "invitado";
            if (!_carritosPorUsuario.ContainsKey(userId))
            {
                _carritosPorUsuario[userId] = new List<CarritoItem>();
            }

            return _carritosPorUsuario[userId];
        }

        public static List<CarritoItem> GetCarrito()
        {
            return GetCarritoDelUsuarioActual();
        }

        public static void LimpiarCarrito()
        {
            var userId = AuthService.UsuarioActual?.Id ?? "invitado";
            if (_carritosPorUsuario.ContainsKey(userId))
            {
                _carritosPorUsuario[userId].Clear();
            }
        }
    }
}