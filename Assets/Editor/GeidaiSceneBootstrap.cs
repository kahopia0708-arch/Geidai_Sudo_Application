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
using Geidai.Common.UI;
using Geidai.Foundation;
using Geidai.Rec;
using Geidai.Collection;
using Geidai.Theme;
using Geidai.Game1;
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

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                es.transform.SetParent(root.transform, false);
            }

            var camGo = new GameObject("Main Camera", typeof(Camera));
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
            img.color = new Color(0.3f, 0.55f, 0.45f, 1f);
            var btn = go.GetComponent<Button>();
            var labelText = CreateText(go.transform, "Label", label, 28, TextAnchor.MiddleCenter);
            StretchFull(labelText.rectTransform);
            labelText.color = Color.white;
            return btn;
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
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiRegisterRoot");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "せってい", 48, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            var nicknameGo = new GameObject("Nickname", typeof(RectTransform), typeof(Image), typeof(InputField));
            nicknameGo.transform.SetParent(content, false);
            var nickRt = nicknameGo.GetComponent<RectTransform>();
            nickRt.sizeDelta = new Vector2(600, 80);
            nickRt.anchoredPosition = new Vector2(0, 200);
            var nickInput = nicknameGo.GetComponent<InputField>();
            var nickText = CreateText(nicknameGo.transform, "Text", "", 28, TextAnchor.MiddleLeft);
            StretchFull(nickText.rectTransform);
            nickInput.textComponent = nickText;

            var dropdownGo = new GameObject("BirthYear", typeof(RectTransform), typeof(Image), typeof(Dropdown));
            dropdownGo.transform.SetParent(content, false);
            var ddRt = dropdownGo.GetComponent<RectTransform>();
            ddRt.sizeDelta = new Vector2(600, 80);
            ddRt.anchoredPosition = new Vector2(0, 320);
            var dropdown = dropdownGo.GetComponent<Dropdown>();
            var caption = CreateText(dropdownGo.transform, "Label", "うまれたとし", 28, TextAnchor.MiddleCenter);
            StretchFull(caption.rectTransform);
            dropdown.captionText = caption;

            var submit = CreateButton(content, "Submit", "けってい", new Vector2(280, 90));
            submit.GetComponent<RectTransform>().anchoredPosition = new Vector2(-160, -100);
            var cancel = CreateButton(content, "Cancel", "もどる", new Vector2(280, 90));
            cancel.GetComponent<RectTransform>().anchoredPosition = new Vector2(160, -100);
            var error = CreateErrorPresenter(content);

            var screenGo = new GameObject("RegisterScreen", typeof(UserRegistrationScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var reg = screenGo.GetComponent<UserRegistrationScreenController>();
            WireScreenRoot(reg, shell.responsive, shell.fitter);
            var so = new SerializedObject(reg);
            so.FindProperty("birthYearDropdown").objectReferenceValue = dropdown;
            so.FindProperty("nicknameInput").objectReferenceValue = nickInput;
            so.FindProperty("submitButton").objectReferenceValue = submit;
            so.FindProperty("cancelButton").objectReferenceValue = cancel;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiRegister");
        }

        public static void BuildRec()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiRecRoot");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "ろくおん", 48, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            var recordBtn = CreateButton(content, "Record", "ろくおん", new Vector2(280, 90));
            recordBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 200);
            var playBtn = CreateButton(content, "Play", "さいせい", new Vector2(280, 90));
            playBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, 200);
            var saveBtn = CreateButton(content, "Save", "ほぞん", new Vector2(280, 90));
            saveBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 50);
            var backBtn = CreateButton(content, "Back", "もどる", new Vector2(280, 90));
            backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, 50);

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

            var listHost = new GameObject("SoundListView", typeof(RectTransform), typeof(SoundListView));
            listHost.transform.SetParent(content, false);
            var listRt = listHost.GetComponent<RectTransform>();
            listRt.sizeDelta = new Vector2(900, 900);
            listRt.anchoredPosition = new Vector2(0, 0);

            var filterHost = new GameObject("FilterSearch", typeof(RectTransform), typeof(FilterSearchController));
            filterHost.transform.SetParent(content, false);

            var detailHost = new GameObject("SoundDetail", typeof(RectTransform), typeof(SoundDetailController));
            detailHost.transform.SetParent(content, false);
            detailHost.SetActive(false);

            var back = CreateButton(content, "Back", "もどる", new Vector2(240, 80));
            back.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -750);
            var error = CreateErrorPresenter(content);

            var screenGo = new GameObject("CollectionScreen", typeof(CollectionScreenController));
            screenGo.transform.SetParent(shell.canvas.transform, false);
            var col = screenGo.GetComponent<CollectionScreenController>();
            WireScreenRoot(col, shell.responsive, shell.fitter);
            var so = new SerializedObject(col);
            so.FindProperty("listView").objectReferenceValue = listHost.GetComponent<SoundListView>();
            so.FindProperty("filterSearch").objectReferenceValue = filterHost.GetComponent<FilterSearchController>();
            so.FindProperty("detail").objectReferenceValue = detailHost.GetComponent<SoundDetailController>();
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("errorPresenter").objectReferenceValue = error;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("GeidaiCollection");
        }

        public static void BuildTheme()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shell = CreateScreenShell("GeidaiThemeRoot");
            var content = shell.safeArea;

            var title = CreateText(content, "Title", "こんしゅうの おだい", 40, TextAnchor.MiddleCenter);
            AnchorTopBand(title.rectTransform, 100f, 32f);

            var themeText = CreateText(content, "ThemeText", "おだい", 56, TextAnchor.MiddleCenter);
            themeText.rectTransform.anchoredPosition = new Vector2(0, 200);
            var reading = CreateText(content, "ReadingText", "", 32, TextAnchor.MiddleCenter);
            reading.rectTransform.anchoredPosition = new Vector2(0, 80);
            var hint = CreateText(content, "HintText", "", 28, TextAnchor.MiddleCenter);
            hint.rectTransform.anchoredPosition = new Vector2(0, -40);

            var empty = new GameObject("EmptyState", typeof(RectTransform), typeof(Text));
            empty.transform.SetParent(content, false);
            empty.GetComponent<Text>().text = "おだいが まだ ないよ";
            empty.GetComponent<Text>().font = themeText.font;
            empty.GetComponent<Text>().fontSize = 32;
            empty.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            empty.GetComponent<Text>().color = Color.gray;
            empty.SetActive(false);

            var record = CreateButton(content, "Record", "ろくおんする", new Vector2(360, 100));
            record.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
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

            // Back button
            var back = CreateButton(content, "Back", "もどる", new Vector2(240, 80));
            back.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -700);
            back.onClick.AddListener(() => { /* runtime: ScreenRootBase back */ });

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
            frogRt.sizeDelta = new Vector2(280, 280);
            frogRt.anchoredPosition = new Vector2(0, 280);
            frogGo.GetComponent<Image>().color = new Color(0.4f, 0.7f, 0.4f);

            var frogPreview = CreateButton(frogGo.transform, "Preview", "きく", new Vector2(120, 50));
            frogPreview.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -160);
            var frog = frogGo.GetComponent<FrogTargetView>();
            var soFrog = new SerializedObject(frog);
            soFrog.FindProperty("previewButton").objectReferenceValue = frogPreview;
            soFrog.FindProperty("dropArea").objectReferenceValue = frogRt;
            soFrog.ApplyModifiedPropertiesWithoutUndo();

            var choices = new List<ChoiceItemView>();
            for (int i = 0; i < 3; i++)
            {
                var choiceGo = new GameObject($"Choice_{i}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(ChoiceItemView));
                choiceGo.transform.SetParent(content, false);
                var crt = choiceGo.GetComponent<RectTransform>();
                crt.sizeDelta = new Vector2(180, 180);
                crt.anchoredPosition = new Vector2(-220 + i * 220, -200);
                choiceGo.GetComponent<Image>().color = new Color(0.55f, 0.75f, 0.9f);
                var preview = CreateButton(choiceGo.transform, "Preview", "きく", new Vector2(100, 40));
                preview.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -90);
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
            resultText.rectTransform.anchoredPosition = new Vector2(0, -500);
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
                "Assets/Home.unity",
                "Assets/Rec.unity",
                "Assets/Game01.unity",
                "Assets/MySoundCollection.unity",
                "Assets/Place.unity",
                "Assets/Scenes/SampleScene.unity",
            };

            var enabledNew = new[]
            {
                "Assets/Main画面.unity",
                "Assets/game_Home.unity",
                $"{SceneDir}/GeidaiHome.unity",
                $"{SceneDir}/GeidaiRegister.unity",
                $"{SceneDir}/GeidaiRec.unity",
                $"{SceneDir}/GeidaiCollection.unity",
                $"{SceneDir}/GeidaiTheme.unity",
                $"{SceneDir}/GeidaiGame1.unity",
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
