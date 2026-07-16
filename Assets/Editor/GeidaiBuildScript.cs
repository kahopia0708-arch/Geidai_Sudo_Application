using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>
    /// Player ビルド用エントリ（Build and Test / CLI `-executeMethod`）。
    /// 署名なし Development APK / iOS Xcode プロジェクト生成を想定。
    /// ストア配布用の本番署名は手動（キーストア / Apple Team）で行う。
    /// </summary>
    public static class GeidaiBuildScript
    {
        private const string DefaultAndroidOut = "Builds/Android/GeidaiSudo.apk";
        private const string DefaultIosOut = "Builds/iOS";

        [MenuItem("Geidai/Build/Android Development APK")]
        public static void BuildAndroidDevelopmentMenu() => BuildAndroidDevelopment();

        [MenuItem("Geidai/Build/iOS Xcode Project")]
        public static void BuildIosXcodeMenu() => BuildIosXcode();

        /// <summary>CLI: -executeMethod Geidai.EditorTools.GeidaiBuildScript.BuildAndroidDevelopment</summary>
        public static void BuildAndroidDevelopment()
        {
            EnsureOutputDir(DefaultAndroidOut);
            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = DefaultAndroidOut,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(options);
            LogReport(report, "Android");
            if (report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }

        /// <summary>CLI: -executeMethod Geidai.EditorTools.GeidaiBuildScript.BuildIosXcode</summary>
        public static void BuildIosXcode()
        {
            EnsureOutputDir(Path.Combine(DefaultIosOut, "dummy"));
            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = DefaultIosOut,
                target = BuildTarget.iOS,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(options);
            LogReport(report, "iOS");
            if (report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
                throw new InvalidOperationException("EditorBuildSettings に有効なシーンがありません。");
            return scenes;
        }

        private static void EnsureOutputDir(string fileOrDirPath)
        {
            string dir = Path.HasExtension(fileOrDirPath)
                ? Path.GetDirectoryName(fileOrDirPath)
                : fileOrDirPath;
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static void LogReport(BuildReport report, string label)
        {
            var s = report.summary;
            Debug.Log($"[GeidaiBuild] {label} result={s.result} size={s.totalSize} time={s.totalTime} errors={s.totalErrors} warnings={s.totalWarnings} path={s.outputPath}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"[GeidaiBuild] {msg.content}");
                }
            }
        }
    }
}
