using System.Threading.Tasks;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IGenericRepository<Product> Products { get; }
        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<Customer> Customers { get; }
        public IGenericRepository<Supplier> Suppliers { get; }
        public IGenericRepository<Warehouse> Warehouses { get; }
        public IGenericRepository<Inventory> Inventories { get; }
        public IGenericRepository<SalesOrder> SalesOrders { get; }
        public IGenericRepository<PurchaseOrder> PurchaseOrders { get; }
        public IGenericRepository<OrderItem> OrderItems { get; }
        public IGenericRepository<StockMovement> StockMovements { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Products = new GenericRepository<Product>(_context);
            Categories = new GenericRepository<Category>(_context);
            Customers = new GenericRepository<Customer>(_context);
            Suppliers = new GenericRepository<Supplier>(_context);
            Warehouses = new GenericRepository<Warehouse>(_context);
            Inventories = new GenericRepository<Inventory>(_context);
            SalesOrders = new GenericRepository<SalesOrder>(_context);
            PurchaseOrders = new GenericRepository<PurchaseOrder>(_context);
            OrderItems = new GenericRepository<OrderItem>(_context);
            StockMovements = new GenericRepository<StockMovement>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
