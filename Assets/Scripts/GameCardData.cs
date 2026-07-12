using UnityEngine;

[System.Serializable]
public class GameCardData
{
    public string title;
    public string category;
    public string difficulty;

    [Header("Game Settings")]
    public int questionCount;
    public int selectSoundCount;
    public string selectSoundType;

    [Header("Display")]
    public Sprite thumbnail;
    public string description;
}