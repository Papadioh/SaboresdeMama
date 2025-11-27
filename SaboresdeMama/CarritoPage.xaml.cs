using SaboresdeMama.Models;
using SaboresdeMama.Services;
using Microsoft.Maui.Graphics;

namespace SaboresdeMama
{
    public partial class CarritoPage : ContentPage
    {
        private List<CarritoItem> _carrito;
        private readonly DatabaseService _databaseService;

        public CarritoPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            InicializarFechaEntrega();
        }

        private void InicializarFechaEntrega()
        {
            // Establecer la fecha mínima como 7 días desde hoy
            var fechaMinima = DateTime.Now.AddDays(7);
            FechaEntregaPicker.MinimumDate = fechaMinima;
            // Establecer la fecha por defecto como la fecha mínima
            FechaEntregaPicker.Date = fechaMinima;
            ActualizarLabelFecha();
        }

        private void OnFechaEntregaSelected(object sender, DateChangedEventArgs e)
        {
            ActualizarLabelFecha();
        }

        private void ActualizarLabelFecha()
        {
            var fechaSeleccionada = FechaEntregaPicker.Date;
            var diasRestantes = (fechaSeleccionada - DateTime.Now).Days;
            
            if (diasRestantes >= 7)
            {
                FechaSeleccionadaLabel.Text = $"Entrega programada para: {fechaSeleccionada:dd/MM/yyyy} ({diasRestantes} días)";
                FechaSeleccionadaLabel.TextColor = Color.FromArgb("#B19CD9"); // Primary pastel
                FechaSeleccionadaLabel.IsVisible = true;
            }
            else
            {
                FechaSeleccionadaLabel.Text = "⚠ La fecha debe ser al menos 7 días desde hoy";
                FechaSeleccionadaLabel.TextColor = Color.FromArgb("#FFD1DC"); // PastelPink
                FechaSeleccionadaLabel.IsVisible = true;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _carrito = ProductosPage.GetCarrito();
            await CargarProductos();
            CargarCarrito();
        }

        private async Task CargarProductos()
        {
            foreach (var item in _carrito)
            {
                // Preservar el ID original antes de actualizar
                var idOriginal = item.Producto.Id;
                
                if (!string.IsNullOrEmpty(idOriginal))
                {
                    var productoActual = await _databaseService.GetProductoByIdAsync(idOriginal);
                    if (productoActual != null)
                    {
                        // Asegurar que el ID se preserve
                        productoActual.Id = idOriginal;
                        item.Producto = productoActual;
                    }
                    else
                    {
                        // Si no se encuentra, asegurar que el ID se mantenga
                        if (string.IsNullOrEmpty(item.Producto.Id))
                        {
                            item.Producto.Id = idOriginal;
                        }
                        System.Diagnostics.Debug.WriteLine($"Advertencia: No se encontró producto con ID: {idOriginal}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Producto en carrito sin ID: {item.Producto.Nombre}");
                }
            }
        }

        private void CargarCarrito()
        {
            CarritoCollectionView.ItemsSource = null;
            CarritoCollectionView.ItemsSource = _carrito;
            ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            var total = _carrito.Sum(item => item.Total);
            TotalLabel.Text = $"${total:F2}";
            ComprarButton.IsEnabled = _carrito.Count > 0;
        }

        private async void OnIncrementarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is CarritoItem item)
            {
                var productoActual = await _databaseService.GetProductoByIdAsync(item.Producto.Id)
                                     ?? item.Producto;

                if (productoActual != null && item.Cantidad >= productoActual.Stock)
                {
                    await DisplayAlert("Error", $"No hay suficiente stock. Disponible: {productoActual.Stock}", "OK");
                    return;
                }
                item.Cantidad++;
                CargarCarrito();
            }
        }

