using Geidai.Common.Game;
using Geidai.Common.Library;
using Geidai.Common.UI;
using Geidai.Foundation;
using Geidai.Game2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.EditorTools
{
    /// <summary>②神経衰弱シーンとゲーム選択導線の生成。</summary>
    public static class Game2SceneBuilder
    {
        private const string SceneDir = "Assets/Scenes/Geidai";
        private const string PrefabDir = "Assets/Prefabs/Geidai";
        private const string MemoryMatchConfigPath = "Assets/Settings/MemoryMatchConfig.asset";
        private const string CuratedSoundCatalogPath = "Assets/Settings/CuratedSoundCatalog_Default.asset";
        private const string CardPrefabPath = "Assets/Prefabs/Geidai/MemoryMatchCard.prefab";

        [MenuItem("Geidai/Scenes/Build Game2 (Memory Match)")]
        public static void BuildGame2Scene()
        {
            GeidaiSceneBootstrap.EnsureFolders();
            EnsureConfig();
            var cardPrefab = EnsureCardPrefab();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = GeidaiSceneBootstrap.CreateScreenShellPublic("GeidaiGame2Root");
            var content = shell.safeArea;

            var title = GeidaiSceneBootstrap.CreateTextPublic(content, "Title", "② しんけいすいじゃく", 40, TextAnchor.MiddleCenter);
            GeidaiSceneBootstrap.AnchorTopBandPublic(title.rectTransform, 100f, 28f);

            var status = GeidaiSceneBootstrap.CreateTextPublic(content, "Status", "", 26, TextAnchor.MiddleCenter);
            var statusRt = status.rectTransform;
            statusRt.anchorMin = new Vector2(0.05f, 0.76f);
            statusRt.anchorMax = new Vector2(0.95f, 0.82f);
            statusRt.offsetMin = Vector2.zero;
            statusRt.offsetMax = Vector2.zero;

            var gridGo = new GameObject("CardGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(content, false);
            var gridRt = gridGo.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.05f, 0.2f);
            gridRt.anchorMax = new Vector2(0.95f, 0.74f);
            gridRt.offsetMin = Vector2.zero;
            gridRt.offsetMax = Vector2.zero;
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150, 180);
            grid.spacing = new Vector2(16, 16);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;

            var empty = new GameObject("EmptyState", typeof(RectTransform), typeof(Text));
            empty.transform.SetParent(content, false);
            var emptyText = empty.GetComponent<Text>();
            emptyText.text = "ペアの おとが まだ ないよ";
            emptyText.font = status.font;
            emptyText.fontSize = 28;
            emptyText.alignment = TextAnchor.MiddleCenter;
            GeidaiSceneBootstrap.StretchFullPublic(empty.GetComponent<RectTransform>());
            empty.SetActive(false);

            var error = GeidaiSceneBootstrap.CreateErrorPresenterPublic(content);
            var back = GeidaiSceneBootstrap.CreateButtonPublic(content, "Back", "もどる", new Vector2(200, 72));
            Game2ScreenLayout.ApplyBackButton(back.GetComponent<RectTransform>());

            var config = AssetDatabase.LoadAssetAtPath<MemoryMatchConfig>(MemoryMatchConfigPath);
            var catalog = AssetDatabase.LoadAssetAtPath<CuratedSoundCatalog>(CuratedSoundCatalogPath);

            var screenGo = new GameObject("MemoryMatchScreen", typeof(MemoryMatchGameController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var game = screenGo.GetComponent<MemoryMatchGameController>();
            GeidaiSceneBootstrap.WireScreenRootPublic(game, shell.responsive, shell.fitter);
            var so = new SerializedObject(game);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("curatedCatalog").objectReferenceValue = catalog;
            so.FindProperty("cardGrid").objectReferenceValue = gridRt;
            so.FindProperty("cardPrefab").objectReferenceValue = cardPrefab.GetComponent<MemoryMatchCardView>();
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("emptyState").objectReferenceValue = empty;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            GeidaiSceneBootstrap.SaveScenePublic("GeidaiGame2");
            GeidaiSceneBootstrap.UpdateBuildSettings();
            Debug.Log("[Game2SceneBuilder] GeidaiGame2 scene saved.");
        }

        [MenuItem("Geidai/Scenes/Wire Game Select (Game2 Button)")]
        public static void WireGameSelectGame2Button()
        {
            var scenePath = $"{SceneDir}/GeidaiGameSelect.unity";
            EditorSceneManager.OpenScene(scenePath);
            var ctrl = Object.FindFirstObjectByType<GameSelectScreenController>();
            if (ctrl == null)
            {
                Debug.LogError("[Game2SceneBuilder] GameSelectScreenController not found.");
                return;
            }

            var existing = GameObject.Find("Game2Button");
            Button game2Btn;
            if (existing != null)
            {
                game2Btn = existing.GetComponent<Button>();
            }
            else
            {
                var game3 = GameObject.Find("Game3Button");
                Transform parent = game3 != null ? game3.transform.parent : ctrl.transform.parent;
                var game2Go = Object.Instantiate(game3, parent);
                game2Go.name = "Game2Button";
                var label = game2Go.GetComponentInChildren<Text>();
                if (label != null) label.text = "② しんけいすいじゃく";
                game2Btn = game2Go.GetComponent<Button>();
            }

            HomeUiSceneBuilder.FixGameSelectMenuLayout();

            var so = new SerializedObject(ctrl);
            so.FindProperty("game2Button").objectReferenceValue = game2Btn;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(ctrl.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Game2SceneBuilder] Game2 button wired on Game Select.");
        }

        private static void EnsureConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<MemoryMatchConfig>(MemoryMatchConfigPath);
            if (config != null) return;
            config = ScriptableObject.CreateInstance<MemoryMatchConfig>();
            AssetDatabase.CreateAsset(config, MemoryMatchConfigPath);
            AssetDatabase.SaveAssets();
        }

        /// <summary>CI / batchmode: Unity -executeMethod Geidai.EditorTools.Game2SceneBuilder.BuildAllForBatch</summary>
        public static void BuildAllForBatch()
        {
            BuildGame2Scene();
            WireGameSelectGame2Button();
        }

        private static GameObject EnsureCardPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            if (existing != null) return existing;

            var root = new GameObject("MemoryMatchCard", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MemoryMatchCardView));
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 180);
            root.GetComponent<Image>().color = new Color(0.35f, 0.5f, 0.75f);

            var back = new GameObject("Back", typeof(RectTransform), typeof(Image));
            back.transform.SetParent(root.transform, false);
            GeidaiSceneBootstrap.StretchFullPublic(back.GetComponent<RectTransform>());
            back.GetComponent<Image>().color = new Color(0.25f, 0.4f, 0.65f);

            var front = new GameObject("Front", typeof(RectTransform), typeof(Image));
            front.transform.SetParent(root.transform, false);
            GeidaiSceneBootstrap.StretchFullPublic(front.GetComponent<RectTransform>());
            front.GetComponent<Image>().color = new Color(0.92f, 0.94f, 0.98f);
            front.SetActive(false);

            var label = GeidaiSceneBootstrap.CreateTextPublic(root.transform, "Label", "", 22, TextAnchor.MiddleCenter);
            GeidaiSceneBootstrap.StretchFullPublic(label.rectTransform);
            label.color = Color.black;

            var view = root.GetComponent<MemoryMatchCardView>();
            var so = new SerializedObject(view);
            so.FindProperty("cardButton").objectReferenceValue = root.GetComponent<Button>();
            so.FindProperty("backImage").objectReferenceValue = back.GetComponent<Image>();
            so.FindProperty("frontImage").objectReferenceValue = front.GetComponent<Image>();
            so.FindProperty("labelText").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!System.IO.Directory.Exists(PrefabDir))
                System.IO.Directory.CreateDirectory(PrefabDir);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
