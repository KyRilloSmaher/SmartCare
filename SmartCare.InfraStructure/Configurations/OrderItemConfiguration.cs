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
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItem");
            
            builder.HasKey(o => o.Id);


            builder.Property(o => o.Quantity)
                .IsRequired();

            builder.Property(o => o.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(oi => oi.SubTotal)
                .HasComputedColumnSql("[Quantity] * [UnitPrice]")
                .HasColumnType("decimal(18,2)");


            builder.HasOne(o => o.Product)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(o => o.ProductId);

            builder.HasOne(o => o.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(o => o.OrderId);

            builder.HasOne(o => o.Inventory)
                .WithOne(o => o.OrderItem)
                .HasForeignKey<OrderItem>(o => o.InvetoryId);


            builder.HasIndex(oi => oi.ProductId);
            builder.HasIndex(oi => oi.OrderId);
            builder.HasIndex(oi => oi.InvetoryId);

            



        }
    }
}
