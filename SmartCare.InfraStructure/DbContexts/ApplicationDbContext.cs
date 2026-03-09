
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using SmartCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;



namespace SmartCare.InfraStructure.DbContexts
{

    public class ApplicationDBContext : IdentityDbContext<ApplictionUser>
    {
        public ApplicationDBContext()
        {

        }
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OnlineOrder> OnlineOrders { get; set; }
        public DbSet<PickUpOrder> FromStoreOrders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Store> Stores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OnlineOrder>().ToTable("OnlineOrders");
            modelBuilder.Entity<PickUpOrder>().ToTable("FromStoreOrders");
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}