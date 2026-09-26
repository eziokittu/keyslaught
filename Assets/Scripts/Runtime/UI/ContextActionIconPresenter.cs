using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    public sealed class ContextActionIconPresenter : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private Image familyIcon;
        [SerializeField] private Image costIcon;
        [SerializeField] private Text costLabel;
        [SerializeField] private Sprite teacher;
        [SerializeField] private Sprite engineer;
        [SerializeField] private Sprite scientist;
        [SerializeField] private Sprite president;
        [SerializeField] private Sprite brainCell;
        public void Configure(Text text, Image family, Image cost, Sprite teacherSprite, Sprite engineerSprite, Sprite scientistSprite, Sprite presidentSprite, Sprite brain)
        { label=text;familyIcon=family;costIcon=cost;teacher=teacherSprite;engineer=engineerSprite;scientist=scientistSprite;president=presidentSprite;brainCell=brain;Refresh(); }
        public void ConfigureCostLabel(Text value) => costLabel = value;
        public void SetCost(string value, bool showIcon)
        {
            if (costLabel != null)
            {
                costLabel.text = value ?? string.Empty;
                costLabel.fontSize = value == "LOCKED" ? 16 : 22;
                costLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                costLabel.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
            if (costIcon != null)
            {
                costIcon.sprite = brainCell;
                costIcon.gameObject.SetActive(showIcon);
            }
        }
        private void Update() => Refresh();
        private void Refresh()
        {
            if (label == null) return; var value = label.text ?? string.Empty; Sprite sprite = null;
            if (value.Contains("Teacher")) sprite=teacher; else if(value.Contains("Engineer")) sprite=engineer;
            else if(value.Contains("Scientist")) sprite=scientist; else if(value.Contains("President")) sprite=president;
            if(familyIcon!=null){familyIcon.sprite=sprite;familyIcon.gameObject.SetActive(sprite!=null);}
            if(costIcon!=null)costIcon.sprite=brainCell;
        }
    }
}
