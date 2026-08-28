using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Common.Utils;
using Geidai.Services;
using Geidai.Services.Storage;
using Geidai.Services.Navigation;

namespace Geidai.Foundation
{
    /// <summary>
    /// 登録/編集画面のコントローラ（US-REG-01/02 / BR-05〜09 / nfr-design §5）。
    /// 年齢は「○ さい」ドロップダウン、ニックネームは 1〜8 文字。内部では birthYear として保存する。
    /// </summary>
    public class UserRegistrationScreenController : ScreenRootBase
    {
        private const string AgePlaceholder = "えらんでね";

        [Header("U2 Registration")]
        [SerializeField] private RegistrationMode mode = RegistrationMode.New;
        [SerializeField] private HomeMenuIconCatalog iconCatalog;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Dropdown birthYearDropdown;
        [SerializeField] private InputField nicknameInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ErrorPresenter errorPresenter;

        private void Awake()
        {
            EnsureWired();
            if (submitButton != null) submitButton.onClick.AddListener(OnSubmit);
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnDestroy()
        {
            if (submitButton != null) submitButton.onClick.RemoveListener(OnSubmit);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancel);
        }

        protected override void OnShow()
        {
            ApplyChrome();
            PopulateAgeOptions();
            if (TryLoadExisting())
                mode = RegistrationMode.Edit;
            else
                mode = RegistrationMode.New;
        }

        /// <summary>遷移元から New/Edit を指定する（ProfileEdit=Edit）。</summary>
        public void Initialize(RegistrationMode registrationMode)
        {
            mode = registrationMode;
            if (IsVisible)
            {
                ApplyChrome();
                PopulateAgeOptions();
                if (mode == RegistrationMode.Edit) TryLoadExisting();
            }
        }

        private void PopulateAgeOptions()
        {
            if (birthYearDropdown == null) return;

            var options = new List<string> { AgePlaceholder };
            for (int age = ValidationUtil.MinAge; age <= ValidationUtil.MaxAge; age++)
            {
                options.Add($"{age} さい");
            }

            birthYearDropdown.ClearOptions();
            birthYearDropdown.AddOptions(options);
            birthYearDropdown.value = 0;
            birthYearDropdown.RefreshShownValue();
        }

        /// <summary>既存プロフィールがあればフォームへ反映。成功時 true。</summary>
        private bool TryLoadExisting()
        {
            var storage = ServiceRegistry.Resolve<IStorageService>();
            if (storage == null) return false;

            var result = storage.LoadProfile();
            if (!result.IsSuccess || result.Value == null) return false;

            var profile = result.Value;
            if (nicknameInput != null) nicknameInput.text = profile.nickname;
            if (birthYearDropdown != null)
            {
                int age = DateTime.Now.Year - profile.birthYear;
                int index = DropdownIndexFromAge(age);
                if (index >= 1 && index < birthYearDropdown.options.Count)
                {
                    birthYearDropdown.value = index;
                    birthYearDropdown.RefreshShownValue();
                }
            }
            return true;
        }

        private static int DropdownIndexFromAge(int age)
        {
            if (age < ValidationUtil.MinAge || age > ValidationUtil.MaxAge) return 0;
            return age - ValidationUtil.MinAge + 1;
        }

        private int SelectedAge()
        {
            if (birthYearDropdown == null || birthYearDropdown.value <= 0) return 0;
            return ValidationUtil.MinAge + (birthYearDropdown.value - 1);
        }

        private static int BirthYearFromAge(int age) => DateTime.Now.Year - age;

        public void OnSubmit()
        {
            int age = SelectedAge();
            string nickname = nicknameInput != null ? nicknameInput.text : string.Empty;

            var ageResult = ValidationUtil.ValidateAge(age);
            if (!ageResult.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(ageResult);
                return;
            }

            int birthYear = BirthYearFromAge(age);
            var birthResult = ValidationUtil.ValidateBirthYear(birthYear);
            if (!birthResult.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(birthResult);
                return;
            }

            var nickResult = ValidationUtil.ValidateNickname(nickname);
            if (!nickResult.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(nickResult);
                return;
            }

            var storage = ServiceRegistry.Resolve<IStorageService>();
            if (storage == null)
            {
                if (errorPresenter != null) errorPresenter.ShowError("ほぞんできませんでした。");
                return;
            }

            var profile = new UserProfile(birthYear, nickname.Trim());
            var saveResult = storage.SaveProfile(profile);
            if (!saveResult.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(saveResult);
                return;
            }

            SafeLogger.Log("[Registration] profile saved.");
            GoHome();
        }

        private void OnCancel() => GoHome();

        public override void OnBackPressed() => GoHome();

