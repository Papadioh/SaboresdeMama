using SaboresdeMama.Models;

namespace SaboresdeMama
{
    [QueryProperty(nameof(Pedido), "Pedido")]
    public partial class DetallePedidoPage : ContentPage
    {
        private Pedido _pedido;

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
        }

        private void CargarDatosPedido()
        {
            if (Pedido != null)
            {
                ClienteLabel.Text = $"Cliente: {Pedido.ClienteNombre}";
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
            }
        }
    }
}

