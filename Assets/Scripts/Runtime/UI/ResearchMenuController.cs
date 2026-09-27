using KeySlaught.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    public sealed class ResearchMenuController : MonoBehaviour
    {
        [SerializeField] private ProgressionService progression;
        [SerializeField] private GameObject[] categoryPanels;
        [SerializeField] private Text[] categoryLabels;
        [SerializeField] private GameObject[] detailPanels;
        [SerializeField] private Text[] summaryLabels;
        [SerializeField] private Text[] detailLabels;
        [SerializeField] private Button[] purchaseButtons;
        [SerializeField] private Text[] purchaseLabels;
        private int openCategory;
        private int openUpgrade = -1;

        public void Configure(ProgressionService service, GameObject[] categories, Text[] categoryHeaders,
            GameObject[] details, Text[] summaries, Text[] detailTexts, Button[] purchases, Text[] purchaseTexts)
        {
            progression = service; categoryPanels = categories; categoryLabels = categoryHeaders;
            detailPanels = details; summaryLabels = summaries; detailLabels = detailTexts;
            purchaseButtons = purchases; purchaseLabels = purchaseTexts;
            openCategory = 0; openUpgrade = -1; Refresh();
        }

        public void ToggleCategory(int index)
        {
            openCategory = openCategory == index ? -1 : index;
            openUpgrade = -1;
            Refresh();
        }

        public void ToggleUpgrade(int index)
        {
            openUpgrade = openUpgrade == index ? -1 : index;
            Refresh();
        }

        public void Purchase(int index)
        {
            if (progression?.Definitions == null || index < 0 || index >= progression.Definitions.Length) return;
            progression.TryPurchase(progression.Definitions[index].Id);
            Refresh();
        }

        private void OnEnable()
        {
            if (progression != null)
            {
                progression.Changed -= Refresh;
                progression.Changed += Refresh;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (progression != null) progression.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (categoryPanels != null)
                for (var index = 0; index < categoryPanels.Length; index++)
                {
                    if (categoryPanels[index] != null) categoryPanels[index].SetActive(index == openCategory);
                    if (categoryLabels != null && index < categoryLabels.Length && categoryLabels[index] != null)
                    {
                        var clean = categoryLabels[index].text.TrimStart('▼', '▶', ' ');
                        categoryLabels[index].text = $"{(index == openCategory ? "▼" : "▶")}  {clean}";
                    }
                }

            var definitions = progression?.Definitions;
            if (definitions == null || progression.Profile == null) return;
            for (var index = 0; index < definitions.Length; index++)
            {
                var definition = definitions[index];
                if (definition == null) continue;
                var visible = progression.IsResearchVisible(definition);
                if (summaryLabels != null && index < summaryLabels.Length && summaryLabels[index] != null)
                    summaryLabels[index].transform.parent.gameObject.SetActive(visible);
                if (!visible)
                {
                    if (detailPanels != null && index < detailPanels.Length && detailPanels[index] != null)
                        detailPanels[index].SetActive(false);
                    if (openUpgrade == index) openUpgrade = -1;
                    continue;
                }
                var level = progression.Profile.GetLevel(definition.Id);
                var maxed = level >= definition.MaxLevel;
                var cost = definition.CostForLevel(level);
                if (summaryLabels != null && index < summaryLabels.Length && summaryLabels[index] != null)
                    summaryLabels[index].text = $"{definition.DisplayName}  LV {level}/{definition.MaxLevel}";
                if (definition.IsUnlock && summaryLabels != null && index < summaryLabels.Length && summaryLabels[index] != null)
                    summaryLabels[index].text = $"UNLOCK {definition.DisplayName}";
                if (detailPanels != null && index < detailPanels.Length && detailPanels[index] != null)
                    detailPanels[index].SetActive(index == openUpgrade);
                if (detailLabels != null && index < detailLabels.Length && detailLabels[index] != null)
                {
                    var current = level * definition.ValuePerLevel;
                    var next = Mathf.Min(definition.MaxLevel, level + 1) * definition.ValuePerLevel;
                    detailLabels[index].text = maxed
                        ? $"Current value: {Format(definition, current)}\nMaximum level reached"
                        : $"Previous value: {Format(definition, current)}\nUpgraded value: {Format(definition, next)}";
                }
                if (purchaseLabels != null && index < purchaseLabels.Length && purchaseLabels[index] != null)
                    purchaseLabels[index].text = maxed ? "MAXIMUM" : $"UPGRADE  •  {cost} KP";
                if (purchaseButtons != null && index < purchaseButtons.Length && purchaseButtons[index] != null)
                    purchaseButtons[index].interactable = !maxed && progression.Profile.knowledgePoints >= cost;
                if (definition.IsUnlock)
                {
                    if (detailLabels != null && index < detailLabels.Length && detailLabels[index] != null)
                        detailLabels[index].text = $"Spend Knowledge Points to unlock {definition.DisplayName} at Level 1.\nIts permanent upgrades appear here afterward.";
                    if (purchaseLabels != null && index < purchaseLabels.Length && purchaseLabels[index] != null)
                        purchaseLabels[index].text = maxed ? "UNLOCKED" : $"UNLOCK  -  {cost} KP";
                }
            }
        }

        private static string Format(ResearchDefinition definition, float value)
        {
            return definition.Stat switch
            {
                ResearchStat.PlayerRange => $"{4f + value:0.00} tiles",
                ResearchStat.MagazineCapacity => $"{4 + Mathf.RoundToInt(value)} slots",
                ResearchStat.LibraryHealth => $"{15 + Mathf.RoundToInt(value)} HP",
                ResearchStat.ReloadSpeed => $"{Mathf.Max(.1f, .5f - value):0.00}s per slot",
                ResearchStat.TeacherRange => $"{2.6f + value:0.00} tiles",
                ResearchStat.EngineerRange => $"{2.4f + value:0.00} tiles",
                ResearchStat.ScientistRange => $"{2.1f + value:0.00} tiles",
                ResearchStat.PresidentRange => $"{2.3f + value:0.00} tiles",
                ResearchStat.TeacherAttackSpeed => $"{1.8f / (1f + value):0.00}s per shot",
                ResearchStat.EngineerAttackSpeed => $"{2.15f / (1f + value):0.00}s per shot",
                ResearchStat.ScientistAttackSpeed => $"{2.6f / (1f + value):0.00}s per shot",
                ResearchStat.PresidentAttackSpeed => $"{2.35f / (1f + value):0.00}s per shot",
                ResearchStat.HistoryDuration or ResearchStat.SocialInfluenceDuration => $"{5f + value:0.0}s",
                ResearchStat.PoliticsTargetCount => $"{3 + Mathf.RoundToInt(value)} targets",
                _ => $"{value:0.00}"
            };
        }
    }
}
