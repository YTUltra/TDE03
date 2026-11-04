namespace TDE03.View;

public partial class ConfiguracaoPage : ContentPage
{
    public ConfiguracaoPage()
    {
        InitializeComponent();
        txUrl.Text = Preferences.Default.Get("FirebaseDatabaseUrl", string.Empty);
    }

    private async void btSalvar_Clicked(object sender, EventArgs e)
    {
        var url = txUrl.Text?.Trim().TrimEnd('/') ?? string.Empty;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            await DisplayAlert("Validação", "Informe uma URL HTTPS válida do Firebase Realtime Database.", "OK");
            return;
        }
        Preferences.Default.Set("FirebaseDatabaseUrl", url);
        await DisplayAlert("Sucesso", "URL salva neste dispositivo.", "OK");
    }
}
