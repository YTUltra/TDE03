using Microsoft.Extensions.Logging;
using TDE03.Services;
using TDE03.View;

namespace TDE03;

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

        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<FirebaseService>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CadastroClientesPage>();
        builder.Services.AddTransient<CadastroProdutosPage>();
        builder.Services.AddTransient<CadastroComprasPage>();
        builder.Services.AddTransient<CadastroItensPage>();
        builder.Services.AddTransient<ConfiguracaoPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
