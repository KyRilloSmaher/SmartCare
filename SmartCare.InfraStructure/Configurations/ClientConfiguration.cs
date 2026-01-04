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
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.Property(c => c.ProfileImageUrl)
                .HasMaxLength(255);

            builder.Property(c => c.OTP)
                .HasMaxLength (255);

            builder.HasIndex(c => c.OTP)
                .IsUnique();

            builder.Property(c => c.BirthDate)
                .IsRequired();

            builder.Property(c => c.RefreshTokenExpiryTime)
                .IsRequired(false);

            builder.Property(c => c.RefreshToken)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(c=>c.RatesCount).HasDefaultValue(0);
            builder.Property(c => c.FavoritesCount).HasDefaultValue(0);
            builder.Property(c=>c.OrdersCount).HasDefaultValue(0);


            //Relations

            builder.HasMany(c => c.Favorites)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Addresses)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Orders)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.SetNull);


        }
    }
}
