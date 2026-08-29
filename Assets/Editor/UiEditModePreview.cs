using Geidai.Foundation;
using Geidai.Library;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Geidai.EditorTools
{
    /// <summary>
    /// 実行時生成 UI を Edit Mode で仮置きする（デザイン調整用）。
    /// プレビュー個体は HideFlags.DontSave のためシーン保存に残らない。
    /// 見た目の恒久変更は Prefab / Settings / シーン上の固定オブジェクトへ。
    /// </summary>
    public static class UiEditModePreview
    {
        private const string HomeScenePath = "Assets/Scenes/Geidai/GeidaiHome.unity";
        private const string LibraryScenePath = "Assets/Scenes/Geidai/GeidaiLibrary.unity";

        [MenuItem("Geidai/UI Preview/Generate Home Menu", priority = 100)]
        public static void GenerateHomeMenu()
        {
            if (!EnsureEditMode("ホームメニュー")) return;
            EnsureSceneOpen(HomeScenePath);
            var home = Object.FindFirstObjectByType<HomeScreenController>(FindObjectsInactive.Include);
            if (home == null)
            {
                EditorUtility.DisplayDialog(
                    "UI Preview",
                    "HomeScreenController が見つかりません。\nGeidaiHome シーンを開いてから再実行してください。",
                    "OK");
                return;
            }

            home.BuildMenu();
            Selection.activeGameObject = home.gameObject;
            Debug.Log("[UI Preview] ホームメニューを Edit Mode に生成しました（DontSave）。ボタン見た目は Prefab、並びは HomeMenuConfig を編集してください。");
        }

        [MenuItem("Geidai/UI Preview/Clear Home Menu Preview", priority = 101)]
        public static void ClearHomeMenu()
        {
            if (!EnsureEditMode("ホームメニュー解除")) return;
            var home = Object.FindFirstObjectByType<HomeScreenController>(FindObjectsInactive.Include);
            if (home == null)
            {
                EditorUtility.DisplayDialog("UI Preview", "HomeScreenController が見つかりません。", "OK");
                return;
            }

            home.ClearMenuPreview();
            Debug.Log("[UI Preview] ホームメニューのプレビューを消しました。");
        }

        [MenuItem("Geidai/UI Preview/Generate Library Grid", priority = 120)]
        public static void GenerateLibraryGrid()
        {
            if (!EnsureEditMode("おとずかん")) return;
            EnsureSceneOpen(LibraryScenePath);
            var library = Object.FindFirstObjectByType<LibraryScreenController>(FindObjectsInactive.Include);
            if (library == null)
            {
                EditorUtility.DisplayDialog(
                    "UI Preview",
                    "LibraryScreenController が見つかりません。\nGeidaiLibrary シーンを開いてから再実行してください。",
                    "OK");
                return;
            }

            library.BuildEditModePreview();
            Selection.activeGameObject = library.gameObject;
            Debug.Log("[UI Preview] おとずかんグリッドを Edit Mode に生成しました（DontSave）。セル見た目は item Prefab、登録音は Curated Sound Catalog を編集してください。");
        }

        [MenuItem("Geidai/UI Preview/Clear Library Grid Preview", priority = 121)]
        public static void ClearLibraryGrid()
        {
            if (!EnsureEditMode("おとずかん解除")) return;
            var library = Object.FindFirstObjectByType<LibraryScreenController>(FindObjectsInactive.Include);
            if (library == null)
            {
                EditorUtility.DisplayDialog("UI Preview", "LibraryScreenController が見つかりません。", "OK");
                return;
            }

            library.ClearEditModePreview();
            Debug.Log("[UI Preview] おとずかんのプレビューを消しました。");
        }

        private static bool EnsureEditMode(string label)
        {
            if (!EditorApplication.isPlaying) return true;
            EditorUtility.DisplayDialog(
                "UI Preview",
                $"{label}のプレビューは Edit Mode 専用です。\nPlay を止めてから実行してください。",
                "OK");
            return false;
        }

        private static void EnsureSceneOpen(string scenePath)
        {
            var active = SceneManager.GetActiveScene();
            if (active.path == scenePath) return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }
}
