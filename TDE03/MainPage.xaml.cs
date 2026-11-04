using TDE03.Services;
using TDE03.View;

namespace TDE03;

public partial class MainPage : ContentPage
{
    private readonly FirebaseService _firebase;

    public MainPage(FirebaseService firebase)
    {
        InitializeComponent();
        _firebase = firebase;
    }

    private async void btClientes_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new CadastroClientesPage(_firebase));
    private async void btProdutos_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new CadastroProdutosPage(_firebase));
    private async void btCompras_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new CadastroComprasPage(_firebase));
    private async void btItens_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new CadastroItensPage(_firebase));
    private async void btConfiguracao_Clicked(object sender, EventArgs e) => await Navigation.PushAsync(new ConfiguracaoPage());
}
