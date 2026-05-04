using SmartCare.Application.IServices;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Services.FP_Growth
{
    public class FPNode
    {
        public Guid? Item { get; set; }
        public int Count { get; set; }

        public FPNode Parent { get; set; }

        public Dictionary<Guid, FPNode> Children { get; set; } = new();

        // Link to next node with same item (HEADER TABLE LINK)
        public FPNode Next { get; set; }

        public FPNode(Guid? item, FPNode parent)
        {
            Item = item;
            Parent = parent;
            Count = 1;
        }
    }

    public class HeaderTable
    {
        public Dictionary<Guid, FPNode> FirstNode { get; set; } = new();
        public Dictionary<Guid, int> Frequency { get; set; } = new();
    }
    public class FP_Growth : IDataMiningService
    {
        private List<(List<Guid> path, int count)> GetConditionalPatternBase(Guid item,HeaderTable header)
        {
            var patterns = new List<(List<Guid>, int)>();

            var node = header.FirstNode[item];

            while (node != null)
            {
                var path = new List<Guid>();
                var parent = node.Parent;

                while (parent != null && parent.Item != null)
                {
                    path.Add(parent.Item.Value);
                    parent = parent.Parent;
                }

                if (path.Any())
                    patterns.Add((path, node.Count));

                node = node.Next;
            }

            return patterns;
        }
        private (FPNode root, HeaderTable header) BuildFPTree(List<List<Guid>> transactions,int minSupport)
        {
            var header = new HeaderTable();

            // Step 1: Count frequency
            var freq = transactions
                .SelectMany(t => t)
                .GroupBy(i => i)
                .ToDictionary(g => g.Key, g => g.Count())
                .Where(kv => kv.Value >= minSupport)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            header.Frequency = freq;

            var root = new FPNode(null, null);

            // Step 2: Build tree
            foreach (var transaction in transactions)
            {
                var filtered = transaction
                    .Where(i => freq.ContainsKey(i))
                    .OrderByDescending(i => freq[i])
                    .ToList();

                var current = root;

                foreach (var item in filtered)
                {
                    if (!current.Children.ContainsKey(item))
                    {
                        var newNode = new FPNode(item, current);
                        current.Children[item] = newNode;

                        // Header linking
                        if (!header.FirstNode.ContainsKey(item))
                        {
                            header.FirstNode[item] = newNode;
                        }
                        else
                        {
                            var temp = header.FirstNode[item];
                            while (temp.Next != null)
                                temp = temp.Next;

                            temp.Next = newNode;
                        }
                    }
                    else
                    {
                        current.Children[item].Count++;
                    }

                    current = current.Children[item];
                }
            }

            return (root, header);
        }

        private List<List<Guid>> ExpandPatternBase(List<(List<Guid> path, int count)> patternBase)
        {
            var transactions = new List<List<Guid>>();

            foreach (var (path, count) in patternBase)
            {
                for (int i = 0; i < count; i++)
                {
                    transactions.Add(new List<Guid>(path));
                }
            }

            return transactions;
        }
        private void MineTree(HeaderTable header,List<Guid> suffix,List<FrequentItemset> result,int minSupport,List<List<Guid>> transactions)
        {
            // Sort items by ascending frequency (important)
            var items = header.Frequency
                .OrderBy(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var item in items)
            {
                var newSuffix = new List<Guid>(suffix) { item };

                var support = header.Frequency[item];

                result.Add(new FrequentItemset
                {
                    Items = new HashSet<Guid>(newSuffix),
                    SupportCount = support
                });

                // Step 1: Conditional pattern base
                var patternBase = GetConditionalPatternBase(item, header);

                // Step 2: Convert to transactions
                var conditionalTransactions = ExpandPatternBase(patternBase);

                // Step 3: Build conditional FP-tree
                var (condRoot, condHeader) = BuildFPTree(conditionalTransactions, minSupport);

                if (condHeader.Frequency.Any())
                {
                    // Step 4: Recursive mining
                    MineTree(condHeader, newSuffix, result, minSupport, conditionalTransactions);
                }
            }
        }

        private Dictionary<string, int> BuildSupportLookup(List<FrequentItemset> itemsets)
        {
            return itemsets.ToDictionary(
                x => string.Join(",", x.Items.OrderBy(i => i)),
                x => x.SupportCount);
        }

        private IEnumerable<List<Guid>> GetSubsets(List<Guid> items)
        {
            int n = items.Count;

            for (int i = 1; i < (1 << n) - 1; i++) // exclude empty & full set
            {
                var subset = new List<Guid>();

                for (int j = 0; j < n; j++)
                {
                    if ((i & (1 << j)) > 0)
                        subset.Add(items[j]);
                }

                yield return subset;
            }
        }

        public async Task<List<FrequentItemset>> GenerateFrequentItemsetsAsync(IEnumerable<TransactionDTO> transactions,double minSupport)
        {
            var transList = transactions
                .Select(t => t.productIds)
                .Where(t => t.Any())
                .ToList();

            int minCount = (int)Math.Ceiling(transList.Count * minSupport);

            var (root, header) = BuildFPTree(transList, minCount);

            var result = new List<FrequentItemset>();

            MineTree(header, new List<Guid>(), result, minCount, transList);

            return result;
        }
        public async Task<List<AssociationRule>> GenerateAssociationRulesAsync(List<FrequentItemset> itemsets,double minConfidence)
        {
            var rules = new List<AssociationRule>();

            var supportLookup = BuildSupportLookup(itemsets);

            double totalTransactions = itemsets
                .Where(i => i.Items.Count == 1)
                .Max(i => i.SupportCount); // approximate total

            foreach (var itemset in itemsets.Where(i => i.Items.Count > 1))
            {
                var items = itemset.Items.ToList();

                foreach (var antecedentList in GetSubsets(items))
                {
                    var antecedent = new HashSet<Guid>(antecedentList);
                    var consequent = new HashSet<Guid>(items.Except(antecedent));

                    var antecedentKey = string.Join(",", antecedent.OrderBy(i => i));
                    var consequentKey = string.Join(",", consequent.OrderBy(i => i));
                    var itemsetKey = string.Join(",", items.OrderBy(i => i));

                    if (!supportLookup.ContainsKey(antecedentKey) ||
                        !supportLookup.ContainsKey(consequentKey))
                        continue;

                    double supportAB = itemset.SupportCount / totalTransactions;
                    double supportA = supportLookup[antecedentKey] / totalTransactions;
                    double supportB = supportLookup[consequentKey] / totalTransactions;

                    double confidence = supportAB / supportA;

                    if (confidence < minConfidence)
                        continue;

                    double lift = confidence / supportB;

                    double conviction = (1 - supportB) / (1 - confidence + 1e-10);

                    rules.Add(new AssociationRule
                    {
                        Antecedent = antecedent,
                        Consequent = consequent,
                        Support = supportAB,
                        Confidence = confidence,
                        Lift = lift,
                        Conviction = conviction
                    });
                }
            }

            return rules
                .OrderByDescending(r => r.Lift)
                .ThenByDescending(r => r.Confidence)
                .ToList();
        }
        public async Task<List<Guid>> GetRecommendationsAsync(Guid productId,List<AssociationRule> rules,int topN = 5)
        {
            return rules
                .Where(r => r.Antecedent.Contains(productId))
                .OrderByDescending(r => r.Lift)       // strongest relation
                .ThenByDescending(r => r.Confidence)  // reliability
                .SelectMany(r => r.Consequent)
                .Where(p => p != productId)
                .Distinct()
                .Take(topN)
                .ToList();
        }
    }
}
