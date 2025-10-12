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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Category");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LogoUrl)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.IsDeleted)
                .HasDefaultValue(false);



            builder.HasMany(c => c.Products)
                .WithOne(c => c.Category)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasIndex(c => c.Id);

           



        }
    }
}
