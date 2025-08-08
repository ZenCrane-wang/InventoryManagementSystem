using InventoryERP.Infrastructure.Entities;

namespace InventoryERP.Infrastructure.Repositories;

public interface IUnitOfWork
{
    IRepository<Product> Products { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<PurchaseOrder> PurchaseOrders { get; }
    Task<int> SaveChangesAsync();
}