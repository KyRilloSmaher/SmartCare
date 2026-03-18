using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Configurations
{
    public class ContradictionsConfigurations : IEntityTypeConfiguration<Contradiction>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Contradiction> builder)
        {
            builder.ToTable("Contradictions");
            builder.HasKey(i => new { i.Ingredient_A, i.Ingredient_B });
            builder.Property(e => e.Ingredient_A)
                .HasColumnName("Ingredient_A")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Ingredient_B)
                .HasColumnName("Ingredient_B")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Reason)
                .HasColumnName("Reason")
                .HasColumnType("text"); 

            builder.Property(e => e.Severity)
                .HasColumnName("Severity")
                .HasMaxLength(20);

            // Add indexes for better performance
            builder.HasIndex(e => e.Ingredient_A)
                .HasDatabaseName("IX_Contradictions_Ingredient_A");

            builder.HasIndex(e => e.Ingredient_B)
                .HasDatabaseName("IX_Contradictions_Ingredient_B");

            // Ensure unique combinations
            builder.HasIndex(e => new { e.Ingredient_A, e.Ingredient_B })
                .HasDatabaseName("IX_Contradictions_Ingredient_Combination")
                .IsUnique();
        
        }
    }
}
