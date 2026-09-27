using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    public sealed class RunResultVisualAnimator : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private RectTransform emblem;
        [SerializeField] private bool victory = true;

        public void Configure(Image animatedBackground, RectTransform resultEmblem)
        {
            background = animatedBackground;
            emblem = resultEmblem;
        }

        public void SetVictory(bool value) => victory = value;

        private void Update()
        {
            var wave = (Mathf.Sin(Time.unscaledTime * 1.4f) + 1f) * .5f;
            if (background != null)
                background.color = victory
                    ? Color.Lerp(new Color(.03f, .18f, .13f, .94f), new Color(.08f, .33f, .24f, .98f), wave)
                    : Color.Lerp(new Color(.18f, .025f, .05f, .95f), new Color(.34f, .035f, .08f, .98f), wave);
            if (emblem != null)
            {
                emblem.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * 1.8f) * 4f);
                emblem.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 2.4f) * .045f);
            }
        }
    }
}
