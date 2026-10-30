using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 師匠の手元に表示するお手本楽器（図鑑スプライト）。
    /// </summary>
    public static class Game1WizardHeldInstrument
    {
        public const string ChildName = "HeldInstrument";

        public static Image Ensure(RectTransform wizardStage)
        {
            if (wizardStage == null) return null;

            var existing = wizardStage.Find(ChildName) as RectTransform;
            Image img;
            RectTransform heldRt;
            if (existing != null)
            {
                heldRt = existing;
                img = existing.GetComponent<Image>();
            }
            else
            {
                var go = new GameObject(ChildName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(wizardStage, false);
                heldRt = go.GetComponent<RectTransform>();
                img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.preserveAspect = true;
            }

            Game1StoryboardLayout.ApplyHeldInstrumentLayout(heldRt);

            return img;
        }

        public static void SetSprite(RectTransform wizardStage, Sprite sprite)
        {
            var img = Ensure(wizardStage);
            if (img == null) return;
            img.sprite = sprite;
            img.enabled = sprite != null;
            img.color = Color.white;
        }

        /// <summary>お手本楽器を非表示（出題中は師匠は音だけ）。</summary>
        public static void Hide(RectTransform wizardStage)
        {
            var img = Ensure(wizardStage);
            if (img == null) return;
            img.enabled = false;
        }

        /// <summary>正解後など、お手本楽器を初めて表示する。</summary>
        public static void Reveal(RectTransform wizardStage, Sprite sprite)
        {
            SetSprite(wizardStage, sprite);
            var held = wizardStage != null ? wizardStage.Find(ChildName) as RectTransform : null;
            if (held != null)
                Geidai.Common.UI.InstrumentFloorShadow.SetVisible(held, false);
        }
    }
}
