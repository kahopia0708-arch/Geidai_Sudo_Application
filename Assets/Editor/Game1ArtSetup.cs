using Geidai.Common.Game;
using Geidai.Common.Library;
using Geidai.Game1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.EditorTools
{
    /// <summary>
    /// ①音合わせの暫定スプライト割当と SoundMatchConfig の図鑑参照を整える。
    /// </summary>
    public static class Game1ArtSetup
    {
        private const string ConfigPath = "Assets/Settings/SoundMatchConfig.asset";
        private const string CatalogPath = "Assets/Settings/CuratedSoundCatalog_Default.asset";
        private const string FrogPath = "Assets/Art/Game/Game1_Placeholder_Frog.png";
        private const string TadpolePath = "Assets/Art/Game/Game1_Placeholder_Tadpole.png";
        private const string FrogOpenPath = "Assets/Art/Game/Game1_Placeholder_Frog_Open.png";
        private const string TadpoleOpenPath = "Assets/Art/Game/Game1_Placeholder_Tadpole_Open.png";
        private const string ScenePath = "Assets/Scenes/Geidai/GeidaiGame1.unity";

        [MenuItem("Geidai/Game1/Apply Placeholder Art And Catalog")]
        public static void ApplyAll()
        {
            ApplyConfig();
            ApplyScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[Geidai] Game1: placeholder art and catalog wired.");
        }

        private static void ApplyConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<SoundMatchConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogError($"[Geidai] Missing {ConfigPath}");
                return;
            }

            var catalog = AssetDatabase.LoadAssetAtPath<CuratedSoundCatalog>(CatalogPath);
            var frog = LoadSprite(FrogPath);
            var tadpole = LoadSprite(TadpolePath);
            var frogOpen = LoadSprite(FrogOpenPath);
            var tadpoleOpen = LoadSprite(TadpoleOpenPath);

            var so = new SerializedObject(config);
            so.FindProperty("curatedCatalog").objectReferenceValue = catalog;
            so.FindProperty("placeholderTargetSprite").objectReferenceValue = frog;
            so.FindProperty("placeholderChoiceSprite").objectReferenceValue = tadpole;
            so.FindProperty("placeholderTargetOpenSprite").objectReferenceValue = frogOpen;
            so.FindProperty("placeholderChoiceOpenSprite").objectReferenceValue = tadpoleOpen;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void ApplyScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var frogSprite = LoadSprite(FrogPath);
            var tadpoleSprite = LoadSprite(TadpolePath);

            var frogViews = Object.FindObjectsByType<FrogTargetView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var frog in frogViews)
            {
                var img = frog.GetComponent<Image>();
                if (img != null && frogSprite != null)
                {
                    img.sprite = frogSprite;
                    Game1CharacterImage.Apply(img);
                }

                var so = new SerializedObject(frog);
                so.FindProperty("targetImage").objectReferenceValue = img;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var choices = Object.FindObjectsByType<ChoiceItemView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var choice in choices)
            {
                var img = choice.GetComponent<Image>();
                if (img != null && tadpoleSprite != null)
                {
                    img.sprite = tadpoleSprite;
                    Game1CharacterImage.Apply(img);
                }

                var so = new SerializedObject(choice);
                so.FindProperty("choiceImage").objectReferenceValue = img;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var effects = Object.FindObjectsByType<ResultEffectController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var fx in effects)
            {
                var so = new SerializedObject(fx);
                var frogImageProp = so.FindProperty("frogImage");
                if (frogImageProp.objectReferenceValue == null)
                {
                    foreach (var frog in frogViews)
                    {
                        var img = frog.GetComponent<Image>();
                        if (img != null)
                        {
                            frogImageProp.objectReferenceValue = img;
                            break;
                        }
                    }
                }

                var stages = so.FindProperty("growthStages");
                if (stages.arraySize == 0 && frogSprite != null && tadpoleSprite != null)
                {
                    stages.arraySize = 2;
                    stages.GetArrayElementAtIndex(0).objectReferenceValue = tadpoleSprite;
                    stages.GetArrayElementAtIndex(1).objectReferenceValue = frogSprite;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Sprite LoadSprite(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
    }
}
