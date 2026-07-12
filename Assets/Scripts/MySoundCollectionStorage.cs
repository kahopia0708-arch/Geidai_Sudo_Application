using System.IO;
using UnityEngine;

public static class MySoundCollectionStorage
{
    public static void SaveSoundWithSettings(AudioClip clip, SoundEffectSettings settings)
    {
        if (clip == null)
        {
            Debug.LogWarning("保存する AudioClip がありません。");
            return;
        }

        if (settings == null)
        {
            Debug.LogWarning("保存する加工設定がありません。");
            return;
        }

        SoundSavePaths.EnsureCollectionDirectoryExists();

        string baseName = Path.GetFileNameWithoutExtension(SoundSavePaths.CreateNewSoundFilePath());

        string wavFileName = baseName + ".wav";
        string jsonFileName = baseName + ".json";

        string wavPath = Path.Combine(SoundSavePaths.CollectionDirectory, wavFileName);
        string jsonPath = Path.Combine(SoundSavePaths.CollectionDirectory, jsonFileName);

        settings.displayName = baseName;
        settings.wavFileName = wavFileName;

        WavUtility.Save(wavPath, clip);

        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(jsonPath, json);

        Debug.Log("音声と加工設定を保存しました: " + baseName);
    }

    public static SoundEffectSettings LoadSettings(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogWarning("設定ファイルが見つかりません: " + jsonPath);
            return null;
        }

        string json = File.ReadAllText(jsonPath);
        return JsonUtility.FromJson<SoundEffectSettings>(json);
    }

    public static string[] GetSavedSettingFiles()
    {
        SoundSavePaths.EnsureCollectionDirectoryExists();

        return Directory.GetFiles(SoundSavePaths.CollectionDirectory, "*.json");
    }

    public static AudioClip LoadClipFromSettings(SoundEffectSettings settings)
    {
        if (settings == null)
        {
            Debug.LogWarning("音声設定がありません。");
            return null;
        }

        if (string.IsNullOrEmpty(settings.wavFileName))
        {
            Debug.LogWarning("音声ファイル名が設定されていません。");
            return null;
        }

        string wavPath = Path.Combine(SoundSavePaths.CollectionDirectory, settings.wavFileName);

        return WavUtility.Load(wavPath);
    }

    public static string GetWavPathFromSettings(SoundEffectSettings settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.wavFileName))
        {
            return string.Empty;
        }

        return Path.Combine(SoundSavePaths.CollectionDirectory, settings.wavFileName);
    }
}