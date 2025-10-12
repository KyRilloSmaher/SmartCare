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
    public class ProductImagesConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImage");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Url)
              .IsRequired()
              .HasMaxLength(500);

            builder.Property(p => p.IsPrimary)
                   .HasDefaultValue(false);

            builder.HasOne(p => p.Product)
                   .WithMany(p => p.Images)
                   .HasForeignKey(p => p.ProductId);

            builder.HasIndex(p => p.ProductId);
            builder.HasIndex(p => p.IsPrimary);


        }
    }
}
