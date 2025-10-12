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
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Company");

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
                .WithOne(c => c.Company)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasIndex(c => c.Id);

        }
    }
}
