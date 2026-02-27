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

            builder.Property(x => x.Method)
                .IsRequired()
                .HasDefaultValue(Domain.Enums.PaymentMethod.Cash);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(Domain.Enums.PaymentStatus.Pending);

            builder.Property(x => x.ClientPaymentToken)
                .IsRequired(false);
            builder.Property(x => x.ProviderReferenceId)
                .IsRequired(false);

            //builder.Property(x => x.SessionId)
            //    .IsRequired(false);

            builder.Property(x => x.Version)
                .IsRequired()
                .HasDefaultValue(1); // track updates

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(p => p.OrderId).IsUnique();
            builder.HasIndex(p => p.ProviderReferenceId);
        }
    }

}
