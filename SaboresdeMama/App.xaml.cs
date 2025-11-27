using System.Globalization;

namespace SaboresdeMama;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Configuración de idioma (que ya tenías)
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-CL");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-CL");

        // ================================================================
        // CORRECCIÓN AQUÍ: Envolvemos LoginPage en una NavigationPage
        // ================================================================
        MainPage = new NavigationPage(new LoginPage());
    }
}