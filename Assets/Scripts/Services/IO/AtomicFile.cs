using System;
using System.IO;
using Geidai.Common.Results;
using Geidai.Common.Utils;

namespace Geidai.Services.IO
{
    /// <summary>
    /// 原子的置換の共通書込ヘルパー（U4 の主眼 / nfr-design §1・NFR-COL-R1・Q1=A）。
    /// 「一時ファイルへ全内容を書く → flush/close → 本ファイルへ原子的置換」を集約し、
    /// StorageService の profile/meta/wav/写真の全書込をここへ統一する。
    /// 書込を中断/失敗させても本ファイルは旧内容のまま無傷（置換の瞬間まで触れない）。
    /// </summary>
    public static class AtomicFile
    {
        private const string TempSuffix = ".tmp";

        /// <summary>バイト列を原子的に書き込む。</summary>
        public static Result WriteAllBytesAtomic(string path, byte[] data)
        {
            if (string.IsNullOrEmpty(path))
                return Result.Fail(ResultCode.ValidationError, "パスが空です。");
            if (data == null)
                return Result.Fail(ResultCode.ValidationError, "データが空です。");

            return WriteAtomic(path, stream => stream.Write(data, 0, data.Length));
        }

        /// <summary>テキストを原子的に書き込む（UTF-8）。</summary>
        public static Result WriteAllTextAtomic(string path, string text)
        {
            if (string.IsNullOrEmpty(path))
                return Result.Fail(ResultCode.ValidationError, "パスが空です。");
            if (text == null)
                return Result.Fail(ResultCode.ValidationError, "データが空です。");

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            return WriteAtomic(path, stream => stream.Write(bytes, 0, bytes.Length));
        }

        /// <summary>
        /// 既存ファイルを別ファイルへ原子的にコピー（写真の取込等）。
        /// 一時ファイル経由で行い、失敗時は本ファイルを壊さない。
        /// </summary>
        public static Result CopyAtomic(string sourcePath, string destPath)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                return Result.Fail(ResultCode.NotFound, "コピー元が見つかりません。");

            try
            {
                byte[] data = File.ReadAllBytes(sourcePath);
                return WriteAllBytesAtomic(destPath, data);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[AtomicFile] CopyAtomic failed: " + e.Message);
                return Result.Fail(ResultCode.IOError, "コピーに失敗しました。");
            }
        }

        private static Result WriteAtomic(string path, Action<FileStream> writeBody)
        {
            string tmpPath = path + TempSuffix;
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                // 1) 一時ファイルへ全内容を書き、flush してから閉じる。
                using (var stream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    writeBody(stream);
                    stream.Flush(true);
                }

                // 2) 本ファイルへ原子的置換（既存あり=Replace／新規=Move）。
                Replace(tmpPath, path);
                return Result.Ok();
            }
            catch (Exception e)
            {
                SafeLogger.Error("[AtomicFile] write failed: " + e.Message);
                // 3) 失敗時は一時ファイルを破棄（本ファイルは無傷）。
                TryDelete(tmpPath);
                return Result.Fail(ResultCode.IOError, "保存に失敗しました。");
            }
        }

        private static void Replace(string tmpPath, string finalPath)
        {
            if (File.Exists(finalPath))
            {
                // File.Replace は同一ボリューム前提で原子的。バックアップは作らない。
                File.Replace(tmpPath, finalPath, null);
            }
            else
            {
                File.Move(tmpPath, finalPath);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                SafeLogger.Warn("[AtomicFile] tmp cleanup failed: " + e.Message);
            }
        }
    }
}
