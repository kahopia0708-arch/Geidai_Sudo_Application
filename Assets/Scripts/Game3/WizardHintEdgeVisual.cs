
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace Geidai.Game3
{
    [RequireComponent(typeof(Image))]
    public class WizardHintEdgeVisual : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private Image wizardImage;

        [Header("Effect Area")]
        [SerializeField, Min(0f)]
        private float padding = 100f;

        private Image _edgeImage;

        // Project内の元Material
        private Material _sourceMaterial;

        // 実際の描画に使用するMaterial
        private Material _material;

        private bool _playing;

        private void Awake()
        {
            _edgeImage = GetComponent<Image>();

            // 親のWizardImageを自動取得
            if (wizardImage == null && transform.parent != null)
            {
                wizardImage =
                    transform.parent.GetComponent<Image>();
            }

            if (wizardImage == null)
            {
                Debug.LogError(
                    "WizardHintEdgeVisual: WizardImageが設定されていません"
                );
                return;
            }

            Shader shader =
                Shader.Find("Geidai/Game3/WizardHintEdge");

            if (shader == null)
            {
                Debug.LogError(
                    "WizardHintEdge Shaderが見つかりません"
                );
                return;
            }

            // Imageに設定されている元Materialを取得
            Material original = _edgeImage.material;

            if (original == null || original.shader != shader)
            {
                Debug.LogError(
                    "WizardEdgeEffectのImageに" +
                    "WizardHintEdge.matを設定してください"
                );
                return;
            }

            _sourceMaterial = original;

            // 描画用のMaterialを複製
            // 元Materialを直接変更しないため
            _material = new Material(_sourceMaterial);
            _material.name = "WizardHintEdge (Runtime)";

            _edgeImage.material = _material;

            // UI設定
            _edgeImage.raycastTarget = false;
            _edgeImage.type = Image.Type.Simple;
            _edgeImage.useSpriteMesh = false;

            RefreshAppearance();

            // 初期状態は待機中
            SetPlaying(false);
        }

        private void Update()
        {
            if (_material == null || _sourceMaterial == null)
                return;

            // Project内のMaterial変更を
            // Play中も描画用Materialに反映する
            SyncMaterialProperties();
        }

        private void SyncMaterialProperties()
        {
            // 輪郭の色
            _material.SetColor(
                "_EdgeColor",
                _sourceMaterial.GetColor("_EdgeColor")
            );

            // 再生中の実線の太さ
            _material.SetFloat(
                "_LineWidth",
                _sourceMaterial.GetFloat("_LineWidth")
            );

            // 待機中の光の広がり
            _material.SetFloat(
                "_GlowRadius",
                _sourceMaterial.GetFloat("_GlowRadius")
            );

            // 待機中の光の濃さ
            _material.SetFloat(
                "_GlowOpacity",
                _sourceMaterial.GetFloat("_GlowOpacity")
            );

            // 待機中の揺らめく速さ
            _material.SetFloat(
                "_GlowSpeed",
                _sourceMaterial.GetFloat("_GlowSpeed")
            );
        }

        // 輪郭の描画領域を設定
        public void RefreshAppearance()
        {
            if (wizardImage == null ||
                _edgeImage == null ||
                _material == null)
            {
                return;
            }

            Sprite sprite = wizardImage.sprite;

            if (sprite == null)
                return;

            // 元の師匠と同じ画像を使用
            _edgeImage.sprite = sprite;
            _edgeImage.preserveAspect =
                wizardImage.preserveAspect;

            RectTransform rect =
                _edgeImage.rectTransform;

            RectTransform sourceRect =
                wizardImage.rectTransform;

            // 親のImageより描画領域を広げる
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            rect.offsetMin =
                new Vector2(-padding, -padding);

            rect.offsetMax =
                new Vector2(padding, padding);

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            float width = Mathf.Max(
                1f,
                sourceRect.rect.width
            );

            float height = Mathf.Max(
                1f,
                sourceRect.rect.height
            );

            // 描画領域を広げても
            // 元画像の大きさを維持する
            _material.SetVector(
                "_SourceZoom",
                new Vector4(
                    (width + padding * 2f) / width,
                    (height + padding * 2f) / height,
                    0f,
                    0f
                )
            );

            _material.SetVector(
                "_SourceRectSize",
                new Vector4(
                    width,
                    height,
                    0f,
                    0f
                )
            );

            // SpriteのUV範囲を設定
            _material.SetVector(
                "_SourceUVRect",
                DataUtility.GetOuterUV(sprite)
            );

            // 初期Material設定を反映
            SyncMaterialProperties();
        }

        // ヒント再生状態を切り替える
        public void SetPlaying(bool playing)
        {
            _playing = playing;

            if (_material == null)
                return;

            _material.SetFloat(
                "_Playing",
                _playing ? 1f : 0f
            );
        }

        private void OnDisable()
        {
            // 再表示時に待機状態から開始
            SetPlaying(false);
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                if (_edgeImage != null)
                {
                    _edgeImage.material = _sourceMaterial;
                }

                Destroy(_material);
                _material = null;
            }
        }
    }
}
