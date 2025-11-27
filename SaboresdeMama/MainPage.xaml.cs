using System.Collections.Generic;
using System.Diagnostics;
using SaboresdeMama.Models;
using SaboresdeMama.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace SaboresdeMama
{
    // Asegúrate de que la clase sea 'public partial class'
    public partial class MainPage : ContentPage
    {
        private List<Pedido> _allPedidos = new();
        private readonly DatabaseService _databaseService;
        private bool _dateInitialized = false;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPedidosDataAsync();
            if (!_dateInitialized)
            {
                _dateInitialized = true;
                DatePickerPedidos.Date = DateTime.Today;
                LoadPedidosForSelectedDate(DateTime.Today);
            }
        }

        private async Task LoadPedidosDataAsync()
        {
            try
            {
                var pedidos = await _databaseService.GetPedidosAsync();
                _allPedidos = pedidos?
                    .Where(p => p.Estado == "Pendiente" || p.Estado == "Aceptado")
                    .OrderBy(p => p.FechaEntrega)
                    .ToList() ?? new List<Pedido>();

                LoadPedidosForSelectedDate(DatePickerPedidos.Date);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando pedidos para el panel: {ex.Message}");
                _allPedidos = new List<Pedido>();
                PedidosCollectionView.ItemsSource = new List<Pedido>();
                LabelPedidosHoy.Text = "No se pudieron cargar los pedidos.";
                LabelPedidosHoy.TextColor = Colors.Red;
            }
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            LoadPedidosForSelectedDate(e.NewDate);
        }

        private void LoadPedidosForSelectedDate(DateTime date)
        {
            var pedidosFiltrados = _allPedidos?
                .OrderBy(p => p.FechaEntrega)
                .ToList() ?? new List<Pedido>();

            PedidosCollectionView.ItemsSource = pedidosFiltrados;

            int pendientes = pedidosFiltrados.Count(p => p.Estado == "Pendiente");
            int aceptados = pedidosFiltrados.Count(p => p.Estado == "Aceptado");

            if (pendientes > 0)
            {
                LabelPedidosHoy.Text = $"⚠️ ¡ALERTA! {pendientes} pedidos pendientes de revisión.";
                LabelPedidosHoy.TextColor = Color.FromArgb("#F08080");
            }
            else if (aceptados > 0)
            {
                LabelPedidosHoy.Text = $"✅ {aceptados} pedidos aceptados para preparar.";
                LabelPedidosHoy.TextColor = Colors.DarkGreen;
            }
            else
            {
                LabelPedidosHoy.Text = "Todo al día para esta fecha. ✅";
                LabelPedidosHoy.TextColor = Colors.DarkGreen;
            }
        }

        private async void OnPedidoListoClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not Pedido pedido)
                return;

            var confirmar = await DisplayAlert("Pedido listo", $"¿Marcar como completado el pedido de {pedido.ClienteNombre}?", "Sí", "No");
            if (!confirmar)
                return;

            var (success, message) = await _databaseService.CompletarPedidoConVentaAsync(pedido);
            if (!success)
            {
                await DisplayAlert("Error", message, "OK");
                return;
            }

            await DisplayAlert("Éxito", message, "OK");
            await LoadPedidosDataAsync();
        }

        private async void OnPedidoNoCompletadoClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not Pedido pedido)
                return;

            var confirmar = await DisplayAlert("Revertir pedido", $"¿Marcar el pedido de {pedido.ClienteNombre} como no completado?", "Sí", "No");
            if (!confirmar)
                return;

            var (success, message) = await _databaseService.RevertirPedidoAAceptadoAsync(pedido);
            if (!success)
            {
                await DisplayAlert("Error", message, "OK");
                return;
            }

            await DisplayAlert("Información", message, "OK");
            await LoadPedidosDataAsync();
        }
    }
}