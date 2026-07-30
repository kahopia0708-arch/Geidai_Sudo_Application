using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Common.Library
{
    /// <summary>解除条件表（SO / BR-UNLOCK-01）。</summary>
    [CreateAssetMenu(fileName = "UnlockRulesCatalog", menuName = "Geidai/Unlock Rules Catalog", order = 11)]
    public class UnlockRulesCatalog : ScriptableObject
    {
        [SerializeField] private List<UnlockRule> rules = new List<UnlockRule>();

        public IReadOnlyList<UnlockRule> Rules => rules;

        public List<UnlockRule> ValidRules()
        {
            var result = new List<UnlockRule>();
            if (rules == null) return result;
            for (int i = 0; i < rules.Count; i++)
            {
                var r = rules[i];
                if (r != null && r.IsValid) result.Add(r);
            }
            return result;
        }

        public void SetRules(IEnumerable<UnlockRule> newRules)
        {
            rules = newRules != null ? new List<UnlockRule>(newRules) : new List<UnlockRule>();
        }
    }
}
