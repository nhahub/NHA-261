using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<SupplierProduct> SupplierProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Composite key for Inventory
            builder.Entity<Inventory>()
                .HasKey(i => new { i.ProductID, i.WarehouseID });

            // Many-to-many for Supplier <-> Product
            builder.Entity<SupplierProduct>()
                .HasKey(sp => new { sp.SupplierID, sp.ProductID });

            // StockMovement creator
            builder.Entity<StockMovement>()
                .HasOne(m => m.CreatedBy)
                .WithMany(u => u.CreatedStockMovements)
                .HasForeignKey(m => m.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional self references
            builder.Entity<StockMovement>()
                .HasOne(m => m.FromWarehouse)
                .WithMany(w => w.FromMovements)
                .HasForeignKey(m => m.FromWarehouseID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockMovement>()
                .HasOne(m => m.ToWarehouse)
                .WithMany(w => w.ToMovements)
                .HasForeignKey(m => m.ToWarehouseID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
