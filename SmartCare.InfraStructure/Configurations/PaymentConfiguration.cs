using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payment");

            builder.HasKey(x => x.Id);


            builder.Property(p => p.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.PaymentMethod)
                .IsRequired()
                .HasDefaultValue(Domain.Enums.PaymentMethod.Cash);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(Domain.Enums.PaymentStatus.Pending);

            builder.Property(x => x.PaymentIntentId)
                .IsRequired(false);  
            builder.Property(x => x.SessionId)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasIndex(p => p.OrderId);
            builder.HasIndex(p => p.SessionId);
            builder.HasIndex(p => p.PaymentIntentId);

        }
    }
}
