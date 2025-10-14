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
    public class RateConfiguration : IEntityTypeConfiguration<Rate>
    {
        public void Configure(EntityTypeBuilder<Rate> builder)
        {
            builder.ToTable("Rate");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Value)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);


            builder.HasOne(x => x.Product)
                .WithMany(x => x.Rates)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(r => r.ClientId);
            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.Value);
        }
    }
}
