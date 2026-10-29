using Geidai.Common.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// Play 時に魔法使い UI（師匠・弟子・図鑑楽器）を自動配置する。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    public class SoundMatchStoryboardUiBootstrap : MonoBehaviour
    {
        [SerializeField] private SoundMatchGameController controller;

        private void Awake()
        {
            EnsureStoryboardBuilt();
        }

        private void Start()
        {
            // 実機（iPad 等）で SafeArea 確定後に再配置する。
            EnsureStoryboardBuilt();
        }

        /// <summary>魔法使い UI を構築（冪等）。Controller.OnShow からも呼ぶ。</summary>
        public void EnsureStoryboardBuilt()
        {
            if (controller == null)
                controller = GetComponent<SoundMatchGameController>();
            if (controller == null) return;

            var playArea = FindPlayArea();
            if (playArea == null) return;

            HideLegacyPond(playArea);
            HideLegacyFrog(playArea);
            UpdateHint(playArea);
            DisableLegacyGrowthPresentation();
            ClearLegacyTadpoleArt(playArea, controller.Config);

            ApplyStoryboardScale(playArea, controller);
            EnsureStageBackground(playArea);
            var back = playArea.Find("Back") as RectTransform;
            Game1StoryboardLayout.ApplyBackButtonLayout(back);

            bool storyboardMissing =
                playArea.Find("WizardStage") == null ||
                playArea.Find("ApprenticeStage") == null;

            if (controller.HasAnswerPad && !storyboardMissing)
            {
                RefreshApprenticeSnapLayout(playArea);
                RefreshWizardHeldLayout(playArea);
                RefreshWizardPortraits(playArea, controller);
                RepositionChoices(playArea, controller);
                return;
            }

            var config = controller.Config;
            var wizard = Game1WizardArt.ResolveMaster(config);
            var apprentice = Game1WizardArt.ResolveApprentice(config);

            var left = EnsureStage(playArea, "WizardStage", Game1StoryboardLayout.WizardAnchor);
            var right = EnsureStage(playArea, "ApprenticeStage", Game1StoryboardLayout.ApprenticeAnchor);

            var wizardView = EnsureWizard(left, wizard, controller);
            var answer = EnsureAnswerPad(right, apprentice);

            controller.ApplyStoryboardReferences(wizardView, answer, playArea, left, right);
            RepositionChoices(playArea, controller);
        }

        private static void RefreshWizardPortraits(RectTransform playArea, SoundMatchGameController controller)
        {
            if (playArea == null || controller == null) return;
            var config = controller.Config;
            var wizard = Game1WizardArt.ResolveMaster(config);
            var apprentice = Game1WizardArt.ResolveApprentice(config);

            var masterStage = playArea.Find("WizardStage");
            if (masterStage != null)
            {
                var masterTr = masterStage.Find("WizardMaster");
                if (masterTr != null)
                {
                    var img = masterTr.GetComponent<Image>();
                    if (img != null && wizard != null)
                    {
                        img.sprite = wizard;
                        img.enabled = true;
                        Game1CharacterImage.Apply(img);
                    }

                    var view = masterTr.GetComponent<FrogTargetView>();
                    if (view != null)
                        view.Setup(controller, wizard, null);
                }
            }

            var apprenticeStage = playArea.Find("ApprenticeStage");
            if (apprenticeStage != null)
            {
                var appTr = apprenticeStage.Find("Apprentice");
                if (appTr != null)
                {
                    var appImg = appTr.GetComponent<Image>();
                    if (appImg != null && apprentice != null)
                    {
                        appImg.sprite = apprentice;
                        appImg.enabled = true;
                        Game1CharacterImage.Apply(appImg);
                    }
                }
            }
        }

        private void DisableLegacyGrowthPresentation()
        {
            var fx = GetComponent<ResultEffectController>();
            if (fx != null)
                fx.SetLegacyGrowthEnabled(false);
        }

        private static void RefreshApprenticeSnapLayout(RectTransform playArea)
        {
            if (playArea == null) return;
            var stage = playArea.Find("ApprenticeStage");
            if (stage == null) return;
            var pad = stage.Find("AnswerPad");
            if (pad == null) return;
            var snap = pad.Find("Snap") as RectTransform;
            Game1StoryboardLayout.ApplyAnswerSnapLayout(snap);
        }

        private static void RefreshWizardHeldLayout(RectTransform playArea)
        {
            if (playArea == null) return;
            var stage = playArea.Find("WizardStage") as RectTransform;
            if (stage == null) return;
            var held = stage.Find(Game1WizardHeldInstrument.ChildName) as RectTransform;
            Game1StoryboardLayout.ApplyHeldInstrumentLayout(held);
            Game1WizardHeldInstrument.Hide(stage);
        }

        private static RectTransform FindPlayArea()
        {
            var choices = Object.FindAnyObjectByType<ChoiceItemView>();
            if (choices != null)
                return choices.transform.parent as RectTransform;

            var fitter = Object.FindAnyObjectByType<Geidai.Common.UI.SafeAreaFitter>();
            return fitter != null ? fitter.transform as RectTransform : null;
        }

        private static void EnsureStageBackground(RectTransform playArea)
        {
            var existing = playArea.Find("StageBackground") as RectTransform;
            RectTransform rt;
            if (existing == null)
            {
                var go = new GameObject("StageBackground", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(playArea, false);
                go.transform.SetAsFirstSibling();
                rt = go.GetComponent<RectTransform>();
                var img = go.GetComponent<Image>();
                img.color = Game1StoryboardLayout.StageBackground;
                img.raycastTarget = false;
            }
            else
            {
                rt = existing;
                existing.SetAsFirstSibling();
                existing.GetComponent<Image>().color = Game1StoryboardLayout.StageBackground;
            }

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void HideLegacyPond(RectTransform playArea)
        {
            var water = playArea.Find("PondWater");
            if (water != null) water.gameObject.SetActive(false);
            var lilies = new[] { "LilyPad_Left", "LilyPad_Answer" };
            for (int i = 0; i < lilies.Length; i++)
            {
                var lily = playArea.Find(lilies[i]);
                if (lily != null) lily.gameObject.SetActive(false);
            }
        }

        private static RectTransform EnsureStage(RectTransform playArea, string name, Vector2 anchor)
        {
            var rt = playArea.Find(name) as RectTransform;
            if (rt == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(playArea, false);
                rt = go.GetComponent<RectTransform>();
            }

            rt.gameObject.SetActive(true);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = Game1StoryboardLayout.WizardCharacterSize;
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        private static FrogTargetView EnsureWizard(
            RectTransform stage,
            Sprite wizardSprite,
            SoundMatchGameController controller)
        {
            var tr = stage.Find("WizardMaster") as RectTransform;
            GameObject go;
            if (tr == null)
            {
                go = new GameObject("WizardMaster", typeof(RectTransform), typeof(Image), typeof(FrogTargetView));
                go.transform.SetParent(stage, false);
                tr = go.GetComponent<RectTransform>();
            }
            else
            {
                go = tr.gameObject;
            }

            tr.anchorMin = new Vector2(0.5f, 0.5f);
            tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = Game1StoryboardLayout.WizardCharacterSize;
            tr.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            if (wizardSprite != null)
            {
                img.sprite = wizardSprite;
                img.enabled = true;
                Game1CharacterImage.Apply(img);
            }
            else
            {
                img.sprite = null;
                img.enabled = false;
                Debug.LogWarning("[Geidai] Game1: 師匠スプライトがありません。Resources/Geidai/Game1/WizardMaster または SoundMatchConfig を確認してください。");
            }

            var view = go.GetComponent<FrogTargetView>();
            view.Setup(controller, wizardSprite, null);
            Game1WizardHeldInstrument.Ensure(stage);
            Game1WizardHeldInstrument.Hide(stage);
            var held = stage.Find(Game1WizardHeldInstrument.ChildName) as RectTransform;
            Game1StoryboardLayout.ApplyHeldInstrumentLayout(held);
            return view;
        }

        private static SoundMatchAnswerPadView EnsureAnswerPad(RectTransform stage, Sprite apprenticeSprite)
        {
            var apprenticeTr = stage.Find("Apprentice") as RectTransform;
            if (apprenticeTr == null)
            {
                var appGo = new GameObject("Apprentice", typeof(RectTransform), typeof(Image));
                appGo.transform.SetParent(stage, false);
                apprenticeTr = appGo.GetComponent<RectTransform>();
            }

            apprenticeTr.anchorMin = new Vector2(0.5f, 0.5f);
            apprenticeTr.anchorMax = new Vector2(0.5f, 0.5f);
            apprenticeTr.sizeDelta = Game1StoryboardLayout.ApprenticeCharacterSize;
            apprenticeTr.anchoredPosition = Vector2.zero;
            var appImg = apprenticeTr.GetComponent<Image>();
            if (apprenticeSprite != null)
            {
                appImg.sprite = apprenticeSprite;
                appImg.enabled = true;
                appImg.preserveAspect = true;
                appImg.raycastTarget = false;
                Game1CharacterImage.Apply(appImg);
            }
            else
            {
                appImg.sprite = null;
                appImg.enabled = false;
                Debug.LogWarning("[Geidai] Game1: 弟子スプライトがありません。Resources/Geidai/Game1/WizardApprentice または SoundMatchConfig を確認してください。");
            }

            var padRoot = stage.Find("AnswerPad") as RectTransform;
            if (padRoot == null)
            {
                var go = new GameObject("AnswerPad", typeof(RectTransform), typeof(SoundMatchAnswerPadView));
                go.transform.SetParent(stage, false);
                padRoot = go.GetComponent<RectTransform>();
            }

            padRoot.anchorMin = Vector2.zero;
            padRoot.anchorMax = Vector2.one;
            padRoot.offsetMin = Vector2.zero;
            padRoot.offsetMax = Vector2.zero;

            var snap = padRoot.Find("Snap") as RectTransform;
            if (snap == null)
            {
                var snapGo = new GameObject("Snap", typeof(RectTransform));
                snapGo.transform.SetParent(padRoot, false);
                snap = snapGo.GetComponent<RectTransform>();
            }

            Game1StoryboardLayout.ApplyAnswerSnapLayout(snap);

            var pad = padRoot.GetComponent<SoundMatchAnswerPadView>();
            pad.Configure(padRoot, snap);
            return pad;
        }

        private static void HideLegacyFrog(RectTransform playArea)
        {
            var legacy = playArea.Find("Frog");
            if (legacy != null)
            {
                var img = legacy.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = null;
                    img.enabled = false;
                }

                legacy.gameObject.SetActive(false);
            }

            var demo = playArea.Find("DemoFrog");
            if (demo != null) demo.gameObject.SetActive(false);
        }

        private static void ClearLegacyTadpoleArt(RectTransform playArea, SoundMatchConfig config)
        {
            if (config == null) return;
            Game1LegacyTadpoleMotif.ClearFromChoiceViews(playArea, config);
        }

        private static void UpdateHint(RectTransform playArea)
        {
            var hintTr = playArea.Find("Hint");
            if (hintTr == null) return;
            var text = hintTr.GetComponent<Text>();
            if (text == null) return;
            text.text =
                "おんが なっている どうぐを タップで きいて、弟子の てもとへ ドラッグ。師匠 → どうぐ の おと で あわせてね";
            var rt = hintTr as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.05f, 0.38f);
                rt.anchorMax = new Vector2(0.95f, 0.44f);
            }
        }

        private static void ApplyStoryboardScale(RectTransform playArea, SoundMatchGameController controller)
        {
            RepositionChoices(playArea, controller);
        }

        private static void RepositionChoices(RectTransform playArea, SoundMatchGameController controller)
        {
            var choices = playArea.GetComponentsInChildren<ChoiceItemView>(true);
            int count = choices.Length;
            for (int i = 0; i < choices.Length; i++)
            {
                var rt = choices[i].transform as RectTransform;
                if (rt == null) continue;
                rt.sizeDelta = Game1StoryboardLayout.ChoiceSize;
                rt.anchorMin = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                rt.anchorMax = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                float x = Game1StoryboardLayout.ChoiceX(i, count);
                rt.anchoredPosition = new Vector2(x, 0f);
            }
        }
    }
}