        private void OnDecrementarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is CarritoItem item)
            {
                if (item.Cantidad > 1)
                {
                    item.Cantidad--;
                }
                else
                {
                    _carrito.Remove(item);
                }
                CargarCarrito();
            }
        }

        private async void OnComprarClicked(object sender, EventArgs e)
        {
            if (_carrito.Count == 0)
            {
                await DisplayAlert("Error", "El carrito está vacío", "OK");
                return;
            }

            // Validar que se haya seleccionado una fecha de entrega válida
            var fechaEntrega = FechaEntregaPicker.Date;
            var diasRestantes = (fechaEntrega - DateTime.Now).Days;

            if (diasRestantes < 7)
            {
                await DisplayAlert("Error", 
                    $"La fecha de entrega debe ser al menos 7 días desde hoy.\n" +
                    $"Fecha seleccionada: {fechaEntrega:dd/MM/yyyy}\n" +
                    $"Días restantes: {diasRestantes}", 
                    "OK");
                return;
            }

            var total = _carrito.Sum(item => item.Total);
            var confirmacion = await DisplayAlert(
                "Confirmar Compra",
                $"¿Desea realizar la compra por un total de ${total:F2}?\n\n" +
                $"Fecha de entrega: {fechaEntrega:dd/MM/yyyy}",
                "Sí",
                "No");

            if (confirmacion)
            {
                var usuarioActual = AuthService.UsuarioActual;
                if (usuarioActual == null)
                {
                    await DisplayAlert("Sesión requerida", "Debes iniciar sesión para realizar un pedido.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(usuarioActual.Telefono) || string.IsNullOrWhiteSpace(usuarioActual.Direccion))
                {
                    bool irPerfil = await DisplayAlert(
                        "Perfil incompleto",
                        "Necesitas registrar tu dirección y teléfono para completar el pedido.",
                        "Ir a mi perfil",
                        "Cancelar");

                    if (irPerfil)
                    {
                        await Shell.Current.GoToAsync(nameof(PerfilClientePage));
                    }
                    return;
                }

                var nombreCliente = usuarioActual.Nombre ?? "Cliente";
                var clienteId = usuarioActual.Id;

                foreach (var item in _carrito)
                {
                    var productoActual = await _databaseService.GetProductoByIdAsync(item.Producto.Id)
                                           ?? item.Producto;

                    if (productoActual == null || productoActual.Stock < item.Cantidad)
                    {
                        var stockDisponible = productoActual?.Stock ?? item.Producto.Stock;
                        await DisplayAlert("Error", $"No hay suficiente stock para {item.Producto.Nombre}. Stock disponible: {stockDisponible}", "OK");
                        await CargarProductos();
                        return;
                    }
                }

                var detallesPedido = string.Join("\n", _carrito.Select(item => 
                    $"{item.Producto.Nombre} x{item.Cantidad} - ${item.Total:F2}"));

                var descripcionProducto = string.Join(", ", _carrito.Select(item => 
                    $"{item.Producto.Nombre} x{item.Cantidad}"));

                var nuevoPedido = new Pedido
                {
                    ClienteId = clienteId,
                    ClienteNombre = nombreCliente,
                    ClienteTelefono = usuarioActual.Telefono,
                    ClienteDireccion = usuarioActual.Direccion,
                    DescripcionProducto = descripcionProducto,
                    DetallesPedido = detallesPedido,
                    Total = total,
                    Estado = "Pendiente",
                    FechaCreacion = DateTime.Now,
                    FechaEntrega = fechaEntrega // Usar la fecha seleccionada por el cliente
                };

                await _databaseService.AddPedidoAsync(nuevoPedido);

                // Actualizar el stock de cada producto en el carrito
                bool stockActualizado = true;
                var erroresStock = new List<string>();

                foreach (var item in _carrito)
                {
                    try
                    {
                        // Verificar que el producto tenga un ID válido
                        if (string.IsNullOrEmpty(item.Producto.Id))
                        {
                            erroresStock.Add($"El producto {item.Producto.Nombre} no tiene un ID válido");
                            stockActualizado = false;
                            System.Diagnostics.Debug.WriteLine($"ERROR: Producto sin ID: {item.Producto.Nombre}");
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"Actualizando stock para: {item.Producto.Nombre} (ID: {item.Producto.Id}), Cantidad a reducir: {item.Cantidad}");

                        // Obtener el producto actualizado de la base de datos
                        var productoActual = await _databaseService.GetProductoByIdAsync(item.Producto.Id);
                        
                        if (productoActual == null)
                        {
                            erroresStock.Add($"No se encontró el producto: {item.Producto.Nombre} (ID: {item.Producto.Id})");
                            stockActualizado = false;
                            System.Diagnostics.Debug.WriteLine($"ERROR: No se encontró producto con ID: {item.Producto.Id}");
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"Producto encontrado: {productoActual.Nombre}, Stock actual: {productoActual.Stock}");

                        // Verificar que el stock sea suficiente antes de actualizar
                        if (productoActual.Stock < item.Cantidad)
                        {
                            erroresStock.Add($"Stock insuficiente para {item.Producto.Nombre}. Disponible: {productoActual.Stock}, Solicitado: {item.Cantidad}");
                            stockActualizado = false;
                            System.Diagnostics.Debug.WriteLine($"ERROR: Stock insuficiente - Disponible: {productoActual.Stock}, Solicitado: {item.Cantidad}");
                            continue;
                        }

                        // Reducir el stock en la cantidad comprada (cantidad negativa)
                        var resultado = await _databaseService.ActualizarStockAsync(item.Producto.Id, -item.Cantidad);
                        
                        if (!resultado)
                        {
                            erroresStock.Add($"No se pudo actualizar el stock de {item.Producto.Nombre}");
                            stockActualizado = false;
                            System.Diagnostics.Debug.WriteLine($"ERROR: No se pudo actualizar el stock de {item.Producto.Nombre}");
                        }
                        else
                        {
                            // Verificar que el stock se actualizó correctamente
                            var productoVerificado = await _databaseService.GetProductoByIdAsync(item.Producto.Id);
                            if (productoVerificado != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"✓ Stock actualizado correctamente: {item.Producto.Nombre} - Stock anterior: {productoActual.Stock}, Stock nuevo: {productoVerificado.Stock}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN al actualizar stock para {item.Producto.Nombre}: {ex.Message}\n{ex.StackTrace}");
                        erroresStock.Add($"Error al actualizar {item.Producto.Nombre}: {ex.Message}");
                        stockActualizado = false;
                    }
                }

                // Si hubo errores al actualizar el stock, informar al usuario
                if (!stockActualizado && erroresStock.Count > 0)
                {
                    var mensajeError = "El pedido se creó, pero hubo problemas al actualizar el stock:\n\n" + 
                                      string.Join("\n", erroresStock);
                    await DisplayAlert("Advertencia", mensajeError, "OK");
                }

                ProductosPage.LimpiarCarrito();
                _carrito.Clear();

                await DisplayAlert("Éxito", "Pedido realizado exitosamente. El stock ha sido actualizado. El administrador lo revisará pronto.", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}

