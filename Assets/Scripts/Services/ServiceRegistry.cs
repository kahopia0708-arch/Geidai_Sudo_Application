using System;
using System.Collections.Generic;

namespace Geidai.Services
{
    /// <summary>
    /// 軽量サービスロケータ（Q6=A / nfr-design §7）。
    /// インターフェース単位で登録/解決し、テストでモック差し替えを可能にする。
    /// 本格 DI コンテナは導入しない（保守簡素・オーバーヘッド回避）。
    /// </summary>
    public static class ServiceRegistry
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            Services[typeof(T)] = service;
        }

        public static T Resolve<T>() where T : class
        {
            return Services.TryGetValue(typeof(T), out var service) ? (T)service : null;
        }

        public static bool IsRegistered<T>() where T : class
        {
            return Services.ContainsKey(typeof(T));
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
