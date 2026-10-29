using Geidai.Game1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.EditorTools
{
    /// <summary>
    /// ①音合わせを絵コンテどおり（左カエル・右に答え睡蓮・下におたま）に組み替える。
    /// </summary>
    public static class Game1PondLayoutSetup
    {
        private const string ScenePath = "Assets/Scenes/Geidai/GeidaiGame1.unity";
        private const string LilyPath = "Assets/Art/Game/Game1_Placeholder_LilyPad.png";
        private const string TadpolePath = "Assets/Art/Game/Game1_Placeholder_Tadpole.png";

        [MenuItem("Geidai/Game1/Apply Storyboard Pond Layout")]
        public static void ApplyStoryboardLayout()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var playArea = FindPlayArea();
            if (playArea == null)
            {
                Debug.LogError("[Geidai] Game1: SafeArea / play content not found.");
                return;
            }

            var lily = LoadSprite(LilyPath);
            var frogSprite = LoadSprite(Game1ArtPaths.WizardMaster);
            var tadpoleSprite = LoadSprite(TadpolePath);

            EnsureWater(playArea);
            var leftPad = EnsureLilyPad(playArea, "LilyPad_Left", Game1StoryboardLayout.LilyAnchorLeft, lily);
            var rightPad = EnsureLilyPad(playArea, "LilyPad_Answer", Game1StoryboardLayout.LilyAnchorAnswer, lily);

            var frogView = EnsureDemoFrog(leftPad, frogSprite);
            var answerPad = EnsureAnswerPad(rightPad);

            RepositionChoices(playArea, tadpoleSprite);
            UpdateHint(playArea);
            WireController(frogView, answerPad);
            var back = playArea.Find("Back") as RectTransform;
            Game1StoryboardLayout.ApplyBackButtonLayout(back);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Geidai] Game1 storyboard pond layout applied.");
        }

        private static RectTransform FindPlayArea()
        {
            var fitter = Object.FindFirstObjectByType<Geidai.Common.UI.SafeAreaFitter>();
            if (fitter != null)
                return fitter.transform as RectTransform;

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return null;
            if (canvas.transform.childCount == 0) return null;
            return canvas.transform.GetChild(0) as RectTransform;
        }

        private static void EnsureWater(RectTransform playArea)
        {
            var existing = playArea.Find("PondWater");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var go = new GameObject("PondWater", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(playArea, false);
            go.transform.SetAsFirstSibling();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, Game1StoryboardLayout.WaterSurfaceAnchorY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = new Color(0.75f, 0.9f, 0.98f, 0.55f);
            img.raycastTarget = false;
        }

        private static RectTransform EnsureLilyPad(
            RectTransform playArea,
            string name,
            Vector2 anchorCenter,
            Sprite lilySprite)
        {
            var tr = playArea.Find(name) as RectTransform;
            if (tr == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(playArea, false);
                tr = go.GetComponent<RectTransform>();
            }

            tr.anchorMin = anchorCenter;
            tr.anchorMax = anchorCenter;
            tr.sizeDelta = Game1StoryboardLayout.LilyPadSize;
            tr.anchoredPosition = Vector2.zero;
            Game1LilyPadVisual.Setup(tr, lilySprite);
            return tr;
        }

        private static FrogTargetView EnsureDemoFrog(RectTransform leftPad, Sprite frogSprite)
        {
            var frogTr = leftPad.Find("DemoFrog") as RectTransform;
            GameObject frogGo;
            if (frogTr == null)
            {
                frogGo = new GameObject("DemoFrog", typeof(RectTransform), typeof(Image), typeof(FrogTargetView));
                frogGo.transform.SetParent(leftPad, false);
                frogTr = frogGo.GetComponent<RectTransform>();
            }
            else
            {
                frogGo = frogTr.gameObject;
            }

            frogTr.anchorMin = new Vector2(0.5f, 0.55f);
            frogTr.anchorMax = new Vector2(0.5f, 0.55f);
            frogTr.sizeDelta = Game1StoryboardLayout.DemoFrogSize;
            frogTr.anchoredPosition = Vector2.zero;

            var img = frogGo.GetComponent<Image>();
            if (frogSprite != null)
            {
                img.sprite = frogSprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }

            var preview = frogTr.Find("Preview");
            Button previewBtn;
            if (preview == null)
            {
                previewBtn = Game1UiFactory.CreateButton(frogTr, "Preview", "きく", new Vector2(100f, 44f));
                previewBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -95f);
            }
            else
            {
                previewBtn = preview.GetComponent<Button>();
            }

            var frogView = frogGo.GetComponent<FrogTargetView>();
            var so = new SerializedObject(frogView);
            so.FindProperty("previewButton").objectReferenceValue = previewBtn;
            so.FindProperty("targetImage").objectReferenceValue = img;
            so.ApplyModifiedPropertiesWithoutUndo();

            var legacy = Object.FindFirstObjectByType<FrogTargetView>();
            if (legacy != null && legacy.gameObject.name == "Frog")
                legacy.gameObject.SetActive(false);

            return frogView;
        }

        private static SoundMatchAnswerPadView EnsureAnswerPad(RectTransform rightPad)
        {
            var padRoot = rightPad.Find("AnswerPad") as RectTransform;
            if (padRoot == null)
            {
                var go = new GameObject("AnswerPad", typeof(RectTransform), typeof(SoundMatchAnswerPadView));
                go.transform.SetParent(rightPad, false);
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

            var answer = padRoot.GetComponent<SoundMatchAnswerPadView>();
            var so = new SerializedObject(answer);
            so.FindProperty("dropArea").objectReferenceValue = padRoot;
            so.FindProperty("snapAnchor").objectReferenceValue = snap;
            so.ApplyModifiedPropertiesWithoutUndo();
            return answer;
        }

        private static void RepositionChoices(RectTransform playArea, Sprite tadpoleSprite)
        {
            var choices = Object.FindObjectsByType<ChoiceItemView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            float[] xs = Game1StoryboardLayout.ChoicePoolX;
            int i = 0;
            foreach (var choice in choices)
            {
                if (choice == null) continue;
                var rt = choice.transform as RectTransform;
                if (rt == null) continue;
                rt.anchorMin = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                rt.anchorMax = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                rt.sizeDelta = Game1StoryboardLayout.ChoiceSize;
                rt.anchoredPosition = new Vector2(i < xs.Length ? xs[i] : 0f, 0f);
                i++;

                var img = choice.GetComponent<Image>();
                if (img != null && tadpoleSprite != null)
                {
                    img.sprite = tadpoleSprite;
                    img.color = Color.white;
                    img.preserveAspect = true;
                }

                var so = new SerializedObject(choice);
                so.FindProperty("choiceImage").objectReferenceValue = img;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void UpdateHint(RectTransform playArea)
        {
            var hintTr = playArea.Find("Hint");
            if (hintTr == null) return;
            var text = hintTr.GetComponent<Text>();
            if (text == null) return;
            text.text = "おたまを 右の 睡蓮へ ドラッグ。カエルの おと → おたまの おと で あわせてね";
            var rt = hintTr as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.05f, 0.44f);
                rt.anchorMax = new Vector2(0.95f, 0.5f);
            }
        }

        private static void WireController(FrogTargetView frog, SoundMatchAnswerPadView answerPad)
        {
            var game = Object.FindFirstObjectByType<SoundMatchGameController>();
            if (game == null) return;
            var so = new SerializedObject(game);
            so.FindProperty("frog").objectReferenceValue = frog;
            so.FindProperty("answerPad").objectReferenceValue = answerPad;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite LoadSprite(string path) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    internal static class Game1UiFactory
    {
        public static Button CreateButton(Transform parent, string name, string label, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.85f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 22;
            text.color = Color.black;
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }
    }
}
