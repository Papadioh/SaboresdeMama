using SaboresdeMama.Models;
using SaboresdeMama.Services;
using System.Globalization; // <-- Necesario para el nombre del mes

namespace SaboresdeMama;

public partial class VentasPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public VentasPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadVentasAsync();
    }

    private async Task LoadVentasAsync()
    {
        // 1. Obtenemos TODAS las ventas de la base de datos
        var ventas = await _databaseService.GetVentasAsync();

        // 2. Mostramos el historial completo en la lista
        VentasCollectionView.ItemsSource = ventas;

        // ===============================================
        // ===== L�GICA PARA CALCULAR EL TOTAL DEL MES =====
        // ===============================================
        var now = DateTime.Now;

        // 3. Filtramos la lista para obtener solo las ventas de este mes Y este a�o
        var ventasDelMes = ventas
            .Where(v => v.FechaCompletado.Month == now.Month && v.FechaCompletado.Year == now.Year)
            .ToList();

        // 4. Sumamos el total de esas ventas
        double totalMes = ventasDelMes.Sum(v => v.Total);

        // 5. Mostramos el total en el Label
        // (Formateamos la fecha para que diga "Octubre", "Noviembre", etc.)
        string nombreMes = now.ToString("MMMM", new CultureInfo("es-ES"));
        nombreMes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombreMes); // Pone la primera letra en may�scula

        TotalMesLabel.Text = $"Total {nombreMes}: ${totalMes:F2}";
        // ===============================================
    }

    private async void OnBorrarVentaClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is Venta venta)
        {
            bool confirmar = await DisplayAlert("Confirmar Borrado", $"�Seguro que quieres borrar permanentemente esta venta? ({venta.DescripcionProducto})\n\nEsta acci�n no se puede deshacer.", "S�, Borrar", "Cancelar");
            if (confirmar)
            {
                await _databaseService.DeleteVentaAsync(venta);
                await LoadVentasAsync(); // Recargamos la lista Y el total del mes
            }
        }
    }
}