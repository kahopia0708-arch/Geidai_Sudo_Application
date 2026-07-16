using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Geidai.Common.Results;
using Geidai.Services.IO;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// AtomicFile の単体テスト（NFR-COL-R1 / U4）。
    /// 成功時は新値へ更新、失敗（無効パス）時は旧値を保持し `.tmp` を残さないことを検証する。
    /// 生成物は TearDown で削除する。
    /// </summary>
    public class AtomicFileTests
    {
        private readonly List<string> _paths = new List<string>();
        private string Dir => Path.Combine(Application.persistentDataPath, "atomic_test");

        [SetUp]
        public void SetUp()
        {
            _paths.Clear();
            Directory.CreateDirectory(Dir);
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(Dir)) Directory.Delete(Dir, true); }
            catch { /* テスト後始末のため無視 */ }
        }

        private string PathFor(string name)
        {
            string p = Path.Combine(Dir, name);
            _paths.Add(p);
            return p;
        }

        [Test]
        public void WriteText_Creates_New_File_With_Content()
        {
            string p = PathFor("a.txt");
            var result = AtomicFile.WriteAllTextAtomic(p, "hello");

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual("hello", File.ReadAllText(p));
            Assert.IsFalse(File.Exists(p + ".tmp"), ".tmp を残さないこと");
        }

        [Test]
        public void WriteText_Replaces_Existing_Atomically()
        {
            string p = PathFor("b.txt");
            Assert.IsTrue(AtomicFile.WriteAllTextAtomic(p, "old").IsSuccess);
            Assert.IsTrue(AtomicFile.WriteAllTextAtomic(p, "new").IsSuccess);

            Assert.AreEqual("new", File.ReadAllText(p));
            Assert.IsFalse(File.Exists(p + ".tmp"));
        }

        [Test]
        public void WriteBytes_Creates_New_File()
        {
            string p = PathFor("c.bin");
            byte[] data = { 1, 2, 3, 4, 5 };
            var result = AtomicFile.WriteAllBytesAtomic(p, data);

            Assert.IsTrue(result.IsSuccess, result.Message);
            CollectionAssert.AreEqual(data, File.ReadAllBytes(p));
        }

        [Test]
        public void Invalid_Path_Fails_And_Preserves_Existing_Value()
        {
            string p = PathFor("d.txt");
            Assert.IsTrue(AtomicFile.WriteAllTextAtomic(p, "keep").IsSuccess);

            // 無効なパス（既存ファイルをディレクトリ扱い）で失敗させる。
            string invalid = Path.Combine(p, "child.txt");
            LogAssert.Expect(LogType.Error, new Regex(@"\[AtomicFile\] write failed"));
            var result = AtomicFile.WriteAllTextAtomic(invalid, "boom");

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.IOError, result.Code);
            // 元ファイルは無傷
            Assert.AreEqual("keep", File.ReadAllText(p));
            Assert.IsFalse(File.Exists(invalid + ".tmp"), "失敗時に .tmp を残さないこと");
        }

        [Test]
        public void CopyAtomic_Copies_File()
        {
            string src = PathFor("src.txt");
            string dest = PathFor("dest.txt");
            Assert.IsTrue(AtomicFile.WriteAllTextAtomic(src, "copyme").IsSuccess);

            var result = AtomicFile.CopyAtomic(src, dest);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual("copyme", File.ReadAllText(dest));
        }

        [Test]
        public void CopyAtomic_Missing_Source_Fails()
        {
            string dest = PathFor("dest2.txt");
            var result = AtomicFile.CopyAtomic(Path.Combine(Dir, "nope.txt"), dest);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.NotFound, result.Code);
        }
    }
}
