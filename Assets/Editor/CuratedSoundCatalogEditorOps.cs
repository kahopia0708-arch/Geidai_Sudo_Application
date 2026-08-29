using System.IO;
using Geidai.Common.Library;
using Geidai.Common.Results;
using UnityEditor;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>
    /// 図鑑カタログ Editor のアセット操作（Window から分離 / U7b NFR Q2=A）。
    /// </summary>
    public static class CuratedSoundCatalogEditorOps
    {
        public const string AudioLibraryFolder = "Assets/Audio/Library";
        public const string DefaultCatalogPath = "Assets/Settings/CuratedSoundCatalog_Default.asset";
        public const string DefaultTimbrePath = "Assets/Settings/TimbreTagCatalog_Default.asset";

        public static void EnsureAudioLibraryFolder()
        {
            if (AssetDatabase.IsValidFolder(AudioLibraryFolder)) return;
            if (!AssetDatabase.IsValidFolder("Assets/Audio"))
                AssetDatabase.CreateFolder("Assets", "Audio");
            AssetDatabase.CreateFolder("Assets/Audio", "Library");
        }

        public static Result<AudioClip> ImportWavToLibrary(string sourcePath, string soundId)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return Result<AudioClip>.Fail(ResultCode.ValidationError, "WAV が みつからないよ");

            if (string.IsNullOrWhiteSpace(soundId))
                return Result<AudioClip>.Fail(ResultCode.ValidationError, "ID を いれてね");

            EnsureAudioLibraryFolder();

            string ext = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(ext)) ext = ".wav";
            string dest = $"{AudioLibraryFolder}/{soundId}{ext}";

            try
            {
                if (File.Exists(dest))
                    File.Delete(dest);
                File.Copy(sourcePath, dest, true);
            }
            catch (System.Exception)
            {
                return Result<AudioClip>.Fail(ResultCode.IOError, "ファイルを コピーできなかったよ");
            }

            AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(dest);
            if (clip == null)
                return Result<AudioClip>.Fail(ResultCode.NotFound, "AudioClip に ならなかったよ");

            return Result<AudioClip>.Ok(clip);
        }

        public static Result SaveSound(
            CuratedSoundCatalog catalog,
            TimbreTagCatalog timbre,
            CuratedSoundDefinition draft,
            string replaceId)
        {
            if (catalog == null)
                return Result.Fail(ResultCode.NotFound, "カタログが ないよ");

            var validation = CuratedSoundValidation.ValidateForUpsert(
                draft,
                catalog.Items,
                timbre,
                replaceId);
            if (!validation.IsSuccess)
                return validation;

            Undo.RecordObject(catalog, "Upsert Curated Sound");
            catalog.Upsert(draft, replaceId);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return Result.Ok();
        }

        public static Result SaveTimbreTag(
            TimbreTagCatalog catalog,
            TimbreTagDefinition draft,
            string replaceId)
        {
            if (catalog == null)
                return Result.Fail(ResultCode.NotFound, "おんしょくタグが ないよ");

            var existing = new System.Collections.Generic.List<TimbreTagDefinition>();
            if (catalog.Tags != null)
            {
                for (int i = 0; i < catalog.Tags.Count; i++)
                    existing.Add(catalog.Tags[i]);
            }

            var validation = CuratedSoundValidation.ValidateTagUpsert(draft, existing, replaceId);
            if (!validation.IsSuccess)
                return validation;

            Undo.RecordObject(catalog, "Upsert Timbre Tag");
            if (!string.IsNullOrEmpty(replaceId))
            {
                for (int i = 0; i < existing.Count; i++)
                {
                    if (existing[i] != null && existing[i].id == replaceId)
                    {
                        existing[i] = draft;
                        catalog.SetTags(existing);
                        EditorUtility.SetDirty(catalog);
                        AssetDatabase.SaveAssets();
                        return Result.Ok();
                    }
                }
            }

            existing.Add(draft);
            catalog.SetTags(existing);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return Result.Ok();
        }

        public static Result RemoveTimbreTag(
            TimbreTagCatalog timbre,
            CuratedSoundCatalog sounds,
            string tagId)
        {
            if (timbre == null)
                return Result.Fail(ResultCode.NotFound, "おんしょくタグが ないよ");

            if (!CuratedSoundValidation.CanRemoveTag(tagId, timbre, sounds != null ? sounds.Items : null))
                return Result.Fail(ResultCode.ValidationError, "つかわれている タグは 消せないよ");

            var next = new System.Collections.Generic.List<TimbreTagDefinition>();
            if (timbre.Tags != null)
            {
                for (int i = 0; i < timbre.Tags.Count; i++)
                {
                    var t = timbre.Tags[i];
                    if (t != null && t.id != tagId) next.Add(t);
                }
            }

            Undo.RecordObject(timbre, "Remove Timbre Tag");
            timbre.SetTags(next);
            EditorUtility.SetDirty(timbre);
            AssetDatabase.SaveAssets();
            return Result.Ok();
        }
    }
}
