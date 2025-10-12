using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventory");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StockQuantity)
                .IsRequired();

            builder.HasOne(x => x.Store)
                .WithMany(x => x.Inventories)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Inventories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.OrderItem)
                .WithOne(x => x.Inventory)
                .HasForeignKey<OrderItem>(x => x.InvetoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.CartItems)
                .WithOne(x => x.Inventory)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasIndex(x => new { x.StoreId, x.ProductId })
                .IsUnique();
            
        }
    }
}
