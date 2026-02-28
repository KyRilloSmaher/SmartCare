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
    public class PharmacistConfiguration : IEntityTypeConfiguration<Pharmacist>
    {
        public void Configure(EntityTypeBuilder<Pharmacist> builder)
        {


            builder.Property(p => p.LicenseNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(p => p.Store)
                .WithMany(s => s.pharmacists)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.LicenseNumber).IsUnique();
        }
    }
}
