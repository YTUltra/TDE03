namespace TDE03.Model;

public class ItemCompra
{
    public string Id { get; set; } = string.Empty;
    public string CodigoProduto { get; set; } = string.Empty;
    public string CodigoCompra { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorParcial { get; set; }
}
