namespace InventoryERP.Infrastructure.Entities;

public class PurchaseOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = null!;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "Draft";
    public List<PurchaseOrderItem> Items { get; set; } = new();
}