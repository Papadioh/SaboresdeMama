using SaboresdeMama.Services;

namespace SaboresdeMama;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(GestionPedidosPage), typeof(GestionPedidosPage));
        Routing.RegisterRoute(nameof(RecetarioPage), typeof(RecetarioPage));
        Routing.RegisterRoute(nameof(AgregarRecetaPage), typeof(AgregarRecetaPage));
        Routing.RegisterRoute(nameof(RecetaDetallePage), typeof(RecetaDetallePage));
        Routing.RegisterRoute(nameof(EditarRecetaPage), typeof(EditarRecetaPage));
        Routing.RegisterRoute(nameof(VentasPage), typeof(VentasPage));
        Routing.RegisterRoute(nameof(ProductosPage), typeof(ProductosPage));
        Routing.RegisterRoute(nameof(CarritoPage), typeof(CarritoPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(GestionProductosPage), typeof(GestionProductosPage));
        Routing.RegisterRoute(nameof(AgregarProductoPage), typeof(AgregarProductoPage));
        Routing.RegisterRoute(nameof(EditarProductoPage), typeof(EditarProductoPage));
        Routing.RegisterRoute(nameof(DetallePedidoPage), typeof(DetallePedidoPage));

        ConfigurarMenu();
    }

    private void ConfigurarMenu()
    {
        try
        {
            if (AuthService.EsAdmin)
            {
                InicioMenu.IsVisible = true;
                GestionProductosMenu.IsVisible = true;
                GestionPedidosMenu.IsVisible = true;
                RecetasMenu.IsVisible = true;
                VentasMenu.IsVisible = true;
                ProductosMenu.IsVisible = false;
                CurrentItem = InicioMenu;
                UsuarioLabel.Text = $"Usuario: {AuthService.UsuarioActual?.Nombre ?? "Admin"}";
            }
            else if (AuthService.EsCliente)
            {
                InicioMenu.IsVisible = false;
                GestionProductosMenu.IsVisible = false;
                GestionPedidosMenu.IsVisible = false;
                RecetasMenu.IsVisible = false;
                VentasMenu.IsVisible = false;
                ProductosMenu.IsVisible = true;
                CurrentItem = ProductosMenu;
                UsuarioLabel.Text = $"Usuario: {AuthService.UsuarioActual?.Nombre ?? "Cliente"}";
            }
            else
            {
                Application.Current!.MainPage = new LoginPage();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error configurando menú: {ex.Message}");
            Application.Current!.MainPage = new LoginPage();
        }
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            var authService = new AuthService(new DatabaseService());
            authService.Logout();
            
            if (Application.Current?.Windows != null && Application.Current.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = new LoginPage();
            }
            else
            {
                Application.Current!.MainPage = new LoginPage();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en logout: {ex.Message}");
            Application.Current!.MainPage = new LoginPage();
        }
    }
}