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
    /// U2 で Register（登録シーン）・GameSelect（既存 game_Home）を登録。
    /// Theme（weekly theme 専用画面）は U5 でシーン整備するまで未登録（NotFound を返す＝安全処理 / BR-14）。
    /// </summary>
    public class NavigationService : INavigationService
    {
        private static readonly Dictionary<SceneId, string> SceneMap = new Dictionary<SceneId, string>
        {
            { SceneId.Boot, "Main画面" },
            { SceneId.Home, "Home" },
            { SceneId.Register, "Register" },
            { SceneId.Rec, "Rec" },
            { SceneId.Collection, "MySoundCollection" },
            { SceneId.Game1, "Game01" },
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
