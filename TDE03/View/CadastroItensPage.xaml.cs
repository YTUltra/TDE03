using System.Globalization;
using TDE03.Model;
using TDE03.Services;

namespace TDE03.View;

public partial class CadastroItensPage : ContentPage
{
    private readonly FirebaseService _firebase;
    private List<ItemCompra> _itens = new();

    public CadastroItensPage(FirebaseService firebase) { InitializeComponent(); _firebase = firebase; }
    protected override async void OnAppearing() { base.OnAppearing(); await CarregarItens(); }

    private async Task CarregarItens()
    {
        try { var dados = await _firebase.Listar<ItemCompra>("itensCompra"); _itens = dados.Select(x => { x.Value.Id = x.Key; return x.Value; }).ToList(); lvItens.ItemsSource = _itens; }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async Task<bool> SalvarItem(string id)
    {
        if (!int.TryParse(txQuantidade.Text, out int quantidade) || quantidade <= 0 || string.IsNullOrWhiteSpace(txProduto.Text) || string.IsNullOrWhiteSpace(txCompra.Text))
        { await DisplayAlertAsync("Validação", "Informe produto, compra e uma quantidade maior que zero.", "OK"); return false; }
        string codigoProduto = txProduto.Text.Trim();
        string codigoCompra = txCompra.Text.Trim();
        if (!await _firebase.Existe("produtos", codigoProduto)) { await DisplayAlertAsync("Validação", "O produto informado não existe.", "OK"); return false; }
        if (!await _firebase.Existe("compras", codigoCompra)) { await DisplayAlertAsync("Validação", "A compra informada não existe.", "OK"); return false; }
        var produto = await _firebase.Obter<Produto>("produtos", codigoProduto);
        string? compraAntiga = (await _firebase.Obter<ItemCompra>("itensCompra", id))?.CodigoCompra;
        var item = new ItemCompra { Id = id, CodigoProduto = codigoProduto, CodigoCompra = codigoCompra, Quantidade = quantidade, ValorUnitario = produto!.Preco, ValorParcial = quantidade * produto.Preco };
        await _firebase.Salvar("itensCompra", id, item);
        await AtualizarTotal(codigoCompra);
        if (!string.IsNullOrWhiteSpace(compraAntiga) && compraAntiga != codigoCompra) await AtualizarTotal(compraAntiga);
        await CarregarItens();
        lbValores.Text = $"Valor unitário: {produto.Preco.ToString("C", CultureInfo.CurrentCulture)} | Parcial: {item.ValorParcial.ToString("C", CultureInfo.CurrentCulture)}";
        return true;
    }

    private async Task AtualizarTotal(string codigoCompra)
    {
        var compra = await _firebase.Obter<Compra>("compras", codigoCompra);
        if (compra is null) return;
        var itens = await _firebase.Listar<ItemCompra>("itensCompra");
        compra.Id = codigoCompra;
        compra.ValorTotal = itens.Values.Where(i => i.CodigoCompra == codigoCompra).Sum(i => i.ValorParcial);
        await _firebase.Salvar("compras", codigoCompra, compra);
    }

    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        string id = string.IsNullOrWhiteSpace(txID.Text) ? Guid.NewGuid().ToString("N") : txID.Text.Trim();
        try { if (await SalvarItem(id)) LimparFormulario(); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID do item.", "OK"); return; }
        try { string id = txID.Text.Trim(); if (!await _firebase.Existe("itensCompra", id)) { await DisplayAlertAsync("Erro", "Item não encontrado.", "OK"); return; } await SalvarItem(id); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID do item.", "OK"); return; }
        try { string id = txID.Text.Trim(); var item = await _firebase.Obter<ItemCompra>("itensCompra", id); await _firebase.Excluir("itensCompra", id); if (item is not null) await AtualizarTotal(item.CodigoCompra); await CarregarItens(); LimparFormulario(); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private async void btConsultar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID do item.", "OK"); return; }
        try { string id = txID.Text.Trim(); var item = await _firebase.Obter<ItemCompra>("itensCompra", id); if (item is null) { await DisplayAlertAsync("Consulta", "Item não encontrado.", "OK"); return; } item.Id = id; MostrarItem(item); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }
    private void lvItens_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (e.CurrentSelection.FirstOrDefault() is ItemCompra item) MostrarItem(item); }
    private void MostrarItem(ItemCompra item) { txID.Text = item.Id; txProduto.Text = item.CodigoProduto; txCompra.Text = item.CodigoCompra; txQuantidade.Text = item.Quantidade.ToString(); lbValores.Text = $"Valor unitário: {item.ValorUnitario.ToString("C", CultureInfo.CurrentCulture)} | Parcial: {item.ValorParcial.ToString("C", CultureInfo.CurrentCulture)}"; }
    private void sbBusca_TextChanged(object sender, TextChangedEventArgs e) { string busca = e.NewTextValue?.ToLower() ?? ""; lvItens.ItemsSource = _itens.Where(i => i.Id.ToLower().Contains(busca) || i.CodigoCompra.ToLower().Contains(busca) || i.CodigoProduto.ToLower().Contains(busca)).ToList(); }
    private void btLimpar_Clicked(object sender, EventArgs e) => LimparFormulario();
    private void LimparFormulario() { txID.Text = ""; txProduto.Text = ""; txCompra.Text = ""; txQuantidade.Text = ""; lbValores.Text = "Valor unitário: R$ 0,00 | Parcial: R$ 0,00"; lvItens.SelectedItem = null; }
}
