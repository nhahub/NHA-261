using System;
using System.Threading.Tasks;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Supplier> Suppliers { get; }
        IGenericRepository<Warehouse> Warehouses { get; }
        IGenericRepository<Inventory> Inventories { get; }
        IGenericRepository<SalesOrder> SalesOrders { get; }
        IGenericRepository<PurchaseOrder> PurchaseOrders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<StockMovement> StockMovements { get; }

        Task<int> CompleteAsync();
    }
}
