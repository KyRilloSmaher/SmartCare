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
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("Reservation");
 
            builder.HasKey(r => r.Id);

            builder.Property(r => r.QuantityReserved)
                   .IsRequired();

            builder.Property(r => r.ReservedAt)
                   .IsRequired();

            builder.Property(r => r.ExpiredAt)
                   .IsRequired();

            builder.HasOne(r => r.CartItem)
                   .WithOne(r => r.Reservation) 
                   .HasForeignKey<Reservation>(r => r.CartItemId);

            builder.HasIndex(r => r.CartItemId);
            builder.HasIndex(r => r.ReservedAt);
            builder.HasIndex(r => r.ExpiredAt);


        }
    }
}
