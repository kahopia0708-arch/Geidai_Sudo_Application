using Geidai.Services;
using Geidai.Services.Media;

namespace Geidai.Collection
{
    /// <summary>
    /// Collection モジュールの初期化（logical-components §3）。
    /// 共有サービス（Storage/Audio）は <see cref="AppManager"/> が登録済み前提。
    /// 写真ピッカーは未登録なら <see cref="StubPhotoPicker"/> を登録して返す（実機ピッカーはフォローアップ）。
    /// </summary>
    public static class CollectionBootstrap
    {
        /// <summary>共有 <see cref="IPhotoPicker"/> を保証して返す。未登録ならスタブを登録する。</summary>
        public static IPhotoPicker EnsurePhotoPicker()
        {
            var picker = ServiceRegistry.Resolve<IPhotoPicker>();
            if (picker == null)
            {
                picker = new StubPhotoPicker();
                ServiceRegistry.Register<IPhotoPicker>(picker);
            }
            return picker;
        }
    }
}
