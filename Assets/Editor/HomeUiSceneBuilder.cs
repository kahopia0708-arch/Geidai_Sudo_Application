using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Geidai.Common.UI;
using Geidai.Foundation;
using Geidai.Services;

namespace Geidai.EditorTools
{
    /// <summary>
    /// ホーム UI 整備（2026-08）用のシーン／アセット生成。
    /// </summary>
    public static class HomeUiSceneBuilder
    {
        private const string SceneDir = "Assets/Scenes/Geidai";
        private const string PrefabDir = "Assets/Prefabs/Geidai";
        private const string ArtDir = "Assets/Art/Home/Placeholders";
        private const string HomeMenuConfigPath = "Assets/Settings/HomeMenuConfig_Default.asset";
        private const string IconCatalogPath = "Assets/Settings/HomeMenuIconCatalog_Default.asset";

        private static readonly Color HomeBg = new Color(0.478f, 0.580f, 0.722f, 1f);
        private static readonly Color MenuText = new Color(0.22f, 0.32f, 0.45f, 1f);

        [MenuItem("Geidai/Scenes/Build Home UI (Redesign)")]
        public static void BuildAllHomeUi()
        {
            GeidaiSceneBootstrap.EnsureFolders();
            EnsureArtFolder();
            EnsureIconCatalog();
            EnsureHomeMenuConfig();
            BuildHomeScene();
            BuildGameSelectScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HomeUiSceneBuilder] Home UI build complete.");
        }

