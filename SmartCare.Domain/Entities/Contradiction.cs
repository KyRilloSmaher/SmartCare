using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    [Table("Contradictions")]
    public class Contradiction
    {

        [Required]
        [Column("Ingredient_A")]
        [StringLength(100)]
        public string Ingredient_A { get; set; } = string.Empty;

        [Required]
        [Column("Ingredient_B")]
        [StringLength(100)]
        public string Ingredient_B { get; set; } = string.Empty;

        [Column("Reason")]
        public string? Reason { get; set; } 

        [Column("Severity")]
        [StringLength(20)]
        public string? Severity { get; set; }
    }
}
