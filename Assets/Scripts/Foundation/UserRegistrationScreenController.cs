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
    /// 生年はドロップダウン（1900〜今年）、ニックネームは 1〜8 文字。検証は U1 ValidationUtil を再利用し、
    /// 全通過時のみ UserProfile を生成して保存する。PII はログ非出力・端末外送信なし（NFR-04）。
    /// </summary>
    public class UserRegistrationScreenController : ScreenRootBase
    {
        private const string BirthYearPlaceholder = "えらんでね";

        [Header("U2 Registration")]
        [SerializeField] private RegistrationMode mode = RegistrationMode.New;
        [SerializeField] private Dropdown birthYearDropdown;
        [SerializeField] private InputField nicknameInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ErrorPresenter errorPresenter;

        private void Awake()
        {
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
            PopulateBirthYears();
            if (mode == RegistrationMode.Edit) LoadExisting();
        }

        /// <summary>遷移元から New/Edit を指定する（ProfileEdit=Edit）。</summary>
        public void Initialize(RegistrationMode registrationMode)
        {
            mode = registrationMode;
            if (IsVisible)
            {
                PopulateBirthYears();
                if (mode == RegistrationMode.Edit) LoadExisting();
            }
        }

        private void PopulateBirthYears()
        {
            if (birthYearDropdown == null) return;

            var options = new List<string> { BirthYearPlaceholder };
            int currentYear = DateTime.Now.Year;
            for (int year = ValidationUtil.MinBirthYear; year <= currentYear; year++)
            {
                options.Add(year.ToString());
            }

            birthYearDropdown.ClearOptions();
            birthYearDropdown.AddOptions(options);
            birthYearDropdown.value = 0;
            birthYearDropdown.RefreshShownValue();
        }

        private void LoadExisting()
        {
            var storage = ServiceRegistry.Resolve<IStorageService>();
            if (storage == null) return;

            var result = storage.LoadProfile();
            if (!result.IsSuccess || result.Value == null) return;

            var profile = result.Value;
            if (nicknameInput != null) nicknameInput.text = profile.nickname;
            if (birthYearDropdown != null)
            {
                int index = profile.birthYear - ValidationUtil.MinBirthYear + 1; // +1 はプレースホルダ分
                if (index >= 1 && index < birthYearDropdown.options.Count)
                {
                    birthYearDropdown.value = index;
                    birthYearDropdown.RefreshShownValue();
                }
            }
        }

        private int SelectedBirthYear()
        {
            if (birthYearDropdown == null || birthYearDropdown.value <= 0) return 0; // 未選択→検証で弾く
            return ValidationUtil.MinBirthYear + (birthYearDropdown.value - 1);
        }

        public void OnSubmit()
        {
            int birthYear = SelectedBirthYear();
            string nickname = nicknameInput != null ? nicknameInput.text : string.Empty;

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
                if (errorPresenter != null) errorPresenter.ShowFromResult(saveResult); // フォーム維持で再試行
                return;
            }

            SafeLogger.Log("[Registration] profile saved.");
            GoHome();
        }

        private void OnCancel()
        {
            GoHome(); // 変更破棄でホームへ（Edit のキャンセル / BR-09）
        }

        /// <summary>登録(New)・編集(Edit) いずれもホームへ戻る（frontend-components §2.3）。</summary>
        public override void OnBackPressed()
        {
            GoHome();
        }

        private void GoHome()
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null) return;
            var result = nav.GoTo(SceneId.Home);
            if (!result.IsSuccess && errorPresenter != null) errorPresenter.ShowFromResult(result);
        }
    }
}
