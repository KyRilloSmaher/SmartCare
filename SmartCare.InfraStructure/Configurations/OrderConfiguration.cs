using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SmartCare.InfraStructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Order");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Status)
                .IsRequired()
       .HasDefaultValue(Domain.Enums.OrderStatus.Pending);

            builder.Property(o => o.CreatedAt)
                .IsRequired();


            builder.HasOne(o => o.Payment)
                .WithOne(o => o.Order)
                .HasForeignKey<Order>(o => o.PaymentId);

            builder.HasOne(o => o.Store)
                .WithMany(o => o.Orders)
                .HasForeignKey(o => o.StoreId);

            builder.HasOne(o => o.Address)
                .WithMany(o => o.Orders)
                .HasForeignKey(o => o.AddressId);

            builder.HasMany(o => o.Items)
                .WithOne(o => o.Order)
                .HasForeignKey(o => o.OrderId);



            builder.HasIndex(o => o.Status);

            builder.Property(o => o.Status)
                   .HasDefaultValue(OrderStatus.Pending);



        }
    }
}
