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

            builder.Property(c => c.Code)
                .HasMaxLength (255);

            builder.HasIndex(c => c.Code)
                .IsUnique();

            builder.Property(c => c.birthDate)
                .IsRequired(false);

            builder.Property(c => c.RefreshTokenExpiryTime)
                .IsRequired(false);

            //Relations

            builder.HasMany(c => c.Favorites)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.Addresses)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.Orders)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.Rates)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.Cart)
                .WithOne(c => c.Client)
                .HasForeignKey<Cart>(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
