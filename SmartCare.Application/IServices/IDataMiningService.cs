using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    /// <summary>
    /// Defines methods for building frequent itemsets and association rules in a data mining workflow.
    /// </summary>
    public interface IDataMiningService
    {
        Task<List<FrequentItemset>> GenerateFrequentItemsetsAsync(IEnumerable<TransactionDTO> transactions,double minSupport);

        Task<List<AssociationRule>> GenerateAssociationRulesAsync(List<FrequentItemset> itemsets, double minConfidence);

        Task<List<Guid>> GetRecommendationsAsync(Guid productId,List<AssociationRule> rules, int topN = 5);
    }
    public class FrequentItemset
    {
        public HashSet<Guid> Items { get; set; } = new();
        public int SupportCount { get; set; }
    }

    public class AssociationRule
    {
        public HashSet<Guid> Antecedent { get; set; } = new();
        public HashSet<Guid> Consequent { get; set; } = new();

        public double Support { get; set; }
        public double Confidence { get; set; }
        public double Lift { get; set; }
        public double Conviction { get; set; }
    }
}
