using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// シーンが旧レイアウトのとき、Play 時に絵コンテ配置（左カエル・右答え睡蓮・池）を自動生成する。
    /// </summary>
    [DisallowMultipleComponent]
    public class SoundMatchStoryboardUiBootstrap : MonoBehaviour
    {
        [SerializeField] private SoundMatchGameController controller;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<SoundMatchGameController>();
            if (controller == null) return;

            var playArea = FindPlayArea();
            if (playArea == null) return;

            ApplyStoryboardScale(playArea, controller);
            EnsureWater(playArea);
            var back = playArea.Find("Back") as RectTransform;
            Game1StoryboardLayout.ApplyBackButtonLayout(back);

            if (controller.HasAnswerPad) return;

            var config = controller.Config;
            var lily = config != null ? config.PlaceholderLilyPadSprite : null;
            var frog = config != null ? config.PlaceholderTargetSprite : null;

            EnsureWater(playArea);
            var left = CreateLily(playArea, "LilyPad_Left", Game1StoryboardLayout.LilyAnchorLeft, lily);
            var right = CreateLily(playArea, "LilyPad_Answer", Game1StoryboardLayout.LilyAnchorAnswer, lily);

            var frogView = CreateDemoFrog(left, frog, controller);
            var answer = CreateAnswerPad(right);

            controller.ApplyStoryboardReferences(frogView, answer, playArea);
            HideLegacyFrog(playArea);
            RepositionChoices(playArea, controller);
        }

        private static RectTransform FindPlayArea()
        {
            var choices = Object.FindFirstObjectByType<ChoiceItemView>();
            if (choices != null)
                return choices.transform.parent as RectTransform;

            var fitter = Object.FindFirstObjectByType<Geidai.Common.UI.SafeAreaFitter>();
            return fitter != null ? fitter.transform as RectTransform : null;
        }

        private static void EnsureWater(RectTransform playArea)
        {
            var existing = playArea.Find("PondWater") as RectTransform;
            RectTransform rt;
            if (existing == null)
            {
                var go = new GameObject("PondWater", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(playArea, false);
                go.transform.SetAsFirstSibling();
                rt = go.GetComponent<RectTransform>();
                var img = go.GetComponent<Image>();
                img.color = new Color(0.75f, 0.9f, 0.98f, 0.55f);
                img.raycastTarget = false;
            }
            else
            {
                rt = existing;
                existing.SetAsFirstSibling();
            }

            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, Game1StoryboardLayout.WaterSurfaceAnchorY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateLily(
            RectTransform playArea,
            string name,
            Vector2 anchor,
            Sprite sprite)
        {
            var existing = playArea.Find(name) as RectTransform;
            RectTransform rt;
            if (existing != null)
            {
                rt = existing;
            }
            else
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(playArea, false);
                rt = go.GetComponent<RectTransform>();
            }

            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = Game1StoryboardLayout.LilyPadSize;
            rt.anchoredPosition = Vector2.zero;
            Game1LilyPadVisual.Setup(rt, sprite);
            return rt;
        }

        private static FrogTargetView CreateDemoFrog(
            RectTransform leftPad,
            Sprite frogSprite,
            SoundMatchGameController controller)
        {
            var go = new GameObject("DemoFrog", typeof(RectTransform), typeof(Image), typeof(FrogTargetView));
            go.transform.SetParent(leftPad, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.55f);
            rt.anchorMax = new Vector2(0.5f, 0.55f);
            rt.sizeDelta = Game1StoryboardLayout.DemoFrogSize;
            var img = go.GetComponent<Image>();
            if (frogSprite != null)
            {
                img.sprite = frogSprite;
                Game1CharacterImage.Apply(img);
            }

            var frogView = go.GetComponent<FrogTargetView>();
            var config = controller.Config;
            var frogOpen = config != null ? config.PlaceholderTargetOpenSprite : null;
            frogView.Setup(controller, frogSprite, frogOpen);
            return frogView;
        }

        private static SoundMatchAnswerPadView CreateAnswerPad(RectTransform rightPad)
        {
            var rootGo = new GameObject("AnswerPad", typeof(RectTransform), typeof(SoundMatchAnswerPadView));
            rootGo.transform.SetParent(rightPad, false);
            var root = rootGo.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var snapGo = new GameObject("Snap", typeof(RectTransform));
            snapGo.transform.SetParent(root, false);
            var snap = snapGo.GetComponent<RectTransform>();
            snap.anchorMin = new Vector2(0.5f, 0.55f);
            snap.anchorMax = new Vector2(0.5f, 0.55f);
            snap.sizeDelta = Game1StoryboardLayout.AnswerSnapSize;

            var pad = rootGo.GetComponent<SoundMatchAnswerPadView>();
            pad.Configure(root, snap);
            return pad;
        }

        private static void HideLegacyFrog(RectTransform playArea)
        {
            var legacy = playArea.Find("Frog");
            if (legacy != null) legacy.gameObject.SetActive(false);
        }

        private static void ApplyStoryboardScale(RectTransform playArea, SoundMatchGameController controller)
        {
            var lilies = new[] { "LilyPad_Left", "LilyPad_Answer" };
            for (int i = 0; i < lilies.Length; i++)
            {
                var lily = playArea.Find(lilies[i]) as RectTransform;
                if (lily == null) continue;
                lily.sizeDelta = Game1StoryboardLayout.LilyPadSize;
                lily.anchorMin = i == 0
                    ? Game1StoryboardLayout.LilyAnchorLeft
                    : Game1StoryboardLayout.LilyAnchorAnswer;
                lily.anchorMax = lily.anchorMin;
                var config = controller != null ? controller.Config : null;
                var lilySprite = config != null ? config.PlaceholderLilyPadSprite : null;
                Game1LilyPadVisual.Setup(lily, lilySprite);
            }

            var demoFrog = playArea.GetComponentInChildren<FrogTargetView>(true);
            if (demoFrog != null)
            {
                var rt = demoFrog.transform as RectTransform;
                if (rt != null) rt.sizeDelta = Game1StoryboardLayout.DemoFrogSize;
            }

            var legacy = playArea.Find("Frog") as RectTransform;
            if (legacy != null && legacy.gameObject.activeSelf)
                legacy.sizeDelta = Game1StoryboardLayout.DemoFrogSize;

            RepositionChoices(playArea, controller);
        }

        private static void RepositionChoices(RectTransform playArea, SoundMatchGameController controller)
        {
            var choices = playArea.GetComponentsInChildren<ChoiceItemView>(true);
            for (int i = 0; i < choices.Length; i++)
            {
                var rt = choices[i].transform as RectTransform;
                if (rt == null) continue;
                rt.anchorMin = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                rt.anchorMax = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                rt.sizeDelta = Game1StoryboardLayout.ChoiceSize;
                float x = i < Game1StoryboardLayout.ChoicePoolX.Length
                    ? Game1StoryboardLayout.ChoicePoolX[i]
                    : 0f;
                rt.anchoredPosition = new Vector2(x, 0f);
            }
        }
    }
}
