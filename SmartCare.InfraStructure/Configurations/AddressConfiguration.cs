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
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {

        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("UserAddress");

            builder.HasKey(x => x.Id);

            builder.Property(a => a.Id).IsRequired();

            builder.Property(a => a.address)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(a => a.AdditionalInfo)
                  .IsRequired(false);

            builder.Property( a => a.IsPrimary)
                 .IsRequired();

            //Relations

            builder.HasOne(a => a.Client)
                    .WithMany(a => a.Addresses);
                    
            builder.HasMany(a => a.Orders)
                   .WithOne(a => a.Address)
                   .HasForeignKey(a => a.AddressId);

            builder.HasIndex(a => a.IsPrimary);
            builder.HasIndex(a => new { a.Latitude, a.Longitude });
        }
    }
}