        private void GoHome()
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null) return;
            var result = nav.GoTo(SceneId.Home);
            if (!result.IsSuccess && errorPresenter != null) errorPresenter.ShowFromResult(result);
        }

        private void EnsureWired()
        {
            var content = safeAreaFitter != null ? safeAreaFitter.transform : transform;

            if (backgroundImage == null)
            {
                var bg = content.Find("Background");
                if (bg != null) backgroundImage = bg.GetComponent<Image>();
            }

            if (titleText == null)
                titleText = content.Find("Title")?.GetComponent<Text>();

            if (birthYearDropdown == null)
                birthYearDropdown = content.Find("BirthYear")?.GetComponent<Dropdown>();

            if (nicknameInput == null)
                nicknameInput = content.Find("Nickname")?.GetComponent<InputField>();

            if (submitButton == null)
                submitButton = content.Find("Submit")?.GetComponent<Button>();

            if (cancelButton == null)
                cancelButton = content.Find("Cancel")?.GetComponent<Button>();
        }

        private void ApplyChrome()
        {
            EnsureWired();
            EnsureBackground();
            EnsureFieldLabels();
            ApplyFieldChrome();
            ApplyButtonChrome(submitButton);
            ApplyButtonChrome(cancelButton);
            ApplyTitleChrome();
            ApplyFieldLabelStyles();
        }

        private void EnsureBackground()
        {
            if (safeAreaFitter == null) return;

            var content = safeAreaFitter.transform;
            if (backgroundImage == null)
            {
                var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(content, false);
                bgGo.transform.SetAsFirstSibling();
                var rt = bgGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                backgroundImage = bgGo.GetComponent<Image>();
                backgroundImage.raycastTarget = false;
            }

            backgroundImage.color = HomeUiTheme.Background;

            var cam = Camera.main;
            if (cam != null) cam.backgroundColor = HomeUiTheme.Background;
        }

        private void ApplyTitleChrome()
        {
            if (titleText == null) return;
            UiFontResolver.ApplyTo(titleText, HomeUiTheme.ScreenTitle);
            titleText.color = HomeUiTheme.TitleOnBackground;
            titleText.fontStyle = FontStyle.Bold;
        }

        private void EnsureFieldLabels()
        {
            if (safeAreaFitter == null) return;
            var content = safeAreaFitter.transform;

            if (content.Find("BirthYearLabel") != null)
            {
                var legacy = content.Find("BirthYearLabel").GetComponent<Text>();
                if (legacy != null) legacy.text = "なんさい？";
            }
            else
            {
                EnsureFieldLabel(content, "AgeLabel", "なんさい？", 0.12f, 0.74f, 0.88f, 0.79f);
            }

            EnsureFieldLabel(content, "NicknameLabel", "ニックネーム", 0.12f, 0.58f, 0.88f, 0.63f);
        }

        private static void EnsureFieldLabel(
            Transform content, string name, string label, float minX, float minY, float maxX, float maxY)
        {
            var existing = content.Find(name);
            Text text;
            if (existing == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Text));
                go.transform.SetParent(content, false);
                text = go.GetComponent<Text>();
                text.text = label;
                text.alignment = TextAnchor.MiddleLeft;
                text.raycastTarget = false;
                var rt = text.rectTransform;
                rt.anchorMin = new Vector2(minX, minY);
                rt.anchorMax = new Vector2(maxX, maxY);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                text = existing.GetComponent<Text>();
                if (text != null) text.text = label;
            }
        }

        private void ApplyFieldLabelStyles()
        {
            if (safeAreaFitter == null) return;
            var content = safeAreaFitter.transform;
            StyleFieldLabel(content.Find("AgeLabel"));
            StyleFieldLabel(content.Find("BirthYearLabel"));
            StyleFieldLabel(content.Find("NicknameLabel"));
        }

        private static void StyleFieldLabel(Transform labelTransform)
        {
            if (labelTransform == null) return;
            var text = labelTransform.GetComponent<Text>();
            if (text == null) return;
            UiFontResolver.ApplyTo(text, HomeUiTheme.FieldLabel);
            text.color = HomeUiTheme.FieldLabelOnBackground;
            text.fontStyle = FontStyle.Bold;
        }

        private void ApplyFieldChrome()
        {
            HomeUiImageUtil.ApplyInputFill(birthYearDropdown != null ? birthYearDropdown.GetComponent<Image>() : null);
            HomeUiImageUtil.ApplyInputFill(nicknameInput != null ? nicknameInput.GetComponent<Image>() : null);

            if (birthYearDropdown != null && birthYearDropdown.captionText != null)
            {
                UiFontResolver.ApplyTo(birthYearDropdown.captionText, HomeUiTheme.Body);
                birthYearDropdown.captionText.color = HomeUiTheme.MenuText;
            }

            if (nicknameInput != null)
            {
                if (nicknameInput.textComponent != null)
                {
                    UiFontResolver.ApplyTo(nicknameInput.textComponent, HomeUiTheme.Body);
                    nicknameInput.textComponent.color = HomeUiTheme.MenuText;
                }

                if (nicknameInput.placeholder is Text placeholder)
                {
                    UiFontResolver.ApplyTo(placeholder, HomeUiTheme.Placeholder);
                    placeholder.color = HomeUiTheme.PlaceholderText;
                }
            }
        }

        private void ApplyButtonChrome(Button button)
        {
            if (button == null) return;
            var pill = HomeUiImageUtil.ResolvePillSprite(iconCatalog);
            HomeUiImageUtil.ApplyBackground(button.GetComponent<Image>(), pill, Color.white);

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                UiFontResolver.ApplyTo(label, HomeUiTheme.ActionButtonLabel);
                label.color = HomeUiTheme.MenuText;
                label.fontStyle = FontStyle.Bold;
            }
        }
    }
}
