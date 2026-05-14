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
    public class DeliveryConfigurations : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.Orders)
                   .WithOne(o => o.Delivery)
                   .HasForeignKey(o => o.DeliveryId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
