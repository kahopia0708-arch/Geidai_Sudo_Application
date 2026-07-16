using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>
    /// EditMode 全件実行（Build and Test）。CLI: -executeMethod Geidai.EditorTools.GeidaiTestRunner.RunEditMode
    /// 結果は Logs/editmode-summary.txt に書き出す。
    /// </summary>
    public static class GeidaiTestRunner
    {
        private static bool _running;
        private static ResultCallbacks _callbacks; // GC されないよう保持
        private static TestRunnerApi _api;

        [MenuItem("Geidai/Tests/Run EditMode All")]
        public static void RunEditModeMenu() => RunEditMode();

        public static void RunEditMode()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[GeidaiTests] Play Mode 中は EditMode を実行できないため停止します。");
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += RunEditMode;
                return;
            }

            if (_running)
            {
                Debug.LogWarning("[GeidaiTests] already running — force restart");
                _running = false;
                _callbacks = null;
                _api = null;
            }

            string dir = Path.GetFullPath("Logs");
            Directory.CreateDirectory(dir);
            string summaryPath = Path.Combine(dir, "editmode-summary.txt");
            File.WriteAllText(summaryPath, "STARTED " + DateTime.Now.ToString("o") + Environment.NewLine);

            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter { testMode = TestMode.EditMode };
            _running = true;

            _callbacks = new ResultCallbacks(summaryPath, () =>
            {
                _running = false;
                _callbacks = null;
                _api = null;
            });
            _api.RegisterCallbacks(_callbacks);
            _api.Execute(new ExecutionSettings(filter));
            Debug.Log("[GeidaiTests] EditMode started → " + summaryPath);
        }

        private sealed class ResultCallbacks : ICallbacks
        {
            private readonly string _summaryPath;
            private readonly Action _onDone;

            public ResultCallbacks(string summaryPath, Action onDone)
            {
                _summaryPath = summaryPath;
                _onDone = onDone;
            }

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"FINISHED {DateTime.Now:o}");
                sb.AppendLine($"pass={result.PassCount}");
                sb.AppendLine($"fail={result.FailCount}");
                sb.AppendLine($"skip={result.SkipCount}");
                sb.AppendLine($"inconclusive={result.InconclusiveCount}");
                sb.AppendLine($"status={result.TestStatus}");
                AppendFailures(result, sb, 0);
                File.WriteAllText(_summaryPath, sb.ToString());
                Debug.Log("[GeidaiTests] " + sb.ToString().Replace("\n", " | "));
                _onDone?.Invoke();
            }

            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            private static void AppendFailures(ITestResultAdaptor node, System.Text.StringBuilder sb, int depth)
            {
                if (node == null) return;
                bool isLeafFail = (node.HasChildren == false) &&
                                  (node.TestStatus == TestStatus.Failed || node.TestStatus == TestStatus.Inconclusive);
                if (isLeafFail)
                {
                    sb.AppendLine("--- FAIL ---");
                    sb.AppendLine("name=" + node.FullName);
                    sb.AppendLine("message=" + (node.Message ?? ""));
                    if (!string.IsNullOrEmpty(node.StackTrace))
                        sb.AppendLine("stack=" + node.StackTrace);
                }
                if (node.HasChildren)
                {
                    foreach (var child in node.Children)
                        AppendFailures(child, sb, depth + 1);
                }
            }
        }
    }
}
