using SaboresdeMama.Models;
using SaboresdeMama.Services;
using System.Linq;

namespace SaboresdeMama
{
    [QueryProperty(nameof(Pedido), "Pedido")]
    public partial class DetallePedidoPage : ContentPage
    {
        private Pedido _pedido;
        private readonly DatabaseService _databaseService;

        public Pedido Pedido
        {
            get => _pedido;
            set
            {
                _pedido = value;
                OnPropertyChanged();
                CargarDatosPedido();
            }
        }

        public DetallePedidoPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private void CargarDatosPedido()
        {
            if (Pedido != null)
            {
                ClienteLabel.Text = $"Cliente: {Pedido.ClienteNombre}";
                TelefonoLabel.Text = $"Teléfono: {Pedido.ClienteTelefono ?? "No especificado"}";
                DireccionLabel.Text = $"Dirección: {Pedido.ClienteDireccion ?? "No especificado"}";
                DescripcionLabel.Text = Pedido.DescripcionProducto;
                DetallesLabel.Text = Pedido.DetallesPedido ?? "Sin detalles adicionales";
                FechaCreacionLabel.Text = Pedido.FechaCreacion.ToString("dd/MM/yyyy HH:mm");
                FechaEntregaLabel.Text = Pedido.FechaEntrega.ToString("dd/MM/yyyy");
                EstadoLabel.Text = Pedido.Estado;
                
                // Color según el estado
                EstadoLabel.TextColor = Pedido.Estado switch
                {
                    "Pendiente" => Colors.Orange,
                    "Completado" => Colors.Green,
                    "Rechazado" => Colors.Red,
                    _ => Colors.Black
                };
                
                TotalLabel.Text = $"${Pedido.Total:F2}";

                // Mostrar botón de completar solo si el pedido está en estado "Aceptado"
                if (CompletarPedidoButton != null)
                {
                    CompletarPedidoButton.IsVisible = Pedido.Estado == "Aceptado";
                }
            }
        }

        private async void OnCompletarPedidoClicked(object sender, EventArgs e)
        {
            if (Pedido == null)
                return;

            bool confirmar = await DisplayAlert(
                "Confirmar Completado",
                $"¿Está seguro de completar el pedido de '{Pedido.ClienteNombre}'?\n\nSe restarán los insumos necesarios del inventario.",
                "Sí, Completar",
                "Cancelar");

            if (!confirmar)
                return;

            var (success, message) = await _databaseService.CompletarPedidoConVentaAsync(Pedido);
            if (!success)
            {
                await DisplayAlert("Error", message, "OK");
                return;
            }

            await DisplayAlert("Éxito", message, "OK");
            CargarDatosPedido();
        }
    }
}

