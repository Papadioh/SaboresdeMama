using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using SaboresdeMama.Models; // <-- ¡ESTO RESUELVE EL ERROR DE 'Pedido' y 'Producto'!
using SaboresdeMama.Services; // <-- ¡ESTO RESUELVE EL ERROR DE 'DatabaseService'!
using System; // Necesario para DateTime y Guid
using System.Linq; // Necesario para Where y OrderBy

namespace SaboresdeMama
{
    // Asegúrate de que la clase sea 'public partial class'
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<Pedido> _allPedidos;
        private readonly DatabaseService _databaseService;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();

            // Usaremos datos simulados hasta que tengamos la lógica de carga completa
            LoadPedidosData();

            DatePickerPedidos.Date = DateTime.Today;
            LoadPedidosForSelectedDate(DateTime.Today);
        }

        private void LoadPedidosData()
        {
            // Datos simulados (reemplazar con datos reales cargados desde su servicio)
            _allPedidos = new ObservableCollection<Pedido>
            {
                new Pedido
                {
                    Id = Guid.NewGuid().ToString(),
                    ClienteNombre = "Andrea Guzmán",
                    DescripcionProducto = "3x Queque de Naranja y 1x Mermelada",
                    FechaEntrega = DateTime.Today.AddHours(10).AddMinutes(30),
                    FechaCreacion = DateTime.Today.AddDays(-1),
                    Estado = "Pendiente",
                    Total = 15000.00
                },
                new Pedido
                {
                    Id = Guid.NewGuid().ToString(),
                    ClienteNombre = "Carlos Soto (Mayorista)",
                    DescripcionProducto = "50x Alfajores Clásicos",
                    FechaEntrega = DateTime.Today.AddDays(1).AddHours(15),
                    FechaCreacion = DateTime.Today,
                    Estado = "Aceptado",
                    Total = 150000.00
                },
                new Pedido
                {
                    Id = Guid.NewGuid().ToString(),
                    ClienteNombre = "Fabiola Reyes",
                    DescripcionProducto = "Torta de Chocolate y Maní",
                    FechaEntrega = DateTime.Today.AddHours(18).AddMinutes(0),
                    FechaCreacion = DateTime.Today,
                    Estado = "Pendiente",
                    Total = 25000.00
                },
            };
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            LoadPedidosForSelectedDate(e.NewDate);
        }

        private void LoadPedidosForSelectedDate(DateTime date)
        {
            var pedidosFiltrados = _allPedidos
                .Where(p => p.FechaEntrega.Date == date.Date)
                .OrderBy(p => p.FechaEntrega)
                .ToList();

            PedidosCollectionView.ItemsSource = pedidosFiltrados;

            int pendientesHoy = pedidosFiltrados.Count(p => p.Estado == "Pendiente");

            if (pendientesHoy > 0)
            {
                LabelPedidosHoy.Text = $"⚠️ ¡ALERTA! {pendientesHoy} pedidos pendientes de revisión.";
                LabelPedidosHoy.TextColor = Color.FromArgb("#F08080");
            }
            else
            {
                LabelPedidosHoy.Text = "Todo al día para esta fecha. ✅";
                LabelPedidosHoy.TextColor = Colors.DarkGreen;
            }
        }
    }
}