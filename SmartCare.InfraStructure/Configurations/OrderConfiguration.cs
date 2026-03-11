using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Entities;

namespace SmartCare.InfraStructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Order");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Status)
                .HasField("_status")                     // <-- important for your custom setter
                .IsRequired()
                .HasDefaultValue(Domain.Enums.OrderStatus.Pending)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            builder.HasIndex(o => o.Status);
        }
    }
}
