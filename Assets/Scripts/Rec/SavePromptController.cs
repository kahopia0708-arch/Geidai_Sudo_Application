using System;
using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services.Storage;

namespace Geidai.Rec
{
    /// <summary>
    /// 保存プロンプト（US-REC-03 / frontend-components §2）。
    /// 録音バッファ＋加工設定から <see cref="SavedSound"/> を構築し、
    /// <see cref="IStorageService.SaveSound"/> で wav＋meta を対保存する。
    /// 成否は平易文言で提示（BR-16/19）。設定はスナップショットして保存する。
    /// </summary>
    public class SavePromptController : MonoBehaviour
    {
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private Text statusText;
        [SerializeField] private InputField nameField;
        [SerializeField] private string defaultName = "こえ";

        private IStorageService _storage;

        /// <summary>保存成功。</summary>
        public event Action Saved;
        /// <summary>保存失敗（理由付き）。</summary>
        public event Action<Result> SaveFailed;

        public void Init(IStorageService storage)
        {
            _storage = storage;
        }

        /// <summary>録音バッファと加工設定を保存する。</summary>
        public Result Save(AudioBuffer buffer, SoundEffectSettingsData settings)
        {
            if (_storage == null)
                return Notify(Result.Fail(ResultCode.Unknown, "ほぞんの じゅんびが できてないよ"));
            if (buffer == null || buffer.Samples == null)
                return Notify(Result.Fail(ResultCode.ValidationError, "ほぞんする おとが ないよ"));

            string display = (nameField != null && !string.IsNullOrEmpty(nameField.text))
                ? nameField.text
                : defaultName;

            var meta = SoundClipMeta.CreateNew(display);
            var snapshot = Clone(settings);
            var sound = new SavedSound(meta, snapshot);

            var result = _storage.SaveSound(sound, buffer);
            if (result.IsSuccess)
            {
                if (statusText != null) statusText.text = "ほぞんしたよ！";
                Saved?.Invoke();
                return result;
            }

            return Notify(result);
        }

        private Result Notify(Result result)
        {
            if (errorPresenter != null) errorPresenter.ShowFromResult(result);
            SaveFailed?.Invoke(result);
            return result;
        }

        private static SoundEffectSettingsData Clone(SoundEffectSettingsData s)
        {
            if (s == null) return new SoundEffectSettingsData();
            return new SoundEffectSettingsData
            {
                pitchSemitones = s.pitchSemitones,
                noiseLevel = s.noiseLevel,
                timbre = s.timbre,
                reverb = s.reverb
            };
        }
    }
}
