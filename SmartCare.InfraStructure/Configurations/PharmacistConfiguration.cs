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
            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.LicenseNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.Gender)
                .HasConversion<string>();

            builder.Property(p => p.AccountType)
                .HasConversion<string>();

            builder.Property(c => c.ProfileImageUrl)
                .HasMaxLength(255);

            builder.Property(c => c.OTP)
                .HasMaxLength(255);

            builder.HasIndex(c => c.OTP)
                .IsUnique();

            builder.Property(c => c.RefreshTokenExpiryTime)
                .IsRequired(false);

            builder.Property(c => c.RefreshToken)
                .IsRequired(false)
                .HasMaxLength(500);


            builder.HasOne(p => p.Store)
                .WithMany(s => s.pharmacists) 
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.LicenseNumber).IsUnique();
        }
    }
}
