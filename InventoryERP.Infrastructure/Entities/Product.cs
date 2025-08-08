namespace InventoryERP.Infrastructure.Entities;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Unit { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
}