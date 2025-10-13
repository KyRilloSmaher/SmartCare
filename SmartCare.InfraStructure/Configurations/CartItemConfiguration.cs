using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItem");

            builder.HasKey(c => c.CartItemId);


            builder.Property(c => c.Quantity)
                .IsRequired();

            builder.Property(c => c.UnitPrice)
                .HasColumnType("decimal(10,2)");

            builder.Property(c => c.SubTotal)
                .HasColumnType("decimal(18,2)");


            //Relations
            builder.HasOne(c => c.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(c => c.CartId);

            builder.HasOne(c => c.Product)
                .WithMany(c => c.CartItems)
                .HasForeignKey(c => c.ProductId);

            builder.HasOne(c => c.Inventory)
                .WithMany(c => c.CartItems)
                .HasForeignKey(c => c.InventoryId);

            builder.HasOne(c => c.Reservation)
                .WithOne(c => c.CartItem)
                .HasForeignKey<Reservation>(c => c.CartItemId);

            builder.HasIndex(c => new { c.CartId, c.ProductId }).IsUnique();
            builder.HasIndex(c => c.InventoryId);
            builder.HasIndex(c => c.ReservationId);


        }
    }
}
