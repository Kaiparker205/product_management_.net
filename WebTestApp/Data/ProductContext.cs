using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTestApp.Models;
namespace WebTestApp.Data
{

    public class ProductContext : IdentityDbContext<Employer>
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Employer> Employer { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().HasKey(o => new { o.EmployerId, o.ProductId });
            modelBuilder.Entity<Order>().ToTable("ProductOrders");
            modelBuilder.Entity<Order>().HasOne(o => o.Employer).WithMany().HasForeignKey(o => o.EmployerId);
            modelBuilder.Entity<Order>().HasOne(o => o.Product).WithMany().HasForeignKey(o => o.ProductId);
        }
    }

}