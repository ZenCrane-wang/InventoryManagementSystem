using InventoryERP.Infrastructure.Entities;

namespace InventoryERP.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IRepository<Product>? _products;
    private IRepository<Supplier>? _suppliers;
    private IRepository<PurchaseOrder>? _purchaseOrders;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);
    public IRepository<PurchaseOrder> PurchaseOrders => _purchaseOrders ??= new Repository<PurchaseOrder>(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}