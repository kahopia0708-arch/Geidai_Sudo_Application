using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Geidai.Common.Content;
using Geidai.Common.Game;
using Geidai.Common.Library;
using Geidai.Common.UI;
using Geidai.Foundation;
using Geidai.Rec;
using Geidai.Collection;
using Geidai.Theme;
using Geidai.Game1;
using Geidai.Library;
using Geidai.Create;
using Geidai.Services;

namespace Geidai.EditorTools
{
    /// <summary>
    /// Geidai.* コントローラを配置した実シーン骨組みを生成する（MCP フォローアップ / US-TECH-05）。
    /// 意匠は最小枠のみ。見た目調整は Sさん（US-TECH-07）。
    /// </summary>
    public static class GeidaiSceneBootstrap
    {
        private const string SceneDir = "Assets/Scenes/Geidai";
        private const string PrefabDir = "Assets/Prefabs/Geidai";

        private const string HomeMenuConfigPath = "Assets/Settings/HomeMenuConfig_Default.asset";
        private const string ThemeCatalogPath = "Assets/Settings/ThemeCatalog.asset";
        private const string SoundMatchConfigPath = "Assets/Settings/SoundMatchConfig.asset";
        private const string CuratedSoundCatalogPath = "Assets/Settings/CuratedSoundCatalog_Default.asset";
        private const string UnlockRulesCatalogPath = "Assets/Settings/UnlockRulesCatalog_Default.asset";
        private const string TimbreTagCatalogPath = "Assets/Settings/TimbreTagCatalog_Default.asset";
        private const string LibraryPlaceholderPath = "Assets/Art/Library/Icons/placeholder.png";

        [MenuItem("Geidai/Scenes/Build All Geidai Scenes")]
        public static void BuildAll()
        {
            EnsureFolders();
            BuildHome();
            BuildRegister();
            BuildRec();
            BuildCollection();
            BuildTheme();
            BuildGame1();
            BuildLibrary();
            BuildCreate();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GeidaiSceneBootstrap] BuildAll complete.");
        }

        [MenuItem("Geidai/Scenes/Update Build Settings Only")]
        public static void UpdateBuildSettingsMenu()
        {
            UpdateBuildSettings();
            Debug.Log("[GeidaiSceneBootstrap] Build Settings updated.");
        }

