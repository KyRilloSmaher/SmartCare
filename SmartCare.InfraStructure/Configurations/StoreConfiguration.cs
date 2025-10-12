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
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Store");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.Address)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(s => s.Phone)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(s => s.IsDeleted)
                   .HasDefaultValue(false);

            builder.HasMany(s => s.Orders)
                   .WithOne(o => o.Store)
                   .HasForeignKey(o => o.StoreId);

            builder.HasMany(s => s.Inventories)
                   .WithOne(i => i.Store)
                   .HasForeignKey(i => i.StoreId);

            builder.HasIndex(s => s.Name);
            builder.HasIndex(s => s.Phone);
            builder.HasIndex(s => s.IsDeleted);


        }
    }
}
