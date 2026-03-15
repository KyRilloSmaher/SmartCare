
namespace SmartCare.Application.DTOs.Contradictions.Response
{
    public class ContradictionDetail
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public string IngredientA { get; set; } = string.Empty;
        public string IngredientB { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Severity { get; set; }
        public int SeverityLevel { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}