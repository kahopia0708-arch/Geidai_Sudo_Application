using Geidai.Common.Game;
using Geidai.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game2
{
    /// <summary>神経衰弱画面のランタイム UI（雲・枚数ポップアップ・クリア確認）。</summary>
    public static class MemoryMatchUiBootstrap
    {
        private const string CloudName = "Game2CloudBackground";
        private const string PairCountPopupName = "PairCountPopup";
        private const string PairCountGridName = "PairCountGrid";
        private const string PopupConfirmName = "PopupConfirm";
        private const string ChangePairCountButtonName = "ChangePairCountButton";
        private const string ConfirmDialogName = "MemoryMatchConfirmDialog";
        private const string LegacyBarName = "PairCountBar";
        private const string LegacyStartName = "StartGameButton";

        private static readonly Color PresetNormal = new Color(0.95f, 0.97f, 1f);
        private static readonly Color PresetSelected = new Color(0.72f, 0.86f, 1f);
        private static readonly Color CanvasBackdropColor = new Color(0.88f, 0.93f, 0.98f);

        public static MemoryMatchUiHandles EnsureChrome(
            RectTransform contentRoot,
            MemoryMatchGameController controller,
            Font font,
            Button backButton,
            Text titleText)
        {
            var handles = new MemoryMatchUiHandles();
            if (contentRoot == null || controller == null) return handles;

            HideLegacyInlineUi(contentRoot);

            Game2ScreenLayout.ApplyBackButton(backButton != null ? backButton.GetComponent<RectTransform>() : null);
            Game2ScreenLayout.ApplyTitle(titleText != null ? titleText.rectTransform : null, titleText);

            EnsureCloudBackground(contentRoot);
            GeidaiUiLayout.EnsureCanvasBackdrop(contentRoot, CanvasBackdropColor);
            EnsurePairCountPopup(contentRoot, controller, font, handles);
            handles.ChangePairCountButton = EnsureChangePairCountButton(contentRoot, controller, font);

            Text yesLabel;
            Text noLabel;
            handles.ConfirmDialog = EnsureConfirmDialog(contentRoot, font, out yesLabel, out noLabel);
            handles.ConfirmYesLabel = yesLabel;
            handles.ConfirmNoLabel = noLabel;
            return handles;
        }

        public static void SetPairCountPopupVisible(MemoryMatchUiHandles handles, bool visible)
        {
            if (handles?.PairCountPopupRoot == null) return;
            if (visible)
            {
                var rt = handles.PairCountPopupRoot.transform as RectTransform;
                if (rt != null)
                {
                    GeidaiUiLayout.StretchFull(rt);
                    rt.SetAsLastSibling();
                }
            }

            handles.PairCountPopupRoot.SetActive(visible);
        }

        public static void SetChangePairCountVisible(Button button, bool visible)
        {
            if (button != null) button.gameObject.SetActive(visible);
        }

        public static void SetCardGridVisible(RectTransform cardGrid, bool visible)
        {
            if (cardGrid != null) cardGrid.gameObject.SetActive(visible);
        }

        public static void SetPopupConfirmLabel(MemoryMatchUiHandles handles, string text)
        {
            if (handles?.PopupConfirmLabel != null)
                handles.PopupConfirmLabel.text = text ?? string.Empty;
        }

        public static void UpdatePairSelection(MemoryMatchUiHandles handles, int selectedPairCount)
        {
            if (handles?.PairPresetButtons == null || handles.PairPresetValues == null) return;
            for (int i = 0; i < handles.PairPresetButtons.Length; i++)
            {
                var btn = handles.PairPresetButtons[i];
                if (btn == null) continue;
                bool selected = handles.PairPresetValues[i] == selectedPairCount;
                var img = btn.GetComponent<Image>();
                if (img != null)
                    img.color = selected ? PresetSelected : PresetNormal;
                btn.transform.localScale = selected ? Vector3.one * 1.04f : Vector3.one;
            }
        }

        public static void ApplyPlayAreaLayout(RectTransform cardGrid)
        {
            Game2ScreenLayout.ApplyCardGridForPlay(cardGrid);
        }

        private static void HideLegacyInlineUi(RectTransform contentRoot)
        {
            var bar = contentRoot.Find(LegacyBarName);
            if (bar != null) bar.gameObject.SetActive(false);
            var start = contentRoot.Find(LegacyStartName);
            if (start != null) start.gameObject.SetActive(false);
        }

        private static void EnsureCloudBackground(RectTransform parent)
        {
            var existing = parent.Find(CloudName) as RectTransform;
            if (existing == null)
            {
                var go = new GameObject(CloudName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
                go.transform.SetAsFirstSibling();
                existing = go.GetComponent<RectTransform>();
                GeidaiStretchRect.Full(existing);
            }

            var img = existing.GetComponent<Image>();
            var cloud = Game2CardArt.ResolveCloud();
            if (cloud != null)
            {
                img.sprite = cloud;
                img.color = Color.white;
                img.preserveAspect = true;
                img.type = Image.Type.Simple;
            }
            else
            {
                img.sprite = null;
                img.color = new Color(0.88f, 0.93f, 0.98f);
            }
        }

        private static void EnsurePairCountPopup(
            RectTransform parent,
            MemoryMatchGameController controller,
            Font font,
            MemoryMatchUiHandles handles)
        {
            var overlayRoot = GeidaiUiLayout.ResolveCanvasOverlayRoot(parent) ?? parent;
            var popupRoot = overlayRoot.Find(PairCountPopupName) as RectTransform
                ?? parent.Find(PairCountPopupName) as RectTransform;
            if (popupRoot == null)
            {
                var popupGo = new GameObject(PairCountPopupName, typeof(RectTransform), typeof(Image));
                popupGo.transform.SetParent(overlayRoot, false);
                popupRoot = popupGo.GetComponent<RectTransform>();
                GeidaiUiLayout.StretchFull(popupRoot);
                popupGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

                var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
                box.transform.SetParent(popupRoot, false);
                var boxRt = box.GetComponent<RectTransform>();
                boxRt.anchorMin = new Vector2(0.5f, 0.5f);
                boxRt.anchorMax = new Vector2(0.5f, 0.5f);
                boxRt.sizeDelta = new Vector2(920f, 780f);
                box.GetComponent<Image>().color = new Color(0.98f, 0.99f, 1f, 0.98f);

                var gridGo = new GameObject(PairCountGridName, typeof(RectTransform), typeof(GridLayoutGroup));
                gridGo.transform.SetParent(box.transform, false);
                var gridRt = gridGo.GetComponent<RectTransform>();
                gridRt.anchorMin = new Vector2(0.05f, 0.22f);
                gridRt.anchorMax = new Vector2(0.95f, 0.92f);
                gridRt.offsetMin = Vector2.zero;
                gridRt.offsetMax = Vector2.zero;

                var confirmGo = new GameObject(PopupConfirmName, typeof(RectTransform), typeof(Image), typeof(Button));
                confirmGo.transform.SetParent(box.transform, false);
                var confirmRt = confirmGo.GetComponent<RectTransform>();
                confirmRt.anchorMin = new Vector2(0.12f, 0.06f);
                confirmRt.anchorMax = new Vector2(0.88f, 0.18f);
                confirmRt.offsetMin = Vector2.zero;
                confirmRt.offsetMax = Vector2.zero;
                confirmGo.GetComponent<Image>().color = new Color(0.55f, 0.78f, 0.55f);

                var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                textGo.transform.SetParent(confirmGo.transform, false);
                var label = textGo.GetComponent<Text>();
                label.font = font;
                label.fontSize = 40;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = new Color(0.08f, 0.32f, 0.18f);
                label.text = "はじめる";
                GeidaiStretchRect.Full(label.rectTransform);
            }

            MountFullScreenOverlay(parent, popupRoot);
            handles.PairCountPopupRoot = popupRoot.gameObject;
            handles.PopupConfirmButton = popupRoot.Find("Box/" + PopupConfirmName)?.GetComponent<Button>();
            if (handles.PopupConfirmButton != null)
            {
                handles.PopupConfirmLabel = handles.PopupConfirmButton.GetComponentInChildren<Text>();
                handles.PopupConfirmButton.onClick.RemoveListener(controller.ConfirmPairCountPopup);
                handles.PopupConfirmButton.onClick.AddListener(controller.ConfirmPairCountPopup);
            }

            var gridTransform = popupRoot.Find("Box/" + PairCountGridName) as RectTransform;
            BuildPairPresetsInGrid(gridTransform, controller, font, handles);
            popupRoot.gameObject.SetActive(false);
        }

        private static void BuildPairPresetsInGrid(
            RectTransform bar,
            MemoryMatchGameController controller,
            Font font,
            MemoryMatchUiHandles handles)
        {
            if (bar == null) return;

            var grid = bar.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = bar.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(Game2ScreenLayout.PresetButtonSize, Game2ScreenLayout.PresetButtonSize);
            grid.spacing = new Vector2(Game2ScreenLayout.PresetBarSpacing, Game2ScreenLayout.PresetBarSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Game2ScreenLayout.PresetGridColumns;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(8, 8, 8, 8);

            for (int i = bar.childCount - 1; i >= 0; i--)
                Object.Destroy(bar.GetChild(i).gameObject);

            int n = MemoryMatchPairPresets.CardCounts.Length;
            handles.PairPresetButtons = new Button[n];
            handles.PairPresetValues = new int[n];

            for (int i = 0; i < n; i++)
            {
                int cardCount = MemoryMatchPairPresets.CardCounts[i];
                int pairs = MemoryMatchPairPresets.PairCountForIndex(i);
                handles.PairPresetValues[i] = pairs;
                var btn = CreatePresetButton(bar, font, $"{cardCount}\nまい", pairs, i, controller);
                btn.name = $"PairPreset_{pairs}";
                handles.PairPresetButtons[i] = btn;
            }
        }

        private static Button EnsureChangePairCountButton(
            RectTransform parent,
            MemoryMatchGameController controller,
            Font font)
        {
            var existing = parent.Find(ChangePairCountButtonName);
            if (existing == null)
            {
                var go = new GameObject(ChangePairCountButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
                Game2ScreenLayout.ApplyChangePairCountButton(go.GetComponent<RectTransform>());
                go.GetComponent<Image>().color = new Color(0.95f, 0.97f, 1f);

                var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                textGo.transform.SetParent(go.transform, false);
                var text = textGo.GetComponent<Text>();
                text.font = font;
                text.fontSize = 28;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(0.15f, 0.25f, 0.4f);
                text.text = "枚数";
                GeidaiStretchRect.Full(text.rectTransform);
                existing = go.transform;
            }

            var button = existing.GetComponent<Button>();
            button.onClick.RemoveListener(controller.OpenPairCountPopup);
            button.onClick.AddListener(controller.OpenPairCountPopup);
            Game2ScreenLayout.ApplyChangePairCountButton(existing.GetComponent<RectTransform>());
            existing.gameObject.SetActive(false);
            return button;
        }

        private static Button CreatePresetButton(
            Transform parent,
            Font font,
            string label,
            int pairCount,
            int presetIndex,
            MemoryMatchGameController controller)
        {
            var go = new GameObject("Preset", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = PresetNormal;

            var decoGo = new GameObject("Deco", typeof(RectTransform), typeof(Image));
            decoGo.transform.SetParent(go.transform, false);
            GeidaiStretchRect.Full(decoGo.GetComponent<RectTransform>());
            var deco = decoGo.GetComponent<Image>();
            var decoSprite = Game2CardArt.ResolvePresetDecoration(presetIndex);
            if (decoSprite != null)
            {
                deco.sprite = decoSprite;
                deco.color = new Color(1f, 1f, 1f, 0.55f);
                deco.preserveAspect = true;
            }

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = font;
            text.fontSize = 36;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.12f, 0.22f, 0.38f);
            text.text = label;
            GeidaiStretchRect.Full(text.rectTransform);

            var button = go.GetComponent<Button>();
            int captured = pairCount;
            button.onClick.AddListener(() => controller.OnPairCountPresetClicked(captured));
            return button;
        }

        private static ConfirmDialog EnsureConfirmDialog(
            RectTransform parent,
            Font font,
            out Text yesLabel,
            out Text noLabel)
        {
            yesLabel = null;
            noLabel = null;
            var overlayRoot = GeidaiUiLayout.ResolveCanvasOverlayRoot(parent) ?? parent;
            var existing = overlayRoot.Find(ConfirmDialogName) ?? parent.Find(ConfirmDialogName);
            if (existing != null)
            {
                MountFullScreenOverlay(parent, existing as RectTransform);
                yesLabel = existing.Find("Box/Yes/Text")?.GetComponent<Text>();
                noLabel = existing.Find("Box/No/Text")?.GetComponent<Text>();
                var existingDialog = existing.GetComponent<ConfirmDialog>();
                var existingBox = existing.Find("Box");
                var existingTitle = existingBox?.Find("Title")?.GetComponent<Text>();
                var existingMessage = existingBox?.Find("Message")?.GetComponent<Text>();
                var existingYes = existingBox?.Find("Yes")?.GetComponent<Button>();
                var existingNo = existingBox?.Find("No")?.GetComponent<Button>();
                if (existingDialog != null && existingTitle != null && existingMessage != null
                    && existingYes != null && existingNo != null)
                {
                    existingDialog.Initialize(
                        existing.gameObject,
                        existingTitle,
                        existingMessage,
                        existingYes,
                        existingNo);
                }

                return existingDialog;
            }

            return CreateConfirmDialog(parent, font, out yesLabel, out noLabel);
        }

        private static ConfirmDialog CreateConfirmDialog(
            RectTransform parent,
            Font font,
            out Text yesLabel,
            out Text noLabel)
        {
            yesLabel = null;
            noLabel = null;

            var panel = new GameObject(ConfirmDialogName, typeof(RectTransform), typeof(Image), typeof(ConfirmDialog));
            panel.transform.SetParent(GeidaiUiLayout.ResolveCanvasOverlayRoot(parent) ?? parent, false);
            GeidaiUiLayout.StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.45f);
            panel.SetActive(false);

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(panel.transform, false);
            var boxRt = box.GetComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.5f, 0.5f);
            boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.sizeDelta = new Vector2(700, 360);
            box.GetComponent<Image>().color = Color.white;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(box.transform, false);
            var title = titleGo.GetComponent<Text>();
            title.font = font;
            title.fontSize = 32;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.black;
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.08f, 0.62f);
            titleRt.anchorMax = new Vector2(0.92f, 0.92f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            var messageGo = new GameObject("Message", typeof(RectTransform), typeof(Text));
            messageGo.transform.SetParent(box.transform, false);
            var message = messageGo.GetComponent<Text>();
            message.font = font;
            message.fontSize = 26;
            message.alignment = TextAnchor.MiddleCenter;
            message.color = Color.black;
            var msgRt = message.rectTransform;
            msgRt.anchorMin = new Vector2(0.08f, 0.38f);
            msgRt.anchorMax = new Vector2(0.92f, 0.6f);
            msgRt.offsetMin = Vector2.zero;
            msgRt.offsetMax = Vector2.zero;

            var yes = CreateDialogButton(box.transform, "Yes", "はい", font, new Vector2(-140, -100));
            var no = CreateDialogButton(box.transform, "No", "いいえ", font, new Vector2(140, -100));
            yesLabel = yes.GetComponentInChildren<Text>();
            noLabel = no.GetComponentInChildren<Text>();

            var dialog = panel.GetComponent<ConfirmDialog>();
            dialog.Initialize(panel, title, message, yes, no);
            return dialog;
        }

        private static Button CreateDialogButton(Transform parent, string name, string label, Font font, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(200, 70);
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = new Color(0.9f, 0.92f, 0.96f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = font;
            text.fontSize = 26;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.text = label;
            GeidaiStretchRect.Full(text.rectTransform);
            return go.GetComponent<Button>();
        }

        private static void MountFullScreenOverlay(RectTransform contentRoot, RectTransform overlay)
        {
            if (contentRoot == null || overlay == null) return;
            var root = GeidaiUiLayout.ResolveCanvasOverlayRoot(contentRoot) ?? contentRoot;
            if (overlay.parent != root)
                overlay.SetParent(root, false);
            GeidaiUiLayout.StretchFull(overlay);
        }
    }
}
