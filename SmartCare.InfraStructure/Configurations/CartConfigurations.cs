using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Configurations
{
    public class CartConfigurations : IEntityTypeConfiguration<Cart>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Cart> builder)
        {

            builder.ToTable("Cart");


            builder.HasKey(x => x.Id);



            builder.Property(x => x.status)
                  .IsRequired();

            builder.Property(x => x.TotalPrice)
                .HasColumnType("decimal(8,2)");



            builder.HasMany(x => x.Items)
                .WithOne(x => x.Cart)
                .HasForeignKey(x => x.CartId);



            builder.HasIndex(x => x.ClientId)
                .IsUnique();

            builder.HasIndex(x => x.status);
                
        }
    }
}
