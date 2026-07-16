using System;

namespace Geidai.Common.Models
{
    /// <summary>
    /// ユーザープロフィール（BR-01/02）。
    /// birthYear は 1900〜現在年、nickname は 1〜8 文字。検証は ValidationUtil を用いる。
    /// JsonUtility でシリアライズ可能な素直な構造に限定する。
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public int birthYear;
        public string nickname;

        public UserProfile()
        {
            birthYear = 0;
            nickname = string.Empty;
        }

        public UserProfile(int birthYear, string nickname)
        {
            this.birthYear = birthYear;
            this.nickname = nickname ?? string.Empty;
        }
    }
}
