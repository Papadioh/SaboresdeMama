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

            try
            {
                // Buscar la receta asociada al pedido
                Receta receta = null;
                if (!string.IsNullOrEmpty(Pedido.RecetaId))
                {
                    var todasRecetas = await _databaseService.GetRecetasAsync();
                    receta = todasRecetas.FirstOrDefault(r => r.Id == Pedido.RecetaId);
                }

                // Si no se encuentra por ID, intentar buscar por nombre del producto
                if (receta == null && !string.IsNullOrEmpty(Pedido.DescripcionProducto))
                {
                    var todasRecetas = await _databaseService.GetRecetasAsync();
                    receta = todasRecetas.FirstOrDefault(r => 
                        r.Nombre?.ToLower().Contains(Pedido.DescripcionProducto.ToLower()) == true ||
                        Pedido.DescripcionProducto.ToLower().Contains(r.Nombre?.ToLower() ?? ""));
                }

                // Si hay una receta asociada, verificar y restar insumos
                if (receta != null && receta.InsumosNecesarios != null && receta.InsumosNecesarios.Count > 0)
                {
                    var (disponible, insumosFaltantes) = await _databaseService.VerificarDisponibilidadInsumosAsync(receta);
                    
                    if (!disponible)
                    {
                        await DisplayAlert(
                            "Insumos Insuficientes",
                            $"No se puede completar el pedido. Faltan los siguientes insumos:\n\n{string.Join("\n", insumosFaltantes)}",
                            "OK");
                        return;
                    }

                    // Restar insumos del inventario
                    bool restado = await _databaseService.RestarInsumosDeRecetaAsync(receta);
                    if (!restado)
                    {
                        await DisplayAlert("Error", "No se pudieron restar los insumos del inventario.", "OK");
                        return;
                    }
                }

                // Cambiar estado a Completado
                Pedido.Estado = "Completado";
                await _databaseService.UpdatePedidoAsync(Pedido);

                // Crear una venta
                var venta = new Venta
                {
                    DescripcionProducto = Pedido.DescripcionProducto,
                    ClienteNombre = Pedido.ClienteNombre,
                    Total = Pedido.Total,
                    FechaCompletado = DateTime.Now
                };
                await _databaseService.AddVentaAsync(venta);

                await DisplayAlert("Éxito", "Pedido completado correctamente. Los insumos han sido restados del inventario.", "OK");
                
                // Actualizar la vista
                CargarDatosPedido();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo completar el pedido: {ex.Message}", "OK");
            }
        }
    }
}

