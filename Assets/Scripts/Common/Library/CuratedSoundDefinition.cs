using System;
using UnityEngine;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 制作側同梱音の1件定義（U7 / FR-20 / domain-entities）。
    /// id・displayName・clip が揃わない定義は一覧から除外し得る（BR-LIB-01）。
    /// </summary>
    [Serializable]
    public class CuratedSoundDefinition
    {
        public string id;
        public string displayName;
        public string category;
        [TextArea] public string description;
        public AudioClip clipRef;
        public bool initiallyUnlocked;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id)
            && !string.IsNullOrWhiteSpace(displayName)
            && clipRef != null;
    }
}
