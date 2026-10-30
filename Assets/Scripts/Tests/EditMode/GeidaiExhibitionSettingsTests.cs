using Geidai.Common.Config;
using Geidai.Common.Models;
using Geidai.Foundation;
using NUnit.Framework;
using UnityEngine;

namespace Geidai.Tests.EditMode
{
    public class GeidaiExhibitionSettingsTests
    {
        [Test]
        public void IsSceneBlocked_WhenInactive_ReturnsFalse()
        {
            var settings = ScriptableObject.CreateInstance<GeidaiExhibitionSettings>();
            Assert.IsFalse(settings.IsSceneBlocked(SceneId.Rec));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void ExhibitionHomeMenu_HasTwoVisibleItems()
        {
            var menu = Resources.Load<HomeMenuConfig>("Geidai/HomeMenuConfig_Exhibition");
            if (menu == null)
            {
                Assert.Ignore("HomeMenuConfig_Exhibition not in Resources.");
                return;
            }

            var visible = menu.VisibleSorted();
            Assert.AreEqual(2, visible.Count);
            Assert.AreEqual(ModuleId.GameSelect, visible[0].moduleId);
            Assert.AreEqual(ModuleId.Library, visible[1].moduleId);
        }
    }
}
