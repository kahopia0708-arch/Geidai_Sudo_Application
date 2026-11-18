
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game3
{
    [RequireComponent(typeof(RectTransform))]
    public class WizardReactionEffect : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite noteSprite;
        [SerializeField] private Sprite wrongSprite;

        [Header("Materials")]
        [SerializeField] private Material noteMaterial;
        [SerializeField] private Material wrongMaterial;

        [Header("Effect Layer")]
        [Tooltip("未設定なら師匠の親RectTransformを使用")]
        [SerializeField] private RectTransform effectLayer;

        [Header("Correct - Notes")]
        [SerializeField, Range(1, 8)]
        private int noteCount = 4;

        [SerializeField] private float noteSize = 65f;
        [SerializeField] private float noteDuration = 0.6f;
        [SerializeField] private float noteInterval = 0.06f;
        [SerializeField] private float noteRise = 110f;
        [SerializeField] private float noteSpread = 90f;

        [Header("Wrong - Scribble")]
        [SerializeField] private float wrongSize = 125f;
        [SerializeField] private float wrongDuration = 0.75f;
        [SerializeField] private float wrongRise = 25f;
        [SerializeField] private float wrongStartYOffset = 70f;
        [SerializeField] private float wrongRotationTurns = 1.5f;
        private RectTransform _wizardRect;
        private RectTransform _layer;

        private readonly List<GameObject> _activeEffects =
            new List<GameObject>();

        private void Awake()
        {
            _wizardRect = transform as RectTransform;

            _layer = effectLayer != null
                ? effectLayer
                : transform.parent as RectTransform;
        }

        // =========================================
        // 正解：音符を飛ばす
        // =========================================

        [ContextMenu("Test Correct")]
        public void PlayCorrect()
        {
            if (!Application.isPlaying ||
                !isActiveAndEnabled ||
                noteSprite == null ||
                _layer == null)
            {
                return;
            }

            Clear();
            StartCoroutine(SpawnNotes());
        }

        private IEnumerator SpawnNotes()
        {
            for (int i = 0; i < noteCount; i++)
            {
                Image image = CreateEffect(
                    noteSprite,
                    noteMaterial,
                    noteSize,
                    "NoteEffect"
                );

                // 師匠の上半身付近から発生
                Vector2 start = GetWizardPosition(0.2f);

                start += new Vector2(
                    Random.Range(-15f, 15f),
                    Random.Range(-10f, 10f)
                );

                // 左右に散らばりながら上昇
                Vector2 end = start + new Vector2(
                    Random.Range(-noteSpread, noteSpread),
                    noteRise + Random.Range(-15f, 20f)
                );

                float startAngle =
                    Random.Range(-25f, 25f);

                float endAngle =
                    startAngle + Random.Range(-45f, 45f);

                StartCoroutine(
                    AnimateNote(
                        image,
                        start,
                        end,
                        startAngle,
                        endAngle
                    )
                );

                yield return new WaitForSecondsRealtime(
                    Mathf.Max(0f, noteInterval)
                );
            }
        }

        private IEnumerator AnimateNote(
            Image image,
            Vector2 start,
            Vector2 end,
            float startAngle,
            float endAngle)
        {
            RectTransform rect = image.rectTransform;

            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, noteDuration);

            while (elapsed < duration && image != null)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    elapsed / duration
                );

                float eased = 1f - Mathf.Pow(1f - t, 2f);

                // 上昇
                rect.anchoredPosition = Vector2.Lerp(
                    start, end, eased
                );

                // 軽く回転
                rect.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(startAngle, endAngle, t)
                );

                // 出現時に少し膨らむ
                float scale = t < 0.2f
                    ? Mathf.Lerp(0.5f, 1.1f, t / 0.2f)
                    : Mathf.Lerp(1.1f, 1f, (t - 0.2f) / 0.8f);

                rect.localScale = Vector3.one * scale;

                // 後半でフェードアウト
                float alpha = 1f - Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.InverseLerp(0.45f, 1f, t)
                );

                image.color = new Color(
                    1f, 1f, 1f, alpha
                );

                yield return null;
            }

            RemoveEffect(image);
        }

        // =========================================
        // 不正解：ぐるぐる線を表示
        // =========================================

        [ContextMenu("Test Wrong")]
        public void PlayWrong()
        {
            if (!Application.isPlaying ||
                !isActiveAndEnabled ||
                wrongSprite == null ||
                _layer == null)
            {
                return;
            }

            Clear();

            Image image = CreateEffect(
                wrongSprite,
                wrongMaterial,
                wrongSize,
                "WrongEffect"
            );

            StartCoroutine(AnimateWrong(image));
        }

        private IEnumerator AnimateWrong(Image image)
        {
            RectTransform rect = image.rectTransform;

            // 師匠の頭上から、さらに少し上で開始
            Vector2 start =
                GetWizardPosition(0.4f) +
                Vector2.up * wrongStartYOffset;

            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, wrongDuration);

            while (elapsed < duration && image != null)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    elapsed / duration
                );

                // ゆっくり上昇
                float eased = 1f - Mathf.Pow(1f - t, 2f);

                rect.anchoredPosition =
                    start + Vector2.up * wrongRise * eased;

                // 出現時にポンッと膨らむ
                float scale = t < 0.25f
                    ? Mathf.Lerp(0.4f, 1.15f, t / 0.25f)
                    : Mathf.Lerp(1.15f, 1f, (t - 0.25f) / 0.75f);

                rect.localScale = Vector3.one * scale;

                // ぐるぐる回転
                float angle = 360f * wrongRotationTurns * t;

                rect.localRotation = Quaternion.Euler(
                    0f, 0f, angle
                );

                // 後半でフェードアウト
                float alpha = 1f - Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.InverseLerp(0.5f, 1f, t)
                );

                image.color = new Color(
                    1f, 1f, 1f, alpha
                );

                yield return null;
            }

            RemoveEffect(image);
        }

        // =========================================
        // 共通処理
        // =========================================

        private Image CreateEffect(
            Sprite sprite,
            Material material,
            float size,
            string objectName)
        {
            GameObject obj = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            obj.transform.SetParent(_layer, false);
            obj.transform.SetAsLastSibling();

            RectTransform rect =
                obj.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.one * size;
            rect.localScale = Vector3.one;

            Image image = obj.GetComponent<Image>();

            image.sprite = sprite;
            image.material = material;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            _activeEffects.Add(obj);

            return image;
        }

        private Vector2 GetWizardPosition(float heightRatio)
        {
            // 師匠のローカル座標をワールド座標へ
            Vector3 worldPosition = _wizardRect.TransformPoint(
                new Vector3(
                    0f,
                    _wizardRect.rect.height * heightRatio,
                    0f
                )
            );

            // EffectLayerの中心基準の座標へ変換
            Vector2 localPosition =
                _layer.InverseTransformPoint(worldPosition);

            return localPosition - _layer.rect.center;
        }

        private void RemoveEffect(Image image)
        {
            if (image == null)
                return;

            _activeEffects.Remove(image.gameObject);
            Destroy(image.gameObject);
        }

        public void Clear()
        {
            StopAllCoroutines();

            foreach (GameObject obj in _activeEffects)
            {
                if (obj != null)
                    Destroy(obj);
            }

            _activeEffects.Clear();
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}
