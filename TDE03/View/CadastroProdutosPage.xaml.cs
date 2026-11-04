using System.Globalization;
using TDE03.Model;
using TDE03.Services;

namespace TDE03.View;

public partial class CadastroProdutosPage : ContentPage
{
    private readonly FirebaseService _firebase;
    private List<Produto> _produtos = new();

    public CadastroProdutosPage(FirebaseService firebase) { InitializeComponent(); _firebase = firebase; }

    protected override async void OnAppearing() { base.OnAppearing(); await CarregarProdutos(); }

    private async Task CarregarProdutos()
    {
        try
        {
            var dados = await _firebase.Listar<Produto>("produtos");
            _produtos = dados.Select(item => { item.Value.Id = item.Key; return item.Value; }).ToList();
            lvProdutos.ItemsSource = _produtos;
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private bool LerFormulario(out Produto produto)
    {
        produto = new Produto();
        if (string.IsNullOrWhiteSpace(txNome.Text) || txNome.Text.Trim().Length > 50 ||
            string.IsNullOrWhiteSpace(txTipo.Text) || txTipo.Text.Trim().Length > 50) return false;
        if (!decimal.TryParse(txPreco.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal preco) &&
            !decimal.TryParse(txPreco.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out preco)) return false;
        if (!int.TryParse(txEstoque.Text, out int estoque) || preco < 0 || estoque < 0) return false;
        produto.Nome = txNome.Text.Trim();
        produto.Tipo = txTipo.Text.Trim();
        produto.Preco = preco;
        produto.QuantidadeEstoque = estoque;
        return true;
    }
    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        if (!LerFormulario(out var produto)) { await DisplayAlertAsync("Validação", "Informe todos os dados. Preço e estoque devem ser números positivos ou zero.", "OK"); return; }
        string id = string.IsNullOrWhiteSpace(txID.Text) ? Guid.NewGuid().ToString("N") : txID.Text.Trim();
        try { produto.Id = id; await _firebase.Salvar("produtos", id, produto); await CarregarProdutos(); LimparFormulario(); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text) || !LerFormulario(out var produto)) { await DisplayAlertAsync("Validação", "Selecione um produto e preencha os dados corretamente.", "OK"); return; }
        try { string id = txID.Text.Trim(); if (!await _firebase.Existe("produtos", id)) { await DisplayAlertAsync("Erro", "Produto não encontrado.", "OK"); return; } produto.Id = id; await _firebase.Salvar("produtos", id, produto); await CarregarProdutos(); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID do produto.", "OK"); return; }
        try { string id = txID.Text.Trim(); var itens = await _firebase.Listar<ItemCompra>("itensCompra"); if (itens.Values.Any(i => i.CodigoProduto == id)) { await DisplayAlertAsync("Erro", "O produto possui itens de compra vinculados.", "OK"); return; } await _firebase.Excluir("produtos", id); await CarregarProdutos(); LimparFormulario(); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private async void btConsultar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txID.Text)) { await DisplayAlertAsync("Validação", "Informe o ID do produto.", "OK"); return; }
        try { string id = txID.Text.Trim(); var produto = await _firebase.Obter<Produto>("produtos", id); if (produto is null) { await DisplayAlertAsync("Consulta", "Produto não encontrado.", "OK"); return; } produto.Id = id; MostrarProduto(produto); }
        catch (Exception ex) { await DisplayAlertAsync("Erro", ex.Message, "OK"); }
    }

    private void lvProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (e.CurrentSelection.FirstOrDefault() is Produto produto) MostrarProduto(produto); }
    private void MostrarProduto(Produto p) { txID.Text = p.Id; txNome.Text = p.Nome; txTipo.Text = p.Tipo; txPreco.Text = p.Preco.ToString(CultureInfo.CurrentCulture); txEstoque.Text = p.QuantidadeEstoque.ToString(); }
    private void sbBusca_TextChanged(object sender, TextChangedEventArgs e) { string busca = e.NewTextValue?.ToLower() ?? ""; lvProdutos.ItemsSource = _produtos.Where(p => p.Nome.ToLower().Contains(busca) || p.Tipo.ToLower().Contains(busca) || p.Id.ToLower().Contains(busca)).ToList(); }
    private void btLimpar_Clicked(object sender, EventArgs e) => LimparFormulario();
    private void LimparFormulario() { txID.Text = ""; txNome.Text = ""; txTipo.Text = ""; txPreco.Text = ""; txEstoque.Text = ""; lvProdutos.SelectedItem = null; }
}

