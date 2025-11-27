using Microsoft.Extensions.Logging;
using SaboresdeMama;
using SaboresdeMama.Services;

namespace SaboresdeMama
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            FirebaseService.Configure(
                projectId: "saboresdemama-5d8fa"
            );

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddTransient<GestionPedidosPage>();
            builder.Services.AddTransient<RecetarioPage>();
            builder.Services.AddTransient<AgregarRecetaPage>();
            builder.Services.AddTransient<RecetaDetallePage>();
            builder.Services.AddTransient<EditarRecetaPage>();
            builder.Services.AddTransient<GestionInsumosPage>();

            builder.Services.AddTransient<VentasPage>();

            return builder.Build();
        }
    }
}