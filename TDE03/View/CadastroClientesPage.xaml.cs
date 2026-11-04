using TDE03.Model;
using TDE03.Services;

namespace TDE03.View;

public partial class CadastroClientesPage : ContentPage
{
    private readonly FirebaseService _firebase;
    private List<Cliente> _clientes = new();

    public CadastroClientesPage(FirebaseService firebase)
    {
        InitializeComponent();
        _firebase = firebase;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarClientes();
    }

    private async Task CarregarClientes()
    {
        try
        {
            var dados = await _firebase.Listar<Cliente>("clientes");
            _clientes = dados.Select(item => { item.Value.Id = item.Key; return item.Value; }).ToList();
            lvClientes.ItemsSource = _clientes;
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        if (!LerFormulario(out var cliente))
        { await DisplayAlertAsync("Validação", "Preencha os campos. Nome: até 50 caracteres; RG: inteiro positivo com até 10 dígitos; telefone: até 14 caracteres.", "OK"); return; }
        string id = string.IsNullOrWhiteSpace(txID.Text) ? Guid.NewGuid().ToString("N") : txID.Text.Trim();
        try
        {
            cliente.Id = id;
            await _firebase.Salvar("clientes", id, cliente);
            await CarregarClientes();
            LimparFormulario();
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text) || !LerFormulario(out var cliente))
        { await DisplayAlertAsync("Validação", "Selecione um cliente e preencha os campos dentro dos limites.", "OK"); return; }
        try
        {
            string id = txID.Text.Trim();
            if (!await _firebase.Existe("clientes", id)) { await DisplayAlertAsync("Erro", "Cliente não encontrado.", "OK"); return; }
            cliente.Id = id;
            await _firebase.Salvar("clientes", id, cliente);
            await CarregarClientes();
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID do cliente.", "OK"); return; }
        try
        {
            string id = txID.Text.Trim();
            var compras = await _firebase.Listar<Compra>("compras");
            if (compras.Values.Any(c => c.CodigoCliente == id)) { await DisplayAlertAsync("Erro", "O cliente possui compras vinculadas.", "OK"); return; }
            await _firebase.Excluir("clientes", id);
            await CarregarClientes();
            LimparFormulario();
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async void btConsultar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID do cliente.", "OK"); return; }
        try
        {
            var cliente = await _firebase.Obter<Cliente>("clientes", txID.Text.Trim());
            if (cliente is null) { await DisplayAlertAsync("Consulta", "Cliente não encontrado.", "OK"); return; }
            cliente.Id = txID.Text.Trim();
            MostrarCliente(cliente);
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private void lvClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Cliente cliente) MostrarCliente(cliente);
    }

    private void MostrarCliente(Cliente cliente)
    {
        txID.Text = cliente.Id;
        txNome.Text = cliente.Nome;
        txRG.Text = cliente.Rg.ToString();
        txTelefone.Text = cliente.Telefone;
    }

    private bool LerFormulario(out Cliente cliente)
    {
        cliente = new Cliente();
        if (string.IsNullOrWhiteSpace(txNome.Text) || txNome.Text.Trim().Length > 50) return false;
        if (string.IsNullOrWhiteSpace(txTelefone.Text) || txTelefone.Text.Trim().Length > 14) return false;
        if (string.IsNullOrWhiteSpace(txRG.Text) || txRG.Text.Length > 10 || !int.TryParse(txRG.Text, out int rg) || rg <= 0) return false;
        cliente.Nome = txNome.Text.Trim();
        cliente.Rg = rg;
        cliente.Telefone = txTelefone.Text.Trim();
        return true;
    }

    private void sbBusca_TextChanged(object sender, TextChangedEventArgs e)
    {
        string busca = e.NewTextValue?.ToLower() ?? "";
        lvClientes.ItemsSource = _clientes.Where(c => c.Nome.ToLower().Contains(busca) || c.Id.ToLower().Contains(busca) || c.Rg.ToString().Contains(busca)).ToList();
    }

    private void btLimpar_Clicked(object sender, EventArgs e) => LimparFormulario();
    private void LimparFormulario() { txID.Text = ""; txNome.Text = ""; txRG.Text = ""; txTelefone.Text = ""; lvClientes.SelectedItem = null; }
}
