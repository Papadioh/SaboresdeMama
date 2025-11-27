using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama;

public partial class MisPedidosPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private bool _isLoading;

    public MisPedidosPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPedidosAsync();
    }

    private async Task LoadPedidosAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;
            PedidosRefreshView.IsRefreshing = true;

            var usuario = AuthService.UsuarioActual;
            if (usuario == null)
            {
                await DisplayAlert("Sesión", "Debes iniciar sesión para ver tus pedidos.", "OK");
                MisPedidosCollectionView.ItemsSource = new List<Pedido>();
                return;
            }

            var pedidos = await _databaseService.GetPedidosAsync();
            var filtrados = pedidos?
                .Where(p =>
                    (!string.IsNullOrEmpty(p.ClienteId) && p.ClienteId == usuario.Id) ||
                    (string.IsNullOrEmpty(p.ClienteId) && p.ClienteNombre == usuario.Nombre))
                .OrderBy(p => p.FechaEntrega)
                .ToList() ?? new List<Pedido>();

            MisPedidosCollectionView.ItemsSource = filtrados;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los pedidos: {ex.Message}", "OK");
            MisPedidosCollectionView.ItemsSource = new List<Pedido>();
        }
        finally
        {
            _isLoading = false;
            PedidosRefreshView.IsRefreshing = false;
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadPedidosAsync();
    }
}

