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
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
           
            builder.ToTable("Products");

            builder.HasKey(p => p.ProductId);

            builder.Property(p => p.NameAr)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.NameEn)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.Description)
                   .IsRequired();

            builder.Property(p => p.MedicalDescription)
                   .IsRequired();

            builder.Property(p => p.Tags)
                   .HasMaxLength(500);

            builder.Property(p => p.ActiveIngredients)
                   .HasMaxLength(500);

            builder.Property(p => p.SideEffects)
                   .HasMaxLength(500);

            builder.Property(p => p.Contraindications)
                   .HasMaxLength(500);

            builder.Property(p => p.DosageForm)
                   .HasMaxLength(100);

            builder.Property(p => p.IsDeleted)
                   .HasDefaultValue(false);

            builder.Property(p => p.IsAvailable)
                   .HasDefaultValue(true);

            builder.Property(p => p.Price)
                   .HasColumnType("decimal(18,2)");

            builder.Property(p => p.DiscountPercentage)
                   .HasColumnType("float");

            builder.Property(p => p.AverageRating)
                   .HasColumnType("float");

            builder.Property(p => p.SearchVector)
                   .HasMaxLength(1000);

            builder.Property(p => p.EmbeddingVector)
                   .HasMaxLength(1000);

            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Company)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CompanyId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.CartItems)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Inventories)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Images)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasIndex(p => p.NameEn);
            builder.HasIndex(p => p.NameAr);
            builder.HasIndex(p => p.Tags);
            builder.HasIndex(p => p.IsAvailable);


        }
    }
}