        private static void EnsureArtFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art"))
                AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder("Assets/Art/Home"))
                AssetDatabase.CreateFolder("Assets/Art", "Home");
            if (!AssetDatabase.IsValidFolder(ArtDir))
                AssetDatabase.CreateFolder("Assets/Art/Home", "Placeholders");
        }

        private static void EnsureIconCatalog()
        {
            ConfigurePlaceholderTextures();

            var catalog = AssetDatabase.LoadAssetAtPath<HomeMenuIconCatalog>(IconCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<HomeMenuIconCatalog>();
                AssetDatabase.CreateAsset(catalog, IconCatalogPath);
            }

            var rounded = LoadSprite($"{ArtDir}/rounded_white.png");
            var gather = LoadSprite($"{ArtDir}/gather.png");
            var create = LoadSprite($"{ArtDir}/create.png");
            var library = LoadSprite($"{ArtDir}/library.png");

            var so = new SerializedObject(catalog);
            var entries = so.FindProperty("entries");
            entries.ClearArray();
            AddEntry(entries, "rounded", rounded);
            AddEntry(entries, "pill", LoadSprite($"{ArtDir}/menu_button_pill.png"));
            AddEntry(entries, "gather", gather);
            AddEntry(entries, "create", create);
            AddEntry(entries, "library", library);
            AddEntry(entries, "settings", LoadSprite($"{ArtDir}/settings_gear.png"));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static void AddEntry(SerializedProperty entries, string key, Sprite sprite)
        {
            if (sprite == null) return;
            int i = entries.arraySize;
            entries.InsertArrayElementAtIndex(i);
            var el = entries.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("key").stringValue = key;
            el.FindPropertyRelative("sprite").objectReferenceValue = sprite;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureHomeMenuConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<HomeMenuConfig>(HomeMenuConfigPath);
            if (config == null)
            {
                Debug.LogError("[HomeUiSceneBuilder] HomeMenuConfig not found.");
                return;
            }

            var items = new List<HomeMenuItem>
            {
                new HomeMenuItem { moduleId = ModuleId.Collection, label = "おとあつめ", iconKey = "gather", visible = true, enabled = true, order = 0 },
                new HomeMenuItem { moduleId = ModuleId.GameSelect, label = "おとあそび", iconKey = "", visible = true, enabled = true, order = 1 },
                new HomeMenuItem { moduleId = ModuleId.Create, label = "おとつくり", iconKey = "create", visible = true, enabled = true, order = 2 },
                new HomeMenuItem { moduleId = ModuleId.Library, label = "おとずかん", iconKey = "library", visible = true, enabled = true, order = 3 },
            };

            var so = new SerializedObject(config);
            var prop = so.FindProperty("items");
            prop.ClearArray();
            for (int i = 0; i < items.Count; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                var el = prop.GetArrayElementAtIndex(i);
                var item = items[i];
                el.FindPropertyRelative("moduleId").enumValueIndex = (int)item.moduleId;
                el.FindPropertyRelative("label").stringValue = item.label;
                el.FindPropertyRelative("iconKey").stringValue = item.iconKey;
                el.FindPropertyRelative("visible").boolValue = item.visible;
                el.FindPropertyRelative("enabled").boolValue = item.enabled;
                el.FindPropertyRelative("order").intValue = item.order;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        public static void BuildHomeScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiHomeRoot");
            var content = shell.safeArea;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(content, false);
            StretchFull(bg.GetComponent<RectTransform>());
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = HomeBg;
            bgImg.raycastTarget = false;

            var menuContainerGo = new GameObject("MenuContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            menuContainerGo.transform.SetParent(content, false);
            menuContainerGo.SetActive(true);
            var menuRt = menuContainerGo.GetComponent<RectTransform>();
            menuRt.anchorMin = new Vector2(0.06f, 0.10f);
            menuRt.anchorMax = new Vector2(0.94f, 0.82f);
            menuRt.offsetMin = Vector2.zero;
            menuRt.offsetMax = Vector2.zero;
            var vlg = menuContainerGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 28;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var prefabBtn = CreateHomeMenuButton(content, "MenuButtonPrefab");
            prefabBtn.gameObject.SetActive(false);
            SavePrefab(prefabBtn.gameObject, $"{PrefabDir}/HomeMenuButton.prefab");

            var profileBadge = CreateProfileBadge(content);
            SavePrefab(profileBadge, $"{PrefabDir}/HomeProfileBadge.prefab");

            var profilePanel = CreateProfilePanel(shell.canvas.transform);
            SavePrefab(profilePanel, $"{PrefabDir}/HomeProfilePanel.prefab");

            var error = CreateErrorPresenter(content);
            var confirm = CreateConfirmDialog(content);

            var screenGo = new GameObject("HomeScreen", typeof(HomeScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var home = screenGo.GetComponent<HomeScreenController>();
            WireScreenRoot(home, shell.responsive, shell.fitter);

            var menuConfig = AssetDatabase.LoadAssetAtPath<HomeMenuConfig>(HomeMenuConfigPath);
            var iconCatalog = AssetDatabase.LoadAssetAtPath<HomeMenuIconCatalog>(IconCatalogPath);
            var badgeInstance = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/HomeProfileBadge.prefab"));
            badgeInstance.transform.SetParent(content, false);
            var panelInstance = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/HomeProfilePanel.prefab"));
            panelInstance.transform.SetParent(shell.canvas.transform, false);

            var so = new SerializedObject(home);
            so.FindProperty("menuConfig").objectReferenceValue = menuConfig;
            so.FindProperty("iconCatalog").objectReferenceValue = iconCatalog;
            so.FindProperty("menuContainer").objectReferenceValue = menuContainerGo.transform;
            so.FindProperty("menuButtonPrefab").objectReferenceValue = prefabBtn;
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("profileBadge").objectReferenceValue = badgeInstance.GetComponent<HomeProfileBadgeView>();
            so.FindProperty("profilePanel").objectReferenceValue = panelInstance.GetComponent<HomeProfilePanelView>();
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.FindProperty("confirmDialog").objectReferenceValue = confirm;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiHome");
        }

        public static void BuildGameSelectScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiGameSelectRoot");
            var content = shell.safeArea;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(content, false);
            StretchFull(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().color = HomeBg;

            var title = CreateText(content, "Title", "おとあそび", 44, TextAnchor.MiddleCenter);
            title.color = Color.white;
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.1f, 0.85f);
            titleRt.anchorMax = new Vector2(0.9f, 0.95f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            var game1 = CreateWhiteMenuButton(content, "Game1Button", "① おとあわせ", new Vector2(680, 100));
            var g1Rt = game1.GetComponent<RectTransform>();
            g1Rt.anchorMin = new Vector2(0.5f, 0.55f);
            g1Rt.anchorMax = new Vector2(0.5f, 0.55f);
            g1Rt.anchoredPosition = Vector2.zero;

            var back = CreateWhiteMenuButton(content, "BackButton", "ホームにもどる", new Vector2(320, 80));
            var backRt = back.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.5f, 0.08f);
            backRt.anchorMax = new Vector2(0.5f, 0.08f);
            backRt.anchoredPosition = Vector2.zero;

            var error = CreateErrorPresenter(content);

            var screenGo = new GameObject("GameSelectScreen", typeof(GameSelectScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var ctrl = screenGo.GetComponent<GameSelectScreenController>();
            WireScreenRoot(ctrl, shell.responsive, shell.fitter);
            var so = new SerializedObject(ctrl);
            so.FindProperty("game1Button").objectReferenceValue = game1;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiGameSelect");
        }

        public static void UpdateBuildSettings()
        {
            var keepDisabled = new HashSet<string>
            {
                "Assets/Main画面.unity",
                "Assets/game_Home.unity",
                "Assets/Home.unity",
                "Assets/Rec.unity",
                "Assets/Game01.unity",
                "Assets/MySoundCollection.unity",
                "Assets/Place.unity",
                "Assets/Scenes/SampleScene.unity",
            };

            var enabledNew = new[]
            {
                $"{SceneDir}/GeidaiHome.unity",
                $"{SceneDir}/GeidaiRegister.unity",
                $"{SceneDir}/GeidaiRec.unity",
                $"{SceneDir}/GeidaiCollection.unity",
                $"{SceneDir}/GeidaiTheme.unity",
                $"{SceneDir}/GeidaiGameSelect.unity",
                $"{SceneDir}/GeidaiGame1.unity",
                $"{SceneDir}/GeidaiLibrary.unity",
                $"{SceneDir}/GeidaiCreate.unity",
            };

            var list = new List<EditorBuildSettingsScene>();
            foreach (var path in enabledNew)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                    list.Add(new EditorBuildSettingsScene(path, true));
                else
                    Debug.LogWarning($"[HomeUiSceneBuilder] missing scene: {path}");
            }

            foreach (var path in keepDisabled)
            {
                if (File.Exists(path))
                    list.Add(new EditorBuildSettingsScene(path, false));
            }

            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (list.Exists(s => s.path == existing.path)) continue;
                list.Add(new EditorBuildSettingsScene(existing.path, false));
            }

            EditorBuildSettings.scenes = list.ToArray();
        }

        // ---------- UI helpers (mirrors GeidaiSceneBootstrap) ----------

        private static (Canvas canvas, RectTransform safeArea, ResponsiveCanvasConfigurator responsive, SafeAreaFitter fitter, AppManager app) CreateScreenShell(string rootName)
        {
            var root = new GameObject(rootName);
            var app = root.AddComponent<AppManager>();
            var soApp = new SerializedObject(app);
            soApp.FindProperty("navigateOnStart").boolValue = false;
            soApp.ApplyModifiedPropertiesWithoutUndo();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster),
                typeof(ResponsiveCanvasConfigurator));
            canvasGo.transform.SetParent(root.transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.GetComponent<ResponsiveCanvasConfigurator>().Configure();

            var safeGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeGo.transform.SetParent(canvasGo.transform, false);
            StretchFull(safeGo.GetComponent<RectTransform>());

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                es.transform.SetParent(root.transform, false);
            }

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(root.transform, false);
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = HomeBg;
            cam.orthographic = true;

            return (canvas, safeGo.GetComponent<RectTransform>(), canvasGo.GetComponent<ResponsiveCanvasConfigurator>(),
                safeGo.GetComponent<SafeAreaFitter>(), app);
        }

        private static void WireScreenRoot(ScreenRootBase screen, ResponsiveCanvasConfigurator responsive, SafeAreaFitter fitter)
        {
            var so = new SerializedObject(screen);
            so.FindProperty("responsiveConfigurator").objectReferenceValue = responsive;
            so.FindProperty("safeAreaFitter").objectReferenceValue = fitter;
            so.FindProperty("showOnStart").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateHomeMenuButton(Transform parent, string name)
        {
            var pill = LoadSprite($"{ArtDir}/menu_button_pill.png");
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(HomeMenuButtonView));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(680, 120);
            var img = go.GetComponent<Image>();
            img.sprite = pill;
            img.type = pill != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = Color.white;
            img.pixelsPerUnitMultiplier = 1f;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            ConfigureButtonColors(btn);

            var iconRoot = new GameObject("IconRoot", typeof(RectTransform));
            iconRoot.transform.SetParent(go.transform, false);
            var iconRootRt = iconRoot.GetComponent<RectTransform>();
            iconRootRt.anchorMin = new Vector2(0f, 0.5f);
            iconRootRt.anchorMax = new Vector2(0f, 0.5f);
            iconRootRt.sizeDelta = new Vector2(96, 96);
            iconRootRt.anchoredPosition = new Vector2(64f, 0f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(iconRoot.transform, false);
            StretchFull(iconGo.GetComponent<RectTransform>());
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var label = CreateText(go.transform, "Label", "メニュー", 48, TextAnchor.MiddleLeft);
            var labelRt = label.rectTransform;
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.offsetMin = new Vector2(140f, 0f);
            labelRt.offsetMax = new Vector2(-24f, 0f);
            label.color = MenuText;
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = false;

            var view = go.GetComponent<HomeMenuButtonView>();
            var vso = new SerializedObject(view);
            vso.FindProperty("iconImage").objectReferenceValue = iconImg;
            vso.FindProperty("labelText").objectReferenceValue = label;
            vso.FindProperty("iconRoot").objectReferenceValue = iconRoot;
            vso.ApplyModifiedPropertiesWithoutUndo();

            return btn;
        }

        private static Button CreateIconButton(Transform parent, string name, Sprite icon)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            if (icon != null)
            {
                img.sprite = icon;
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
                img.color = MenuText;
            }
            else
            {
                img.color = new Color(0f, 0f, 0f, 0f);
            }

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            ConfigureButtonColors(btn);
            return btn;
        }

        private static Button CreateWhiteMenuButton(Transform parent, string name, string label, Vector2 size)
        {
            var pill = LoadSprite($"{ArtDir}/menu_button_pill.png");
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = pill;
            img.type = pill != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = Color.white;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            ConfigureButtonColors(btn);
            var text = CreateText(go.transform, "Label", label, 36, TextAnchor.MiddleCenter);
            StretchFull(text.rectTransform);
            text.color = MenuText;
            text.fontStyle = FontStyle.Bold;
            return btn;
        }

        private static GameObject CreateProfileBadge(Transform parent)
        {
            var rounded = LoadSprite($"{ArtDir}/rounded_white.png");
            var go = new GameObject("ProfileBadge", typeof(RectTransform), typeof(Image), typeof(Button), typeof(HomeProfileBadgeView));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(200, 72);
            rt.anchoredPosition = new Vector2(-16f, -16f);

            var img = go.GetComponent<Image>();
            img.sprite = rounded;
            img.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = Color.white;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            ConfigureButtonColors(btn);

            var nick = CreateText(go.transform, "Nickname", "かほ", 26, TextAnchor.UpperCenter);
            var nickRt = nick.rectTransform;
            nickRt.anchorMin = new Vector2(0f, 0.45f);
            nickRt.anchorMax = new Vector2(1f, 1f);
            nickRt.offsetMin = new Vector2(8f, 0f);
            nickRt.offsetMax = new Vector2(-8f, -6f);
            nick.color = MenuText;
            nick.fontStyle = FontStyle.Bold;

            var segRoot = new GameObject("ProgressSegments", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            segRoot.transform.SetParent(go.transform, false);
            var segRt = segRoot.GetComponent<RectTransform>();
            segRt.anchorMin = new Vector2(0.08f, 0.12f);
            segRt.anchorMax = new Vector2(0.92f, 0.38f);
            segRt.offsetMin = Vector2.zero;
            segRt.offsetMax = Vector2.zero;
            var hlg = segRoot.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int i = 0; i < 6; i++)
            {
                var seg = new GameObject($"Seg{i}", typeof(RectTransform), typeof(Image));
                seg.transform.SetParent(segRoot.transform, false);
                seg.GetComponent<Image>().color = i % 2 == 0
                    ? new Color(0.94f, 0.78f, 0.20f, 1f)
                    : new Color(0.75f, 0.78f, 0.82f, 1f);
            }

            var view = go.GetComponent<HomeProfileBadgeView>();
            var vso = new SerializedObject(view);
            vso.FindProperty("button").objectReferenceValue = btn;
            vso.FindProperty("badgeBackground").objectReferenceValue = img;
            vso.FindProperty("nicknameText").objectReferenceValue = nick;
            vso.FindProperty("progressSegmentsRoot").objectReferenceValue = segRoot.transform;
            vso.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static GameObject CreateProfilePanel(Transform canvasTransform)
        {
            var root = new GameObject("ProfilePanel", typeof(RectTransform), typeof(HomeProfilePanelView));
            root.transform.SetParent(canvasTransform, false);
            StretchFull(root.GetComponent<RectTransform>());

            var blocker = new GameObject("Blocker", typeof(RectTransform), typeof(Image), typeof(Button));
            blocker.transform.SetParent(root.transform, false);
            StretchFull(blocker.GetComponent<RectTransform>());
            blocker.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var blockerBtn = blocker.GetComponent<Button>();
            blockerBtn.transition = Selectable.Transition.None;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.08f, 0.18f);
            panelRt.anchorMax = new Vector2(0.92f, 0.88f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = panel.GetComponent<Image>();
            var rounded = LoadSprite($"{ArtDir}/rounded_white.png");
            panelImg.sprite = rounded;
            panelImg.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            panelImg.color = Color.white;

            var title = CreateText(panel.transform, "Title", "かほ のプロフィール", 40, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            title.color = MenuText;
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.06f, 0.84f);
            titleRt.anchorMax = new Vector2(0.76f, 0.94f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            CreateStatRow(panel.transform, "StatSounds", "いままであつめたおと", "—", 0.72f);
            CreateStatRow(panel.transform, "StatPoints", "いままであつめたポイント", "—", 0.60f);
            CreateStatRow(panel.transform, "StatUntil", "あたらしい音まであと", "—", 0.48f);

            var gearSprite = LoadSprite($"{ArtDir}/settings_gear.png");
            var settings = CreateIconButton(panel.transform, "SettingsButton", gearSprite);
            var sRt = settings.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.80f, 0.84f);
            sRt.anchorMax = new Vector2(0.94f, 0.94f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = Vector2.zero;

            var close = CreateWhiteMenuButton(panel.transform, "CloseButton", "とじる", new Vector2(280, 72));
            var cRt = close.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.05f);
            cRt.anchorMax = new Vector2(0.5f, 0.05f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;

            var view = root.GetComponent<HomeProfilePanelView>();
            var vso = new SerializedObject(view);
            vso.FindProperty("root").objectReferenceValue = root;
            vso.FindProperty("contentRoot").objectReferenceValue = panel.transform;
            vso.FindProperty("panelBackground").objectReferenceValue = panelImg;
            vso.FindProperty("settingsButtonBackground").objectReferenceValue = settings.GetComponent<Image>();
            vso.FindProperty("closeButtonBackground").objectReferenceValue = close.GetComponent<Image>();
            vso.FindProperty("titleText").objectReferenceValue = title;
            vso.FindProperty("soundsCollectedValueText").objectReferenceValue =
                panel.transform.Find("StatSounds/Value")?.GetComponent<Text>();
            vso.FindProperty("pointsCollectedValueText").objectReferenceValue =
                panel.transform.Find("StatPoints/Value")?.GetComponent<Text>();
            vso.FindProperty("untilNewSoundValueText").objectReferenceValue =
                panel.transform.Find("StatUntil/Value")?.GetComponent<Text>();
            vso.FindProperty("closeButton").objectReferenceValue = close;
            vso.FindProperty("settingsButton").objectReferenceValue = settings;
            vso.FindProperty("backdropButton").objectReferenceValue = blockerBtn;
            vso.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return root;
        }

        private static void CreateStatRow(Transform parent, string name, string label, string value, float anchorY)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, anchorY - 0.08f);
            rt.anchorMax = new Vector2(0.92f, anchorY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var labelText = CreateText(row.transform, "Label", label, 28, TextAnchor.MiddleLeft);
            StretchFull(labelText.rectTransform);
            labelText.color = MenuText;

            var valueText = CreateText(row.transform, "Value", value, 28, TextAnchor.MiddleRight);
            StretchFull(valueText.rectTransform);
            valueText.color = MenuText;
        }

        private static ErrorPresenter CreateErrorPresenter(Transform parent)
        {
            var banner = new GameObject("ErrorBanner", typeof(RectTransform), typeof(Image));
            banner.transform.SetParent(parent, false);
            var rt = banner.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.85f);
            rt.anchorMax = new Vector2(0.9f, 0.95f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            banner.GetComponent<Image>().color = new Color(0.9f, 0.3f, 0.3f, 0.85f);
            banner.SetActive(false);

            var msg = CreateText(banner.transform, "Message", "", 24, TextAnchor.MiddleCenter);
            StretchFull(msg.rectTransform);
            msg.color = Color.white;

            var host = new GameObject("ErrorPresenter", typeof(ErrorPresenter));
            host.transform.SetParent(parent, false);
            var ep = host.GetComponent<ErrorPresenter>();
            var so = new SerializedObject(ep);
            so.FindProperty("banner").objectReferenceValue = banner;
            so.FindProperty("messageText").objectReferenceValue = msg;
            so.ApplyModifiedPropertiesWithoutUndo();
            return ep;
        }

        private static ConfirmDialog CreateConfirmDialog(Transform parent)
        {
            var panel = new GameObject("ConfirmDialog", typeof(RectTransform), typeof(Image), typeof(ConfirmDialog));
            panel.transform.SetParent(parent, false);
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.45f);
            panel.SetActive(false);

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(panel.transform, false);
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 360);
            box.GetComponent<Image>().color = Color.white;

            var title = CreateText(box.transform, "Title", "", 32, TextAnchor.MiddleCenter);
            title.rectTransform.anchoredPosition = new Vector2(0, 100);
            var message = CreateText(box.transform, "Message", "", 26, TextAnchor.MiddleCenter);
            message.rectTransform.anchoredPosition = new Vector2(0, 20);
            var yes = CreateWhiteMenuButton(box.transform, "Yes", "はい", new Vector2(200, 70));
            yes.GetComponent<RectTransform>().anchoredPosition = new Vector2(-140, -100);
            var no = CreateWhiteMenuButton(box.transform, "No", "いいえ", new Vector2(200, 70));
            no.GetComponent<RectTransform>().anchoredPosition = new Vector2(140, -100);

            var dialog = panel.GetComponent<ConfirmDialog>();
            var so = new SerializedObject(dialog);
            so.FindProperty("root").objectReferenceValue = panel;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("messageText").objectReferenceValue = message;
            so.FindProperty("yesButton").objectReferenceValue = yes;
            so.FindProperty("noButton").objectReferenceValue = no;
            so.ApplyModifiedPropertiesWithoutUndo();
            return dialog;
        }

        private static void ConfigurePlaceholderTextures()
        {
            if (!Directory.Exists(ArtDir)) return;
            ConfigureSprite($"{ArtDir}/menu_button_pill.png", new Vector4(64, 64, 64, 64));
            ConfigureSprite($"{ArtDir}/rounded_white.png", new Vector4(96, 96, 96, 96));
            foreach (var file in Directory.GetFiles(ArtDir, "*.png"))
            {
                var assetPath = ToAssetPath(file);
                if (assetPath.EndsWith("menu_button_pill.png") || assetPath.EndsWith("rounded_white.png"))
                    continue;
                ConfigureSprite(assetPath, Vector4.zero);
            }
        }

        private static string ToAssetPath(string file)
        {
            var p = file.Replace('\\', '/');
            var idx = p.IndexOf("Assets/");
            return idx >= 0 ? p.Substring(idx) : p;
        }

        private static void ConfigureSprite(string assetPath, Vector4 border)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsToUnits = 100;
            if (border != Vector4.zero) importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        private static void ConfigureButtonColors(Button btn)
        {
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.55f);
            btn.colors = colors;
        }

        private static Font ResolveUiFont(int fontSize)
        {
            var os = Font.CreateDynamicFontFromOSFont(new[]
            {
                "Hiragino Sans", "Hiragino Kaku Gothic ProN", "Yu Gothic UI", "Yu Gothic", "Meiryo",
                "Noto Sans CJK JP", "Apple SD Gothic Neo", "Arial Unicode MS", "Arial"
            }, fontSize);
            if (os != null) return os;
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 120);
            var text = go.GetComponent<Text>();
            text.text = content;
            text.font = ResolveUiFont(fontSize);
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = MenuText;
            text.raycastTarget = false;
            return text;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SavePrefab(GameObject source, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(source, path);
        }

        private static void SaveScene(string sceneName)
        {
            var path = $"{SceneDir}/{sceneName}.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
            Debug.Log($"[HomeUiSceneBuilder] Saved {path}");
        }
    }
}
