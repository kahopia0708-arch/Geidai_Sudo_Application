using System;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services.Audio;
using Geidai.Services.Media;
using Geidai.Services.Storage;

namespace Geidai.Collection
{
    /// <summary>
    /// 詳細・編集パネル（US-COL-01/02 / frontend-components §5）。
    /// 選択音の視聴（保存エフェクト再適用）・タイトル/メモ/写真の編集・削除（確認付き）を担う。
    /// 永続化は <see cref="IStorageService"/>（原子的置換）、視聴は共有 <see cref="IAudioService"/>。
    /// 見た目は S さん調整（US-TECH-07）。失敗は <see cref="ErrorPresenter"/> で平易に提示。
    /// </summary>
    public class SoundDetailController : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Text dateText;
        [SerializeField] private Text nicknameText;
        [SerializeField] private Image photoImage;
        [SerializeField] private GameObject noPhotoPlaceholder;

        [Header("Edit")]
        [SerializeField] private InputField titleInput;
        [SerializeField] private InputField memoInput;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button pickPhotoButton;
        [SerializeField] private Button removePhotoButton;
        [SerializeField] private Button closeButton;

        [Header("Dialogs")]
        [SerializeField] private ConfirmDialog confirmDialog;
        [SerializeField] private ErrorPresenter errorPresenter;

        private IStorageService _storage;
        private IAudioService _audio;
        private IPhotoPicker _photoPicker;
        private SavedSound _current;
        private bool _hooked;

        /// <summary>メタが更新された（一覧の再読込用）。</summary>
        public event Action MetaChanged;
        /// <summary>音が削除された（id）。</summary>
        public event Action<string> Deleted;
        /// <summary>パネルを閉じる要求。</summary>
        public event Action Closed;

        public void Init(IStorageService storage, IAudioService audio, IPhotoPicker photoPicker)
        {
            _storage = storage;
            _audio = audio;
            _photoPicker = photoPicker;
            HookOnce();
        }

        private void HookOnce()
        {
            if (_hooked) return;
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (saveButton != null) saveButton.onClick.AddListener(OnSave);
            if (deleteButton != null) deleteButton.onClick.AddListener(OnDeletePressed);
            if (pickPhotoButton != null) pickPhotoButton.onClick.AddListener(OnPickPhoto);
            if (removePhotoButton != null) removePhotoButton.onClick.AddListener(OnRemovePhoto);
            if (closeButton != null) closeButton.onClick.AddListener(() => Closed?.Invoke());
            _hooked = true;
        }

        /// <summary>指定音を詳細表示する。</summary>
        public void Show(SavedSound sound)
        {
            _current = sound;
            if (sound == null || sound.meta == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            var m = sound.meta;

            if (titleInput != null) titleInput.SetTextWithoutNotify(m.title ?? string.Empty);
            if (memoInput != null) memoInput.SetTextWithoutNotify(m.memo ?? string.Empty);
            if (dateText != null) dateText.text = SoundItemViewModel.FormatDate(m.createdAtIso);
            if (nicknameText != null) nicknameText.text = m.nickname ?? string.Empty;

            RefreshPhoto();
        }

        private void RefreshPhoto()
        {
            bool hasPhoto = _current != null && _current.meta != null && !string.IsNullOrEmpty(_current.meta.photoFileName);
            if (photoImage != null)
            {
                photoImage.sprite = hasPhoto ? LoadPhotoSprite(_current.meta.id) : null;
                photoImage.enabled = photoImage.sprite != null;
            }
            if (noPhotoPlaceholder != null)
                noPhotoPlaceholder.SetActive(photoImage == null || photoImage.sprite == null);
            if (removePhotoButton != null) removePhotoButton.interactable = hasPhoto;
        }

        // --- actions ---

        private void OnPlay()
        {
            if (_current == null || _current.meta == null || _storage == null || _audio == null) return;

            var buf = _storage.LoadSoundBuffer(_current.meta.id);
            if (!buf.IsSuccess)
            {
                Show(_current); // 破損時は状態維持
                if (errorPresenter != null) errorPresenter.ShowFromResult(Result.Fail(buf.Code, buf.Message));
                return;
            }

            var settings = _current.settings ?? new SoundEffectSettingsData();
            var played = _audio.Play(buf.Value, settings);
            if (!played.IsSuccess && errorPresenter != null)
                errorPresenter.ShowFromResult(played);
        }

        private void OnSave()
        {
            if (_current == null || _current.meta == null || _storage == null) return;

            _current.meta.title = titleInput != null ? (titleInput.text ?? string.Empty) : _current.meta.title;
            _current.meta.memo = memoInput != null ? (memoInput.text ?? string.Empty) : _current.meta.memo;

            var result = _storage.SaveMeta(_current.meta);
            if (!result.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(result);
                return;
            }
            MetaChanged?.Invoke();
        }

        private void OnDeletePressed()
        {
            if (_current == null || _current.meta == null) return;
            string id = _current.meta.id;

            Action doDelete = () =>
            {
                var result = _storage.DeleteSound(id);
                if (!result.IsSuccess)
                {
                    if (errorPresenter != null) errorPresenter.ShowFromResult(result);
                    return;
                }
                Deleted?.Invoke(id);
            };

            if (confirmDialog != null)
                confirmDialog.Show("さくじょ", "この おとを けす？", doDelete);
            else
                doDelete();
        }

        private void OnPickPhoto()
        {
            if (_current == null || _current.meta == null || _photoPicker == null || _storage == null) return;
            string id = _current.meta.id;

            _photoPicker.Pick(res =>
            {
                if (!res.IsSuccess)
                {
                    if (errorPresenter != null && res.Code != ResultCode.NotImplemented)
                        errorPresenter.ShowFromResult(Result.Fail(res.Code, res.Message));
                    return;
                }

                var saved = _storage.SavePhoto(id, res.Value);
                if (!saved.IsSuccess)
                {
                    if (errorPresenter != null) errorPresenter.ShowFromResult(Result.Fail(saved.Code, saved.Message));
                    return;
                }

                _current.meta.photoFileName = saved.Value;
                var metaResult = _storage.SaveMeta(_current.meta);
                if (!metaResult.IsSuccess)
                {
                    if (errorPresenter != null) errorPresenter.ShowFromResult(metaResult);
                    return;
                }
                RefreshPhoto();
                MetaChanged?.Invoke();
            });
        }

        private void OnRemovePhoto()
        {
            if (_current == null || _current.meta == null || _storage == null) return;

            var removed = _storage.RemovePhoto(_current.meta.id);
            if (!removed.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(removed);
                return;
            }
            _current.meta.photoFileName = string.Empty;
            var metaResult = _storage.SaveMeta(_current.meta);
            if (!metaResult.IsSuccess)
            {
                if (errorPresenter != null) errorPresenter.ShowFromResult(metaResult);
                return;
            }
            RefreshPhoto();
            MetaChanged?.Invoke();
        }

        private Sprite LoadPhotoSprite(string id)
        {
            if (_storage == null) return null;
            var bytes = _storage.LoadPhoto(id);
            if (!bytes.IsSuccess || bytes.Value == null) return null;
            return CollectionSprites.FromBytes(bytes.Value);
        }
    }
}
