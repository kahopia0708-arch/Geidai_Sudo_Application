using UnityEngine;

namespace Geidai.Collection
{
    /// <summary>
    /// 端末内の写真バイト列から表示用 <see cref="Sprite"/> を生成するヘルパー。
    /// 端末内のみで完結（外部送信なし / NFR-COL-Priv2）。失敗時は null（呼び出し側が placeholder）。
    /// </summary>
    public static class CollectionSprites
    {
        public static Sprite FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes)) // jpg/png を自動判別してデコード
            {
                Object.Destroy(tex);
                return null;
            }

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}
