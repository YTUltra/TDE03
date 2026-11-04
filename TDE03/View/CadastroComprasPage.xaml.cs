using System.Globalization;
using TDE03.Model;
using TDE03.Services;

namespace TDE03.View;

public partial class CadastroComprasPage : ContentPage
{
    private readonly FirebaseService _firebase;
    private List<Compra> _compras = new();

    public CadastroComprasPage(FirebaseService firebase) { InitializeComponent(); _firebase = firebase; txData.Text = DateTime.Today.ToString("yyyy-MM-dd"); }
    protected override async void OnAppearing() { base.OnAppearing(); await CarregarCompras(); }

    private async Task CarregarCompras()
    {
        try { var dados = await _firebase.Listar<Compra>("compras"); _compras = dados.Select(x => { x.Value.Id = x.Key; return x.Value; }).ToList(); lvCompras.ItemsSource = _compras; }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private bool LerFormulario(out Compra compra)
    {
        compra = new Compra();
        if (string.IsNullOrWhiteSpace(txCliente.Text) || !DateTime.TryParse(txData.Text, out var data)) return false;
        compra.DataCompra = data; compra.CodigoCliente = txCliente.Text.Trim(); return true;
    }

    private async Task SalvarCompra(string id)
    {
        if (!LerFormulario(out var compra)) { await DisplayAlertAsync("Validação", "Informe uma data válida e o código do cliente.", "OK"); return; }
        if (!await _firebase.Existe("clientes", compra.CodigoCliente)) { await DisplayAlertAsync("Validação", "O cliente informado não existe.", "OK"); return; }
        var itens = await _firebase.Listar<ItemCompra>("itensCompra");
        compra.Id = id; compra.ValorTotal = itens.Values.Where(i => i.CodigoCompra == id).Sum(i => i.ValorParcial);
        await _firebase.Salvar("compras", id, compra);
        await CarregarCompras();
        lbTotal.Text = $"Total calculado pelos itens: {compra.ValorTotal.ToString("C", CultureInfo.CurrentCulture)}";
    }

    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        string id = string.IsNullOrWhiteSpace(txID.Text) ? Guid.NewGuid().ToString("N") : txID.Text.Trim();
        try { await SalvarCompra(id); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID da compra.", "OK"); return; }
        try { string id = txID.Text.Trim(); if (!await _firebase.Existe("compras", id)) { await DisplayAlertAsync("Erro", "Compra não encontrada.", "OK"); return; } await SalvarCompra(id); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID da compra.", "OK"); return; }
        try
        {
            string id = txID.Text.Trim();
            var itens = await _firebase.Listar<ItemCompra>("itensCompra");
            foreach (var item in itens.Where(x => x.Value.CodigoCompra == id)) await _firebase.Excluir("itensCompra", item.Key);
            await _firebase.Excluir("compras", id); await CarregarCompras(); LimparFormulario();
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private async void btConsultar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID da compra.", "OK"); return; }
        try { string id = txID.Text.Trim(); var compra = await _firebase.Obter<Compra>("compras", id); if (compra is null) { await DisplayAlertAsync("Consulta", "Compra não encontrada.", "OK"); return; } compra.Id = id; MostrarCompra(compra); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private void lvCompras_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (e.CurrentSelection.FirstOrDefault() is Compra compra) MostrarCompra(compra); }
    private void MostrarCompra(Compra compra) { txID.Text = compra.Id; txData.Text = compra.DataCompra.ToString("yyyy-MM-dd"); txCliente.Text = compra.CodigoCliente; lbTotal.Text = $"Total calculado pelos itens: {compra.ValorTotal.ToString("C", CultureInfo.CurrentCulture)}"; }
    private void sbBusca_TextChanged(object sender, TextChangedEventArgs e) { string busca = e.NewTextValue?.ToLower() ?? ""; lvCompras.ItemsSource = _compras.Where(c => c.Id.ToLower().Contains(busca) || c.CodigoCliente.ToLower().Contains(busca) || c.DataCompra.ToString("yyyy-MM-dd").Contains(busca)).ToList(); }
    private void btLimpar_Clicked(object sender, EventArgs e) => LimparFormulario();
    private void LimparFormulario() { txID.Text = ""; txData.Text = DateTime.Today.ToString("yyyy-MM-dd"); txCliente.Text = ""; lbTotal.Text = "Total calculado pelos itens: R$ 0,00"; lvCompras.SelectedItem = null; }
}