        public static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder(SceneDir))
                AssetDatabase.CreateFolder("Assets/Scenes", "Geidai");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabDir))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Geidai");
        }

        // ---------- shared UI helpers ----------

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

            var responsive = canvasGo.GetComponent<ResponsiveCanvasConfigurator>();
            responsive.Configure();

            var safeGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeGo.transform.SetParent(canvasGo.transform, false);
            var safeRt = safeGo.GetComponent<RectTransform>();
            StretchFull(safeRt);
            var fitter = safeGo.GetComponent<SafeAreaFitter>();

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
            cam.backgroundColor = new Color(0.86f, 0.90f, 0.88f);
            cam.orthographic = true;

            return (canvas, safeRt, responsive, fitter, app);
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>SafeArea 内の上部に固定（向き・アスペクトで画面外に出ない）。</summary>
        private static void AnchorTopBand(RectTransform rt, float height, float topPadding = 24f)
        {
            rt.anchorMin = new Vector2(0.05f, 1f);
            rt.anchorMax = new Vector2(0.95f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, -topPadding);
        }

        /// <summary>SafeArea 中央の帯（メニュー等）。</summary>
        private static void AnchorCenterBand(RectTransform rt, float top = 0.72f, float bottom = 0.12f)
        {
            rt.anchorMin = new Vector2(0.08f, bottom);
            rt.anchorMax = new Vector2(0.92f, top);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>SafeArea 下部のボタン列。</summary>
        private static void AnchorBottom(RectTransform rt, float height, float bottomPadding = 40f)
        {
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x > 0 ? rt.sizeDelta.x : 280f, height);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, bottomPadding);
        }

        private static Font ResolveUiFont(int fontSize)
        {
            // LegacyRuntime / Arial は日本語グリフが無く「真っ白」に見える。OS フォントを優先する。
            var os = Font.CreateDynamicFontFromOSFont(new[]
            {
                "Hiragino Sans",
                "Hiragino Kaku Gothic ProN",
                "Yu Gothic UI",
                "Yu Gothic",
                "Meiryo",
                "Noto Sans CJK JP",
                "Apple SD Gothic Neo",
                "Arial Unicode MS",
                "Arial"
            }, fontSize);
            if (os != null) return os;
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800, 120);
            var text = go.GetComponent<Text>();
            text.text = content;
            text.font = ResolveUiFont(fontSize);
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            var normal = new Color(0.22f, 0.55f, 0.42f, 1f);
            img.color = normal;
            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.55f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.targetGraphic = img;
            var labelText = CreateText(go.transform, "Label", label, 28, TextAnchor.MiddleCenter);
            StretchFull(labelText.rectTransform);
            labelText.color = Color.white;
            labelText.raycastTarget = false;
            return btn;
        }

        /// <summary>実行時にホームへ戻るコンポーネントを付ける（Edit 時の onClick はシーンに残らないため）。</summary>
        private static void AttachBackToHome(Button back, ErrorPresenter error)
        {
            var bridge = back.gameObject.GetComponent<BackToHomeButton>();
            if (bridge == null) bridge = back.gameObject.AddComponent<BackToHomeButton>();
            var so = new SerializedObject(bridge);
            so.FindProperty("button").objectReferenceValue = back;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>uGUI Dropdown の必須 Template（Toggle 付き）を生成する。</summary>
        private static Dropdown CreateDropdown(Transform parent, string name, Vector2 size)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Dropdown));
            root.transform.SetParent(parent, false);
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = size;
            root.GetComponent<Image>().color = Color.white;

            var caption = CreateText(root.transform, "Label", "えらんでね", 28, TextAnchor.MiddleCenter);
            StretchFull(caption.rectTransform);
            caption.color = Color.black;

            // Template hierarchy (Unity Dropdown 必須)
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(root.transform, false);
            var templateRt = template.GetComponent<RectTransform>();
            templateRt.anchorMin = new Vector2(0f, 0f);
            templateRt.anchorMax = new Vector2(1f, 0f);
            templateRt.pivot = new Vector2(0.5f, 1f);
            templateRt.sizeDelta = new Vector2(0f, 280f);
            templateRt.anchoredPosition = Vector2.zero;
            template.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 1f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(template.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 56f);

            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle), typeof(Image));
            item.transform.SetParent(content.transform, false);
            var itemRt = item.GetComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0f, 0.5f);
            itemRt.anchorMax = new Vector2(1f, 0.5f);
            itemRt.sizeDelta = new Vector2(0f, 48f);
            item.GetComponent<Image>().color = new Color(0.9f, 0.95f, 0.9f, 1f);

            var itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
            itemBg.transform.SetParent(item.transform, false);
            StretchFull(itemBg.GetComponent<RectTransform>());
            itemBg.GetComponent<Image>().color = new Color(0.85f, 0.9f, 0.85f, 1f);

            var check = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(item.transform, false);
            var checkRt = check.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0f, 0.5f);
            checkRt.anchorMax = new Vector2(0f, 0.5f);
            checkRt.sizeDelta = new Vector2(28f, 28f);
            checkRt.anchoredPosition = new Vector2(24f, 0f);
            check.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.3f, 1f);

            var itemLabel = CreateText(item.transform, "Item Label", "Option", 26, TextAnchor.MiddleLeft);
            var itemLabelRt = itemLabel.rectTransform;
            itemLabelRt.anchorMin = Vector2.zero;
            itemLabelRt.anchorMax = Vector2.one;
            itemLabelRt.offsetMin = new Vector2(48f, 2f);
            itemLabelRt.offsetMax = new Vector2(-8f, -2f);
            itemLabel.color = Color.black;

            var toggle = item.GetComponent<Toggle>();
            toggle.targetGraphic = itemBg.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.isOn = true;

            var scroll = template.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var dropdown = root.GetComponent<Dropdown>();
            dropdown.targetGraphic = root.GetComponent<Image>();
            dropdown.captionText = caption;
            dropdown.itemText = itemLabel;
            dropdown.template = templateRt;
            template.SetActive(false);

            return dropdown;
        }

        private static InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = size;
            go.GetComponent<Image>().color = Color.white;

            var text = CreateText(go.transform, "Text", string.Empty, 24, TextAnchor.MiddleLeft);
            StretchFull(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(16f, 4f);
            text.rectTransform.offsetMax = new Vector2(-16f, -4f);
            text.color = Color.black;

            var hint = CreateText(go.transform, "Placeholder", placeholder, 24, TextAnchor.MiddleLeft);
            StretchFull(hint.rectTransform);
            hint.rectTransform.offsetMin = new Vector2(16f, 4f);
            hint.rectTransform.offsetMax = new Vector2(-16f, -4f);
            hint.color = new Color(0.45f, 0.45f, 0.45f, 0.8f);

            var input = go.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder = hint;
            return input;
        }

        private static Slider CreateSlider(
            Transform parent,
            string name,
            float min,
            float max,
            float value,
            bool wholeNumbers,
            Vector2 size)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            root.GetComponent<RectTransform>().sizeDelta = size;

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(root.transform, false);
            var bgRt = background.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.35f);
            bgRt.anchorMax = new Vector2(1f, 0.65f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.75f, 0.78f, 0.75f, 1f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            StretchFull(fillArea.GetComponent<RectTransform>());
            fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 0f);
            fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, 0f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            StretchFull(fill.GetComponent<RectTransform>());
            fill.GetComponent<Image>().color = new Color(0.22f, 0.55f, 0.42f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            StretchFull(handleArea.GetComponent<RectTransform>());
            handleArea.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 0f);
            handleArea.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(36f, 52f);
            handle.GetComponent<Image>().color = Color.white;

            var slider = root.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.value = value;
            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, bool isOn)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            root.transform.SetParent(parent, false);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 54f);

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(root.transform, false);
            var bgRt = background.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(0f, 0.5f);
            bgRt.sizeDelta = new Vector2(44f, 44f);
            bgRt.anchoredPosition = new Vector2(22f, 0f);
            background.GetComponent<Image>().color = Color.white;

            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(background.transform, false);
            StretchFull(check.GetComponent<RectTransform>());
            check.GetComponent<RectTransform>().offsetMin = new Vector2(7f, 7f);
            check.GetComponent<RectTransform>().offsetMax = new Vector2(-7f, -7f);
            check.GetComponent<Image>().color = new Color(0.22f, 0.55f, 0.42f, 1f);

            var text = CreateText(root.transform, "Label", label, 24, TextAnchor.MiddleLeft);
            StretchFull(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(60f, 0f);

            var toggle = root.GetComponent<Toggle>();
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.isOn = isOn;
            return toggle;
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
            var boxRt = box.GetComponent<RectTransform>();
            boxRt.sizeDelta = new Vector2(700, 360);
            box.GetComponent<Image>().color = Color.white;

            var title = CreateText(box.transform, "Title", "", 32, TextAnchor.MiddleCenter);
            title.rectTransform.anchoredPosition = new Vector2(0, 100);
            var message = CreateText(box.transform, "Message", "", 26, TextAnchor.MiddleCenter);
            message.rectTransform.anchoredPosition = new Vector2(0, 20);
            var yes = CreateButton(box.transform, "Yes", "はい", new Vector2(200, 70));
            yes.GetComponent<RectTransform>().anchoredPosition = new Vector2(-140, -100);
            var no = CreateButton(box.transform, "No", "いいえ", new Vector2(200, 70));
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

        private static void WireScreenRoot(ScreenRootBase screen, ResponsiveCanvasConfigurator responsive, SafeAreaFitter fitter)
        {
            var so = new SerializedObject(screen);
            so.FindProperty("responsiveConfigurator").objectReferenceValue = responsive;
            so.FindProperty("safeAreaFitter").objectReferenceValue = fitter;
            so.FindProperty("showOnStart").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SaveScene(string sceneName)
        {
            string path = $"{SceneDir}/{sceneName}.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
            Debug.Log($"[GeidaiSceneBootstrap] Saved {path}");
        }

        // ---------- scenes ----------

        public static void BuildHome()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiHomeRoot");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "ホーム", 48, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            var menuContainerGo = new GameObject("MenuContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            menuContainerGo.transform.SetParent(content, false);
            menuContainerGo.SetActive(true);
            var menuRt = menuContainerGo.GetComponent<RectTransform>();
            AnchorCenterBand(menuRt, 0.78f, 0.14f);
            var vlg = menuContainerGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 24;
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 非アクティブなプレハブから複製するとインスタンスも非アクティブになるため、
            // HomeScreenController.BuildMenu 側で SetActive(true) する（ここではプレハブのみ非表示）。
            var prefabBtn = CreateButton(content, "MenuButtonPrefab", "メニュー", new Vector2(640, 90));
            prefabBtn.gameObject.SetActive(false);
            EnsurePrefab(prefabBtn.gameObject, $"{PrefabDir}/HomeMenuButton.prefab");

            var error = CreateErrorPresenter(content);
            var confirm = CreateConfirmDialog(content);

            var screenGo = new GameObject("HomeScreen", typeof(HomeScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var home = screenGo.GetComponent<HomeScreenController>();
            WireScreenRoot(home, shell.responsive, shell.fitter);

            var menuConfig = AssetDatabase.LoadAssetAtPath<HomeMenuConfig>(HomeMenuConfigPath);
            var so = new SerializedObject(home);
            so.FindProperty("menuConfig").objectReferenceValue = menuConfig;
            so.FindProperty("menuContainer").objectReferenceValue = menuContainerGo.transform;
            so.FindProperty("menuButtonPrefab").objectReferenceValue = prefabBtn;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.FindProperty("confirmDialog").objectReferenceValue = confirm;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiHome");
        }

        public static void BuildRegister()
        {
            HomeUiSceneBuilder.BuildRegisterScene();
        }

        public static void BuildRec()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiRecRoot");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "ろくおん", 48, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            var status = CreateText(content, "Status", "「ろくおん」を おしてね（3びょう）", 28, TextAnchor.MiddleCenter);
            status.rectTransform.anchorMin = new Vector2(0.05f, 0.72f);
            status.rectTransform.anchorMax = new Vector2(0.95f, 0.82f);
            status.rectTransform.offsetMin = Vector2.zero;
            status.rectTransform.offsetMax = Vector2.zero;

            var recordBtn = CreateButton(content, "Record", "ろくおん", new Vector2(300, 100));
            recordBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.55f);
            recordBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.55f);
            recordBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-170f, 0f);
            var playBtn = CreateButton(content, "Play", "さいせい", new Vector2(300, 100));
            playBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.55f);
            playBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.55f);
            playBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(170f, 0f);
            var saveBtn = CreateButton(content, "Save", "ほぞん", new Vector2(300, 100));
            saveBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.42f);
            saveBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.42f);
            saveBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var backBtn = CreateButton(content, "Back", "もどる", new Vector2(280, 90));
            AnchorBottom(backBtn.GetComponent<RectTransform>(), 90f, 48f);

            var recording = new GameObject("RecordingControllerHost");
            recording.transform.SetParent(content, false);
            var recordingCtrl = recording.AddComponent<RecordingController>();

            var effectHost = new GameObject("EffectPanel", typeof(EffectPanelController));
            effectHost.transform.SetParent(content, false);
            var effect = effectHost.GetComponent<EffectPanelController>();

            var saveHost = new GameObject("SavePrompt", typeof(SavePromptController));
            saveHost.transform.SetParent(content, false);
            var savePrompt = saveHost.GetComponent<SavePromptController>();

            var error = CreateErrorPresenter(content);
            var confirm = CreateConfirmDialog(content);
            // もどるは RecScreenController（未保存確認付き）が結線する。BackToHome は二重遷移になるので付けない。

            var screenGo = new GameObject("RecScreen", typeof(RecScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var rec = screenGo.GetComponent<RecScreenController>();
            WireScreenRoot(rec, shell.responsive, shell.fitter);
            var so = new SerializedObject(rec);
            so.FindProperty("recordingController").objectReferenceValue = recordingCtrl;
            so.FindProperty("effectPanel").objectReferenceValue = effect;
            so.FindProperty("savePrompt").objectReferenceValue = savePrompt;
            so.FindProperty("recordButton").objectReferenceValue = recordBtn;
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("saveButton").objectReferenceValue = saveBtn;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.FindProperty("confirmDialog").objectReferenceValue = confirm;
            var statusProp = so.FindProperty("statusText");
            if (statusProp != null) statusProp.objectReferenceValue = status;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiRec");
        }

        public static void BuildCollection()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiCollectionRoot");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "コレクション", 48, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            // ScrollRect + Content + ItemPrefab（未配線だと保存音があっても一覧が空のまま）
            var listHost = new GameObject("SoundListView", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(SoundListView));
            listHost.transform.SetParent(content, false);
            AnchorCenterBand(listHost.GetComponent<RectTransform>(), 0.78f, 0.22f);
            listHost.GetComponent<Image>().color = new Color(0.94f, 0.96f, 0.94f, 1f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(listHost.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var contentRootGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentRootGo.transform.SetParent(viewport.transform, false);
            var contentRt = contentRootGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 0f);
            var vlg = contentRootGo.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 12f;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            var fitter = contentRootGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = listHost.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var itemPrefab = CreateSoundListItemPrefab(listHost.transform);
            var empty = CreateText(content, "EmptyHint", "まだ おとが ないよ。さきに ろくおんしてね", 26, TextAnchor.MiddleCenter);
            empty.rectTransform.anchorMin = new Vector2(0.1f, 0.4f);
            empty.rectTransform.anchorMax = new Vector2(0.9f, 0.55f);
            empty.rectTransform.offsetMin = Vector2.zero;
            empty.rectTransform.offsetMax = Vector2.zero;

            var listView = listHost.GetComponent<SoundListView>();
            var soList = new SerializedObject(listView);
            soList.FindProperty("contentRoot").objectReferenceValue = contentRt;
            soList.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
            soList.FindProperty("emptyState").objectReferenceValue = empty.gameObject;
            soList.ApplyModifiedPropertiesWithoutUndo();

            var filterHost = new GameObject("FilterSearch", typeof(RectTransform), typeof(FilterSearchController));
            filterHost.transform.SetParent(content, false);

            var detailHost = new GameObject("SoundDetail", typeof(RectTransform), typeof(SoundDetailController));
            detailHost.transform.SetParent(content, false);
            detailHost.SetActive(false);

            var back = CreateButton(content, "Back", "もどる", new Vector2(280, 90));
            AnchorBottom(back.GetComponent<RectTransform>(), 90f, 40f);
            var error = CreateErrorPresenter(content);

            var screenGo = new GameObject("CollectionScreen", typeof(CollectionScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var col = screenGo.GetComponent<CollectionScreenController>();
            WireScreenRoot(col, shell.responsive, shell.fitter);
            var so = new SerializedObject(col);
            so.FindProperty("listView").objectReferenceValue = listView;
            so.FindProperty("filterSearch").objectReferenceValue = filterHost.GetComponent<FilterSearchController>();
            so.FindProperty("detail").objectReferenceValue = detailHost.GetComponent<SoundDetailController>();
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiCollection");
        }

        /// <summary>コレクション一覧の1行 Prefab（非アクティブ雛形）。</summary>
        private static SoundListItemView CreateSoundListItemPrefab(Transform parent)
        {
            var go = new GameObject("SoundListItemPrefab", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(SoundListItemView));
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 110f);
            go.GetComponent<Image>().color = new Color(0.22f, 0.55f, 0.42f, 1f);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = 110f;
            le.preferredHeight = 110f;

            var title = CreateText(go.transform, "Title", "おと", 28, TextAnchor.MiddleLeft);
            title.rectTransform.anchorMin = new Vector2(0.05f, 0.45f);
            title.rectTransform.anchorMax = new Vector2(0.55f, 0.95f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.color = Color.white;

            var date = CreateText(go.transform, "Date", "", 20, TextAnchor.MiddleLeft);
            date.rectTransform.anchorMin = new Vector2(0.05f, 0.05f);
            date.rectTransform.anchorMax = new Vector2(0.55f, 0.45f);
            date.rectTransform.offsetMin = Vector2.zero;
            date.rectTransform.offsetMax = Vector2.zero;
            date.color = new Color(0.9f, 0.95f, 0.9f, 1f);

            var play = CreateButton(go.transform, "Play", "きく", new Vector2(120, 70));
            play.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
            play.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
            play.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
            play.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 0f);

            // 行全体タップでも詳細へ（openButton）
            var open = go.GetComponent<Button>();
            if (open == null) open = go.AddComponent<Button>();
            open.targetGraphic = go.GetComponent<Image>();
            open.transition = Selectable.Transition.ColorTint;

            var item = go.GetComponent<SoundListItemView>();
            var so = new SerializedObject(item);
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("dateText").objectReferenceValue = date;
            so.FindProperty("openButton").objectReferenceValue = open;
            so.FindProperty("playButton").objectReferenceValue = play;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        public static void BuildTheme()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiThemeRoot");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "こんしゅうの おだい", 40, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            var themeText = CreateText(content, "ThemeText", "おだい", 56, TextAnchor.MiddleCenter);
            themeText.rectTransform.anchorMin = new Vector2(0.05f, 0.55f);
            themeText.rectTransform.anchorMax = new Vector2(0.95f, 0.7f);
            themeText.rectTransform.offsetMin = Vector2.zero;
            themeText.rectTransform.offsetMax = Vector2.zero;
            var reading = CreateText(content, "ReadingText", "", 32, TextAnchor.MiddleCenter);
            reading.rectTransform.anchorMin = new Vector2(0.05f, 0.48f);
            reading.rectTransform.anchorMax = new Vector2(0.95f, 0.55f);
            reading.rectTransform.offsetMin = Vector2.zero;
            reading.rectTransform.offsetMax = Vector2.zero;
            var hint = CreateText(content, "HintText", "", 28, TextAnchor.MiddleCenter);
            hint.rectTransform.anchorMin = new Vector2(0.05f, 0.4f);
            hint.rectTransform.anchorMax = new Vector2(0.95f, 0.48f);
            hint.rectTransform.offsetMin = Vector2.zero;
            hint.rectTransform.offsetMax = Vector2.zero;

            var empty = new GameObject("EmptyState", typeof(RectTransform), typeof(Text));
            empty.transform.SetParent(content, false);
            empty.GetComponent<Text>().text = "おだいが まだ ないよ";
            empty.GetComponent<Text>().font = themeText.font;
            empty.GetComponent<Text>().fontSize = 32;
            empty.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            empty.GetComponent<Text>().color = Color.gray;
            empty.SetActive(false);

            var record = CreateButton(content, "Record", "ろくおんする", new Vector2(360, 100));
            record.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.28f);
            record.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.28f);
            record.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var error = CreateErrorPresenter(content);

            var themeCtrlGo = new GameObject("WeeklyTheme", typeof(WeeklyThemeController));
            themeCtrlGo.transform.SetParent(content, false);
            var themeCtrl = themeCtrlGo.GetComponent<WeeklyThemeController>();
            var catalog = AssetDatabase.LoadAssetAtPath<ThemeCatalog>(ThemeCatalogPath);
            var soTheme = new SerializedObject(themeCtrl);
            soTheme.FindProperty("themeText").objectReferenceValue = themeText;
            soTheme.FindProperty("readingText").objectReferenceValue = reading;
            soTheme.FindProperty("hintText").objectReferenceValue = hint;
            soTheme.FindProperty("recordButton").objectReferenceValue = record;
            soTheme.FindProperty("emptyState").objectReferenceValue = empty;
            soTheme.FindProperty("errorPresenter").objectReferenceValue = error;
            soTheme.FindProperty("catalog").objectReferenceValue = catalog;
            soTheme.ApplyModifiedPropertiesWithoutUndo();

            var screenGo = new GameObject("ThemeScreen", typeof(WeeklyThemeScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var screen = screenGo.GetComponent<WeeklyThemeScreenController>();
            WireScreenRoot(screen, shell.responsive, shell.fitter);
            var so = new SerializedObject(screen);
            so.FindProperty("themeController").objectReferenceValue = themeCtrl;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.ApplyModifiedPropertiesWithoutUndo();

            var back = CreateButton(content, "Back", "もどる", new Vector2(280, 90));
            AnchorBottom(back.GetComponent<RectTransform>(), 90f, 40f);
            AttachBackToHome(back, error);

            SaveScene("GeidaiTheme");
        }

        public static void BuildGame1()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiGame1Root");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "① 音あわせ", 44, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            var frogGo = new GameObject("Frog", typeof(RectTransform), typeof(Image), typeof(FrogTargetView));
            frogGo.transform.SetParent(content, false);
            var frogRt = frogGo.GetComponent<RectTransform>();
            frogRt.anchorMin = new Vector2(0.5f, 0.58f);
            frogRt.anchorMax = new Vector2(0.5f, 0.58f);
            frogRt.sizeDelta = new Vector2(260, 260);
            frogRt.anchoredPosition = Vector2.zero;
            frogGo.GetComponent<Image>().color = new Color(0.4f, 0.7f, 0.4f);

            var frogPreview = CreateButton(frogGo.transform, "Preview", "きく", new Vector2(120, 50));
            frogPreview.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);
            var frog = frogGo.GetComponent<FrogTargetView>();
            var soFrog = new SerializedObject(frog);
            soFrog.FindProperty("previewButton").objectReferenceValue = frogPreview;
            soFrog.FindProperty("dropArea").objectReferenceValue = frogRt;
            soFrog.ApplyModifiedPropertiesWithoutUndo();

            var hint = CreateText(content, "Hint", "おたまを タップで きいて、カエルへ ドラッグしてね", 24, TextAnchor.MiddleCenter);
            hint.rectTransform.anchorMin = new Vector2(0.05f, 0.48f);
            hint.rectTransform.anchorMax = new Vector2(0.95f, 0.55f);
            hint.rectTransform.offsetMin = Vector2.zero;
            hint.rectTransform.offsetMax = Vector2.zero;

            var choices = new List<ChoiceItemView>();
            for (int i = 0; i < 3; i++)
            {
                var choiceGo = new GameObject($"Choice_{i}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(ChoiceItemView));
                choiceGo.transform.SetParent(content, false);
                var crt = choiceGo.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 0.28f);
                crt.anchorMax = new Vector2(0.5f, 0.28f);
                crt.sizeDelta = new Vector2(170, 170);
                crt.anchoredPosition = new Vector2(-200 + i * 200, 0f);
                choiceGo.GetComponent<Image>().color = new Color(0.55f, 0.75f, 0.9f);
                var preview = CreateButton(choiceGo.transform, "Preview", "きく", new Vector2(100, 40));
                preview.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
                var choice = choiceGo.GetComponent<ChoiceItemView>();
                var soC = new SerializedObject(choice);
                soC.FindProperty("previewButton").objectReferenceValue = preview;
                soC.FindProperty("canvasGroup").objectReferenceValue = choiceGo.GetComponent<CanvasGroup>();
                soC.ApplyModifiedPropertiesWithoutUndo();
                choices.Add(choice);
            }

            var effectGo = new GameObject("ResultEffect", typeof(ResultEffectController));
            effectGo.transform.SetParent(content, false);
            var resultText = CreateText(content, "ResultText", "", 32, TextAnchor.MiddleCenter);
            resultText.rectTransform.anchorMin = new Vector2(0.05f, 0.18f);
            resultText.rectTransform.anchorMax = new Vector2(0.95f, 0.26f);
            resultText.rectTransform.offsetMin = Vector2.zero;
            resultText.rectTransform.offsetMax = Vector2.zero;
            var resultPanel = resultText.gameObject;
            resultPanel.SetActive(false);
            var soFx = new SerializedObject(effectGo.GetComponent<ResultEffectController>());
            soFx.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            soFx.FindProperty("resultText").objectReferenceValue = resultText;
            soFx.ApplyModifiedPropertiesWithoutUndo();

            var empty = new GameObject("EmptyState", typeof(RectTransform), typeof(Text));
            empty.transform.SetParent(content, false);
            empty.GetComponent<Text>().text = "ろくおんした おとが ないよ";
            empty.GetComponent<Text>().font = resultText.font;
            empty.GetComponent<Text>().fontSize = 28;
            empty.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            empty.SetActive(false);

            var error = CreateErrorPresenter(content);
            var back = CreateButton(content, "Back", "もどる", new Vector2(280, 90));
            AnchorBottom(back.GetComponent<RectTransform>(), 90f, 36f);
            AttachBackToHome(back, error);
            var config = AssetDatabase.LoadAssetAtPath<SoundMatchConfig>(SoundMatchConfigPath);

            var screenGo = new GameObject("SoundMatchScreen", typeof(SoundMatchGameController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var game = screenGo.GetComponent<SoundMatchGameController>();
            WireScreenRoot(game, shell.responsive, shell.fitter);
            var so = new SerializedObject(game);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("frog").objectReferenceValue = frog;
            so.FindProperty("resultEffect").objectReferenceValue = effectGo.GetComponent<ResultEffectController>();
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.FindProperty("emptyState").objectReferenceValue = empty;
            var choicesProp = so.FindProperty("choiceViews");
            choicesProp.arraySize = choices.Count;
            for (int i = 0; i < choices.Count; i++)
                choicesProp.GetArrayElementAtIndex(i).objectReferenceValue = choices[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiGame1");
        }

        public static void BuildLibrary()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiLibraryRoot");
            var content = shell.safeArea;

            Image bg = null;
            var canvasImages = shell.canvas.GetComponentsInChildren<Image>(true);
            if (canvasImages != null && canvasImages.Length > 0) bg = canvasImages[0];
            if (bg != null) HomeUiImageUtil.ApplySolidFill(bg, HomeUiTheme.Background);

            var title = CreateText(content, "Title", "おとずかん", 46, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 90f, 24f);
            title.color = HomeUiTheme.TitleOnBackground;
            UiFontResolver.ApplyTo(title, HomeUiTheme.ScreenTitle);

            var categoryDropdown = CreateDropdown(content, "CategoryFilter", new Vector2(300f, 56f));
            var catRt = categoryDropdown.GetComponent<RectTransform>();
            catRt.anchorMin = new Vector2(0.08f, 1f);
            catRt.anchorMax = new Vector2(0.08f, 1f);
            catRt.pivot = new Vector2(0f, 1f);
            catRt.anchoredPosition = new Vector2(0f, -100f);

            var timbreDropdown = CreateDropdown(content, "TimbreFilter", new Vector2(300f, 56f));
            var timRt = timbreDropdown.GetComponent<RectTransform>();
            timRt.anchorMin = new Vector2(0.92f, 1f);
            timRt.anchorMax = new Vector2(0.92f, 1f);
            timRt.pivot = new Vector2(1f, 1f);
            timRt.anchoredPosition = new Vector2(0f, -100f);

            var listHost = new GameObject(
                "CuratedSoundList",
                typeof(RectTransform),
                typeof(Image),
                typeof(ScrollRect),
                typeof(CuratedSoundListView));
            listHost.transform.SetParent(content, false);
            AnchorCenterBand(listHost.GetComponent<RectTransform>(), 0.78f, 0.32f);
            listHost.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(listHost.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var contentRootGo = new GameObject(
                "Content",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            contentRootGo.transform.SetParent(viewport.transform, false);
            var contentRt = contentRootGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;
            var layout = contentRootGo.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 12f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            contentRootGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = listHost.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var itemPrefab = CreateCuratedSoundItemPrefab(listHost.transform);
            var empty = CreateText(content, "EmptyState", "おとが まだ ないよ", 28, TextAnchor.MiddleCenter);
            empty.rectTransform.anchorMin = new Vector2(0.1f, 0.42f);
            empty.rectTransform.anchorMax = new Vector2(0.9f, 0.56f);
            empty.rectTransform.offsetMin = Vector2.zero;
            empty.rectTransform.offsetMax = Vector2.zero;

            var placeholder = AssetDatabase.LoadAssetAtPath<Sprite>(LibraryPlaceholderPath);

            var listView = listHost.GetComponent<CuratedSoundListView>();
            var soList = new SerializedObject(listView);
            soList.FindProperty("contentRoot").objectReferenceValue = contentRt;
            soList.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
            soList.FindProperty("emptyState").objectReferenceValue = empty.gameObject;
            soList.FindProperty("placeholderSprite").objectReferenceValue = placeholder;
            soList.ApplyModifiedPropertiesWithoutUndo();

            var detailGo = new GameObject(
                "DetailPanel",
                typeof(RectTransform),
                typeof(Image),
                typeof(LibraryDetailPanel));
            detailGo.transform.SetParent(content, false);
            var detailRt = detailGo.GetComponent<RectTransform>();
            detailRt.anchorMin = new Vector2(0.08f, 0.12f);
            detailRt.anchorMax = new Vector2(0.92f, 0.30f);
            detailRt.offsetMin = Vector2.zero;
            detailRt.offsetMax = Vector2.zero;
            detailGo.GetComponent<Image>().color = HomeUiTheme.PanelFill;
            var detailTitle = CreateText(detailGo.transform, "DetailTitle", "", 28, TextAnchor.UpperLeft);
            detailTitle.rectTransform.anchorMin = new Vector2(0.04f, 0.55f);
            detailTitle.rectTransform.anchorMax = new Vector2(0.96f, 0.95f);
            detailTitle.rectTransform.offsetMin = Vector2.zero;
            detailTitle.rectTransform.offsetMax = Vector2.zero;
            detailTitle.color = HomeUiTheme.MenuText;
            var detailDesc = CreateText(detailGo.transform, "DetailDescription", "", 22, TextAnchor.UpperLeft);
            detailDesc.rectTransform.anchorMin = new Vector2(0.04f, 0.08f);
            detailDesc.rectTransform.anchorMax = new Vector2(0.96f, 0.55f);
            detailDesc.rectTransform.offsetMin = Vector2.zero;
            detailDesc.rectTransform.offsetMax = Vector2.zero;
            detailDesc.color = HomeUiTheme.MenuText;
            var detailMeta = CreateText(detailGo.transform, "DetailMeta", "", 20, TextAnchor.LowerLeft);
            detailMeta.rectTransform.anchorMin = new Vector2(0.04f, 0f);
            detailMeta.rectTransform.anchorMax = new Vector2(0.96f, 0.22f);
            detailMeta.rectTransform.offsetMin = Vector2.zero;
            detailMeta.rectTransform.offsetMax = Vector2.zero;
            detailMeta.color = HomeUiTheme.MenuText;
            var detailHint = CreateText(detailGo.transform, "DetailHint", "おとを えらんでね", 24, TextAnchor.MiddleCenter);
            detailHint.rectTransform.anchorMin = Vector2.zero;
            detailHint.rectTransform.anchorMax = Vector2.one;
            detailHint.rectTransform.offsetMin = Vector2.zero;
            detailHint.rectTransform.offsetMax = Vector2.zero;
            detailHint.color = HomeUiTheme.PlaceholderText;
            var detail = detailGo.GetComponent<LibraryDetailPanel>();
            var soDetail = new SerializedObject(detail);
            soDetail.FindProperty("titleLabel").objectReferenceValue = detailTitle;
            soDetail.FindProperty("descriptionLabel").objectReferenceValue = detailDesc;
            soDetail.FindProperty("metaLabel").objectReferenceValue = detailMeta;
            soDetail.FindProperty("emptyHint").objectReferenceValue = detailHint.gameObject;
            soDetail.ApplyModifiedPropertiesWithoutUndo();

            var back = CreateButton(content, "Back", "もどる", new Vector2(240f, 84f));
            var backRt = back.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.28f, 0f);
            backRt.anchorMax = new Vector2(0.28f, 0f);
            backRt.pivot = new Vector2(0.5f, 0f);
            backRt.anchoredPosition = new Vector2(0f, 32f);

            var stop = CreateButton(content, "Stop", "とめる", new Vector2(240f, 84f));
            var stopRt = stop.GetComponent<RectTransform>();
            stopRt.anchorMin = new Vector2(0.72f, 0f);
            stopRt.anchorMax = new Vector2(0.72f, 0f);
            stopRt.pivot = new Vector2(0.5f, 0f);
            stopRt.anchoredPosition = new Vector2(0f, 32f);

            var error = CreateErrorPresenter(content);
            var catalog = AssetDatabase.LoadAssetAtPath<CuratedSoundCatalog>(CuratedSoundCatalogPath);
            var rules = AssetDatabase.LoadAssetAtPath<UnlockRulesCatalog>(UnlockRulesCatalogPath);
            var timbres = AssetDatabase.LoadAssetAtPath<TimbreTagCatalog>(TimbreTagCatalogPath);

            var screenGo = new GameObject("LibraryScreen", typeof(LibraryScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var screen = screenGo.GetComponent<LibraryScreenController>();
            WireScreenRoot(screen, shell.responsive, shell.fitter);
            var so = new SerializedObject(screen);
            so.FindProperty("curatedCatalog").objectReferenceValue = catalog;
            so.FindProperty("unlockRules").objectReferenceValue = rules;
            so.FindProperty("timbreTagCatalog").objectReferenceValue = timbres;
            so.FindProperty("listView").objectReferenceValue = listView;
            so.FindProperty("detailPanel").objectReferenceValue = detail;
            so.FindProperty("categoryDropdown").objectReferenceValue = categoryDropdown;
            so.FindProperty("timbreDropdown").objectReferenceValue = timbreDropdown;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("stopButton").objectReferenceValue = stop;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.FindProperty("backgroundImage").objectReferenceValue = bg;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiLibrary");
        }

        private static CuratedSoundItemView CreateCuratedSoundItemPrefab(Transform parent)
        {
            var go = new GameObject(
                "CuratedSoundItemPrefab",
                typeof(RectTransform),
                typeof(Image),
                typeof(LayoutElement),
                typeof(CuratedSoundItemView));
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 112f);
            go.GetComponent<Image>().color = new Color(0.92f, 0.95f, 0.98f, 1f);
            var element = go.GetComponent<LayoutElement>();
            element.minHeight = 112f;
            element.preferredHeight = 112f;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.02f, 0.15f);
            iconRt.anchorMax = new Vector2(0.02f, 0.15f);
            iconRt.pivot = new Vector2(0f, 0f);
            iconRt.sizeDelta = new Vector2(72f, 72f);
            iconGo.GetComponent<Image>().color = Color.white;

            var number = CreateText(go.transform, "Number", "#1", 22, TextAnchor.MiddleLeft);
            number.rectTransform.anchorMin = new Vector2(0.16f, 0.55f);
            number.rectTransform.anchorMax = new Vector2(0.30f, 0.95f);
            number.rectTransform.offsetMin = Vector2.zero;
            number.rectTransform.offsetMax = Vector2.zero;
            number.color = HomeUiTheme.MenuText;

            var name = CreateText(go.transform, "Name", "おと", 28, TextAnchor.MiddleLeft);
            name.rectTransform.anchorMin = new Vector2(0.30f, 0.42f);
            name.rectTransform.anchorMax = new Vector2(0.62f, 0.95f);
            name.rectTransform.offsetMin = Vector2.zero;
            name.rectTransform.offsetMax = Vector2.zero;
            name.color = HomeUiTheme.MenuText;

            var category = CreateText(go.transform, "Category", "", 20, TextAnchor.MiddleLeft);
            category.rectTransform.anchorMin = new Vector2(0.16f, 0.05f);
            category.rectTransform.anchorMax = new Vector2(0.62f, 0.42f);
            category.rectTransform.offsetMin = Vector2.zero;
            category.rectTransform.offsetMax = Vector2.zero;
            category.color = HomeUiTheme.MenuText;

            var lockIconGo = new GameObject("LockIcon", typeof(RectTransform), typeof(Image));
            lockIconGo.transform.SetParent(go.transform, false);
            var lockRt = lockIconGo.GetComponent<RectTransform>();
            lockRt.anchorMin = new Vector2(0.66f, 0.5f);
            lockRt.anchorMax = new Vector2(0.66f, 0.5f);
            lockRt.sizeDelta = new Vector2(40f, 40f);
            lockIconGo.GetComponent<Image>().color = new Color(0.95f, 0.75f, 0.2f, 1f);

            var lockLabel = CreateText(go.transform, "LockLabel", "ロック", 20, TextAnchor.MiddleCenter);
            lockLabel.rectTransform.anchorMin = new Vector2(0.70f, 0.2f);
            lockLabel.rectTransform.anchorMax = new Vector2(0.82f, 0.8f);
            lockLabel.rectTransform.offsetMin = Vector2.zero;
            lockLabel.rectTransform.offsetMax = Vector2.zero;
            lockLabel.color = HomeUiTheme.MenuText;

            var play = CreateButton(go.transform, "Play", "きく", new Vector2(130f, 70f));
            var playRt = play.GetComponent<RectTransform>();
            playRt.anchorMin = new Vector2(1f, 0.5f);
            playRt.anchorMax = new Vector2(1f, 0.5f);
            playRt.pivot = new Vector2(1f, 0.5f);
            playRt.anchoredPosition = new Vector2(-20f, 0f);

            var placeholder = AssetDatabase.LoadAssetAtPath<Sprite>(LibraryPlaceholderPath);
            var item = go.GetComponent<CuratedSoundItemView>();
            var so = new SerializedObject(item);
            so.FindProperty("numberLabel").objectReferenceValue = number;
            so.FindProperty("nameLabel").objectReferenceValue = name;
            so.FindProperty("categoryLabel").objectReferenceValue = category;
            so.FindProperty("lockLabel").objectReferenceValue = lockLabel;
            so.FindProperty("playButton").objectReferenceValue = play;
            so.FindProperty("lockIcon").objectReferenceValue = lockIconGo.GetComponent<Image>();
            so.FindProperty("iconImage").objectReferenceValue = iconGo.GetComponent<Image>();
            so.FindProperty("placeholderSprite").objectReferenceValue = placeholder;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        public static void BuildCreate()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiCreateRoot");
            var safe = shell.safeArea;

            var title = CreateText(safe, "Title", "おとづくり", 44, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 84f, 20f);

            var scrollHost = new GameObject("CreateScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollHost.transform.SetParent(safe, false);
            var scrollRt = scrollHost.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.05f, 0f);
            scrollRt.anchorMax = new Vector2(0.95f, 0.9f);
            // 下端は「もどる」とボタン行のぶんだけピクセルで空ける（端末比率に依存させない）。
            scrollRt.offsetMin = new Vector2(0f, 208f);
            scrollRt.offsetMax = Vector2.zero;
            scrollHost.GetComponent<Image>().color = new Color(0.94f, 0.97f, 0.94f, 1f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollHost.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var formGo = new GameObject("Form", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            formGo.transform.SetParent(viewport.transform, false);
            var formRt = formGo.GetComponent<RectTransform>();
            formRt.anchorMin = new Vector2(0f, 1f);
            formRt.anchorMax = new Vector2(1f, 1f);
            formRt.pivot = new Vector2(0.5f, 1f);
            formRt.sizeDelta = Vector2.zero;
            var formLayout = formGo.GetComponent<VerticalLayoutGroup>();
            formLayout.padding = new RectOffset(24, 24, 20, 20);
            formLayout.spacing = 10f;
            formLayout.childAlignment = TextAnchor.UpperCenter;
            formLayout.childControlWidth = true;
            formLayout.childControlHeight = false;
            formLayout.childForceExpandWidth = true;
            formLayout.childForceExpandHeight = false;
            formGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollHost.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = formRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var pickLabel = CreateText(formGo.transform, "PickLabel", "つかう おと（A / B）", 28, TextAnchor.MiddleLeft);
            AddLayoutHeight(pickLabel.gameObject, 46f);
            var slotA = CreateDropdown(formGo.transform, "SlotA", new Vector2(0f, 64f));
            AddLayoutHeight(slotA.gameObject, 64f);
            var slotB = CreateDropdown(formGo.transform, "SlotB", new Vector2(0f, 64f));
            AddLayoutHeight(slotB.gameObject, 64f);

            var pickerGo = new GameObject("RecipeLayerPicker", typeof(RecipeLayerPicker));
            pickerGo.transform.SetParent(formGo.transform, false);
            AddLayoutHeight(pickerGo, 1f);
            var picker = pickerGo.GetComponent<RecipeLayerPicker>();
            var soPicker = new SerializedObject(picker);
            soPicker.FindProperty("slotA").objectReferenceValue = slotA;
            soPicker.FindProperty("slotB").objectReferenceValue = slotB;
            soPicker.ApplyModifiedPropertiesWithoutUndo();

            var effectLabel = CreateText(formGo.transform, "EffectLabel", "こうか（チェックON=A / OFF=B）", 26, TextAnchor.MiddleLeft);
            AddLayoutHeight(effectLabel.gameObject, 44f);
            var layerToggle = CreateToggle(formGo.transform, "EditLayerA", "A を ちょうせい", true);
            AddLayoutHeight(layerToggle.gameObject, 54f);

            var volumeLabel = CreateText(formGo.transform, "VolumeLabel", "おんりょう", 22, TextAnchor.MiddleLeft);
            AddLayoutHeight(volumeLabel.gameObject, 34f);
            var volume = CreateSlider(formGo.transform, "Volume", 0f, 1f, 1f, false, new Vector2(0f, 54f));
            AddLayoutHeight(volume.gameObject, 54f);

            var pitchLabel = CreateText(formGo.transform, "PitchLabel", "ピッチ（-12 〜 +12）", 22, TextAnchor.MiddleLeft);
            AddLayoutHeight(pitchLabel.gameObject, 34f);
            var pitch = CreateSlider(formGo.transform, "Pitch", -12f, 12f, 0f, true, new Vector2(0f, 54f));
            AddLayoutHeight(pitch.gameObject, 54f);
            var pitchValue = CreateText(formGo.transform, "PitchValue", "0", 22, TextAnchor.MiddleCenter);
            AddLayoutHeight(pitchValue.gameObject, 34f);

            var reverbLabel = CreateText(formGo.transform, "ReverbLabel", "リバーブ", 22, TextAnchor.MiddleLeft);
            AddLayoutHeight(reverbLabel.gameObject, 34f);
            var reverb = CreateSlider(formGo.transform, "Reverb", 0f, 1f, 0f, false, new Vector2(0f, 54f));
            AddLayoutHeight(reverb.gameObject, 54f);

            var timbreLabel = CreateText(formGo.transform, "TimbreLabel", "おんしょく", 22, TextAnchor.MiddleLeft);
            AddLayoutHeight(timbreLabel.gameObject, 34f);
            var timbre = CreateDropdown(formGo.transform, "Timbre", new Vector2(0f, 64f));
            AddLayoutHeight(timbre.gameObject, 64f);

            var effectGo = new GameObject("RecipeEffectPanel", typeof(RecipeEffectPanel));
            effectGo.transform.SetParent(formGo.transform, false);
            AddLayoutHeight(effectGo, 1f);
            var effect = effectGo.GetComponent<RecipeEffectPanel>();
            var soEffect = new SerializedObject(effect);
            soEffect.FindProperty("layerAToggle").objectReferenceValue = layerToggle;
            soEffect.FindProperty("volumeSlider").objectReferenceValue = volume;
            soEffect.FindProperty("pitchSlider").objectReferenceValue = pitch;
            soEffect.FindProperty("reverbSlider").objectReferenceValue = reverb;
            soEffect.FindProperty("timbreDropdown").objectReferenceValue = timbre;
            soEffect.FindProperty("pitchValueLabel").objectReferenceValue = pitchValue;
            soEffect.ApplyModifiedPropertiesWithoutUndo();

            var titleLabel = CreateText(formGo.transform, "RecipeTitleLabel", "レシピの なまえ", 22, TextAnchor.MiddleLeft);
            AddLayoutHeight(titleLabel.gameObject, 34f);
            var titleInput = CreateInputField(formGo.transform, "RecipeTitle", "なまえ（なくてもOK）", new Vector2(0f, 64f));
            AddLayoutHeight(titleInput.gameObject, 64f);

            // 主要操作はスクロールの外へ固定し、縦長端末でも常に表示する。
            var actionRow = CreateButtonRow(safe, "ActionRow");
            var actionRt = actionRow.GetComponent<RectTransform>();
            actionRt.anchorMin = new Vector2(0.05f, 0f);
            actionRt.anchorMax = new Vector2(0.95f, 0f);
            actionRt.pivot = new Vector2(0.5f, 0f);
            actionRt.sizeDelta = new Vector2(0f, 74f);
            actionRt.anchoredPosition = new Vector2(0f, 116f);
            var preview = CreateButton(actionRow.transform, "Preview", "きく", new Vector2(190f, 70f));
            var stop = CreateButton(actionRow.transform, "Stop", "とめる", new Vector2(190f, 70f));
            var save = CreateButton(actionRow.transform, "Save", "ほぞん", new Vector2(190f, 70f));

            var recipeLabel = CreateText(formGo.transform, "RecipeListLabel", "ほぞんした レシピ", 24, TextAnchor.MiddleLeft);
            AddLayoutHeight(recipeLabel.gameObject, 40f);
            var recipeDropdown = CreateDropdown(formGo.transform, "RecipeList", new Vector2(0f, 64f));
            AddLayoutHeight(recipeDropdown.gameObject, 64f);
            var recipeRow = CreateButtonRow(formGo.transform, "RecipeRow");
            var open = CreateButton(recipeRow.transform, "Open", "ひらく", new Vector2(190f, 70f));
            var delete = CreateButton(recipeRow.transform, "Delete", "けす", new Vector2(190f, 70f));
            var export = CreateButton(recipeRow.transform, "Export", "WAVE", new Vector2(190f, 70f));

            var listGo = new GameObject("RecipeListController", typeof(RecipeListController));
            listGo.transform.SetParent(formGo.transform, false);
            AddLayoutHeight(listGo, 1f);
            var recipeList = listGo.GetComponent<RecipeListController>();
            var soList = new SerializedObject(recipeList);
            soList.FindProperty("recipeDropdown").objectReferenceValue = recipeDropdown;
            soList.FindProperty("openButton").objectReferenceValue = open;
            soList.FindProperty("deleteButton").objectReferenceValue = delete;
            soList.ApplyModifiedPropertiesWithoutUndo();

            var confirm = CreateConfirmDialog(safe);
            var exportGo = new GameObject("RecipeExportController", typeof(RecipeExportController));
            exportGo.transform.SetParent(formGo.transform, false);
            AddLayoutHeight(exportGo, 1f);
            var exportController = exportGo.GetComponent<RecipeExportController>();
            var soExport = new SerializedObject(exportController);
            soExport.FindProperty("exportButton").objectReferenceValue = export;
            soExport.FindProperty("confirmDialog").objectReferenceValue = confirm;
            soExport.ApplyModifiedPropertiesWithoutUndo();

            var back = CreateButton(safe, "Back", "もどる", new Vector2(280f, 82f));
            AnchorBottom(back.GetComponent<RectTransform>(), 82f, 24f);
            var error = CreateErrorPresenter(safe);
            var catalog = AssetDatabase.LoadAssetAtPath<CuratedSoundCatalog>(CuratedSoundCatalogPath);
            var rules = AssetDatabase.LoadAssetAtPath<UnlockRulesCatalog>(UnlockRulesCatalogPath);

            var screenGo = new GameObject("CreateScreen", typeof(CreateScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var screen = screenGo.GetComponent<CreateScreenController>();
            WireScreenRoot(screen, shell.responsive, shell.fitter);
            var so = new SerializedObject(screen);
            so.FindProperty("curatedCatalog").objectReferenceValue = catalog;
            so.FindProperty("unlockRules").objectReferenceValue = rules;
            so.FindProperty("layerPicker").objectReferenceValue = picker;
            so.FindProperty("effectPanel").objectReferenceValue = effect;
            so.FindProperty("recipeList").objectReferenceValue = recipeList;
            so.FindProperty("exportController").objectReferenceValue = exportController;
            so.FindProperty("titleField").objectReferenceValue = titleInput;
            so.FindProperty("previewButton").objectReferenceValue = preview;
            so.FindProperty("saveButton").objectReferenceValue = save;
            so.FindProperty("stopButton").objectReferenceValue = stop;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.FindProperty("confirmDialog").objectReferenceValue = confirm;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiCreate");
        }

        private static void AddLayoutHeight(GameObject go, float height)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element == null) element = go.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            element.flexibleHeight = 0f;
        }

        private static GameObject CreateButtonRow(Transform parent, string name)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            // 高さを制御すると LayoutElement 無しのボタンが 0px になり、背景もタップ判定も消える。
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            AddLayoutHeight(row, 78f);
            return row;
        }

        private static void EnsurePrefab(GameObject source, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(source, path);
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
                    Debug.LogWarning($"[GeidaiSceneBootstrap] missing scene: {path}");
            }

            // Keep old scenes in list but disabled for rollback.
            foreach (var path in keepDisabled)
            {
                if (File.Exists(path))
                    list.Add(new EditorBuildSettingsScene(path, false));
            }

            // Preserve any other previously registered scenes as disabled.
            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (list.Exists(s => s.path == existing.path)) continue;
                list.Add(new EditorBuildSettingsScene(existing.path, false));
            }

            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log($"[GeidaiSceneBootstrap] Build settings: {list.Count} scenes.");
        }
    }
}
