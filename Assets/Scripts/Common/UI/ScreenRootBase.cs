using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Geidai.Common.UI
{
    /// <summary>
    /// 全画面コントローラの基底（frontend-components / US-TECH-01/02）。
    /// 表示時に Responsive/SafeArea を必ず適用するライフサイクルを強制する。
    /// 戻る操作は BackRequested イベントで通知（Common は Services に依存しないため、
    /// NavigationService への接続は上位＝Foundation 等が購読して行う）。
    /// </summary>
    public abstract class ScreenRootBase : MonoBehaviour
    {
        [SerializeField] protected ResponsiveCanvasConfigurator responsiveConfigurator;
        [SerializeField] protected SafeAreaFitter safeAreaFitter;

        [Tooltip("端末バック（Android の戻る＝Escape）を購読し OnBackPressed を呼ぶ")]
        [SerializeField] protected bool listenForSystemBack = true;

        [Tooltip("シーン入場時に自動で Show() する（実シーン配線用）。")]
        [SerializeField] protected bool showOnStart = true;

        public bool IsVisible { get; private set; }

        /// <summary>戻る操作が要求されたことを通知する（上位が購読して遷移する）。</summary>
        public event Action BackRequested;

        protected virtual void Start()
        {
            if (showOnStart && !IsVisible) Show();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            ConfigureResponsive();
            ApplySafeArea();
            OnShow();
            IsVisible = true;
        }

        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
            IsVisible = false;
        }

        protected virtual void ConfigureResponsive()
        {
            if (responsiveConfigurator != null) responsiveConfigurator.Configure();
        }

        protected virtual void ApplySafeArea()
        {
            if (safeAreaFitter != null) safeAreaFitter.Apply();
        }

        /// <summary>画面固有の表示初期化（派生クラスで実装）。</summary>
        protected virtual void OnShow() { }

        /// <summary>画面固有の後始末（派生クラスで実装）。</summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// 端末バック（Android の戻る＝Escape）を Input System で受ける（nfr-design §3）。
        /// Player Settings が Input System 専用のため、旧 <c>UnityEngine.Input</c> は使わない。
        /// 表示中かつ購読有効なときのみ OnBackPressed を呼ぶ。
        /// </summary>
        protected virtual void Update()
        {
            if (!IsVisible || !listenForSystemBack) return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                OnBackPressed();
        }

        /// <summary>戻る/システムバック押下時の既定挙動。派生クラスで override して画面別に分岐する。</summary>
        public virtual void OnBackPressed()
        {
            BackRequested?.Invoke();
        }
    }
}
