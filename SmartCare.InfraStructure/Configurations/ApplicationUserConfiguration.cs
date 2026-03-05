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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplictionUser>
    {
        public void Configure(EntityTypeBuilder<ApplictionUser> builder)
        {
            builder.Property(c => c.ProfileImageUrl)
                .HasMaxLength(255);

            builder.Property(c => c.OTP)
                .HasMaxLength (255);

            builder.HasIndex(c => c.OTP)
                .IsUnique();

            builder.Property(c => c.RefreshTokenExpiryTime)
                .IsRequired(false);

            builder.Property(c => c.RefreshToken)
                .IsRequired(false)
                .HasMaxLength(500);

            builder
                .HasOne(u => u.Client)
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.Id);

            builder
                .HasOne(u => u.Pharmacist)
                .WithOne(p => p.User)
                .HasForeignKey<Pharmacist>(p => p.Id);

            builder
                .HasOne(u => u.Admin)
                .WithOne(a => a.User)
                .HasForeignKey<Admin>(a => a.Id);


        }
    }
}
