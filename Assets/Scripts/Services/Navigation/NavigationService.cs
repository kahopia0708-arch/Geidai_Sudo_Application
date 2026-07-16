using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.Utils;

namespace Geidai.Services.Navigation
{
    /// <summary>
    /// SceneId → 実シーン名のマップで型安全に遷移する（FR-02）。
    /// Place は列挙に含めない＝導線から除外（BR-15）。
    /// MCP フォローアップで Geidai* シーンへ切り替え（既存ブラウンフィールド名と衝突回避）。
    /// GameSelect は既存 game_Home を維持。Boot は既存 Main画面。
    /// </summary>
    public class NavigationService : INavigationService
    {
        private static readonly Dictionary<SceneId, string> SceneMap = new Dictionary<SceneId, string>
        {
            { SceneId.Boot, "Main画面" },
            { SceneId.Home, "GeidaiHome" },
            { SceneId.Register, "GeidaiRegister" },
            { SceneId.Rec, "GeidaiRec" },
            { SceneId.Collection, "GeidaiCollection" },
            { SceneId.Theme, "GeidaiTheme" },
            { SceneId.Game1, "GeidaiGame1" },
            { SceneId.GameSelect, "game_Home" }
        };

        private readonly Stack<SceneId> _history = new Stack<SceneId>();

        public SceneId? Current { get; private set; }

        public Result GoTo(SceneId sceneId)
        {
            if (!SceneMap.TryGetValue(sceneId, out var sceneName))
                return Result.Fail(ResultCode.NotFound, $"シーン未定義: {sceneId}");

            try
            {
                if (Current.HasValue) _history.Push(Current.Value);
                SceneManager.LoadScene(sceneName);
                Current = sceneId;
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error($"[Navigation] load failed ({sceneName}): {e.Message}");
                return Result.Fail(ResultCode.IOError, "画面の切り替えに失敗しました。");
            }
        }

        public Result GoBack()
        {
            if (_history.Count == 0)
                return Result.Fail(ResultCode.NotFound, "戻り先がありません。");

            var previous = _history.Pop();
            if (!SceneMap.TryGetValue(previous, out var sceneName))
                return Result.Fail(ResultCode.NotFound, "戻り先シーンが未定義です。");

            try
            {
                SceneManager.LoadScene(sceneName);
                Current = previous;
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error($"[Navigation] back failed ({sceneName}): {e.Message}");
                return Result.Fail(ResultCode.IOError, "画面の切り替えに失敗しました。");
            }
        }
    }
}
