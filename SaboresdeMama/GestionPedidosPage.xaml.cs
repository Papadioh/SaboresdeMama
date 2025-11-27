using SaboresdeMama.Models;
using SaboresdeMama.Services;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaboresdeMama;

public partial class GestionPedidosPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public GestionPedidosPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await LoadPedidosAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando pedidos: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los pedidos. Verifique la conexión a Firebase.", "OK");
        }
    }

    private async Task LoadPedidosAsync()
    {
        try
        {
            var todosLosPedidos = await _databaseService.GetPedidosAsync();
            var pedidosPendientes = todosLosPedidos?
                                        .Where(p => p.Estado == "Pendiente" || p.Estado == "Aceptado")
                                        .OrderBy(p => p.FechaEntrega)
                                        .ToList();

            PedidosCollectionView.ItemsSource = pedidosPendientes ?? new List<Pedido>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en LoadPedidosAsync: {ex.Message}");
            PedidosCollectionView.ItemsSource = new List<Pedido>();
        }
    }

    private async void OnVerDetallesClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is Pedido pedido)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Pedido", pedido }
            };
            await Shell.Current.GoToAsync(nameof(DetallePedidoPage), navigationParameter);
        }
    }

    private async void OnCompletarClicked(object sender, EventArgs e)
    {
        try
        {
            if ((sender as Button)?.CommandParameter is Pedido pedido)
            {
                bool confirmar = await DisplayAlert(
                    "Confirmar Aceptación",
                    $"¿Aceptar el pedido de '{pedido.ClienteNombre}' y pasarlo a estado 'Aceptado' (En Preparación)?",
                    "Sí, Aceptar",
                    "No");

                if (confirmar)
                {
                    pedido.Estado = "Aceptado";
                    await _databaseService.UpdatePedidoAsync(pedido);

                    await LoadPedidosAsync();
                    await DisplayAlert("Éxito", "Pedido Aceptado. Ya puede ser preparado.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error aceptando pedido: {ex.Message}");
            await DisplayAlert("Error", "No se pudo aceptar el pedido. Verifique la conexión a Firebase.", "OK");
        }
    }

    private async void OnRechazarClicked(object sender, EventArgs e)
    {
        try
        {
            if ((sender as Button)?.CommandParameter is Pedido pedido)
            {
                bool confirmar = await DisplayAlert(
                    "Confirmar Rechazo",
                    $"¿Está seguro de rechazar el pedido de '{pedido.ClienteNombre}'?",
                    "Sí, Rechazar",
                    "Cancelar");

                if (confirmar)
                {
                    pedido.Estado = "Rechazado";
                    await _databaseService.UpdatePedidoAsync(pedido);
                    await LoadPedidosAsync();
                    await DisplayAlert("Pedido Rechazado", "El pedido ha sido rechazado", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error rechazando pedido: {ex.Message}");
            await DisplayAlert("Error", "No se pudo rechazar el pedido. Verifique la conexión a Firebase.", "OK");
        }
    }
}