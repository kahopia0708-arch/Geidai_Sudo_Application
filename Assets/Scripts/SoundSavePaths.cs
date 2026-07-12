using System;
using System.IO;
using UnityEngine;

public static class SoundSavePaths
{
    private const string CollectionFolderName = "MySoundCollection";
    private const string SoundFileExtension = ".wav";

    public static string CollectionDirectory
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, CollectionFolderName);
        }
    }

    public static void EnsureCollectionDirectoryExists()
    {
        if (!Directory.Exists(CollectionDirectory))
        {
            Directory.CreateDirectory(CollectionDirectory);
            Debug.Log("音声保存フォルダを作成しました: " + CollectionDirectory);
        }
    }

    public static string CreateNewSoundFilePath()
    {
        EnsureCollectionDirectoryExists();

        string fileName = "sound_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + SoundFileExtension;
        return Path.Combine(CollectionDirectory, fileName);
    }

    public static string[] GetSavedSoundFiles()
    {
        EnsureCollectionDirectoryExists();

        return Directory.GetFiles(CollectionDirectory, "*" + SoundFileExtension);
    }

    public static bool HasSavedSounds()
    {
        string[] files = GetSavedSoundFiles();
        return files.Length > 0;
    }

    public static string GetFileNameWithoutExtension(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }
}