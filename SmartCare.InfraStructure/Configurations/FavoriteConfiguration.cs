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
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.ToTable("Favorite");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Client)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
