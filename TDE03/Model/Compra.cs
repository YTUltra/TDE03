namespace TDE03.Model;

public class Compra
{
    public string Id { get; set; } = string.Empty;
    public DateTime DataCompra { get; set; } = DateTime.Today;
    public decimal ValorTotal { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
}
