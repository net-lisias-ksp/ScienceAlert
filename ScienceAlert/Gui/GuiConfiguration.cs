using ReeperCommon.Serialization;
using UnityEngine;

namespace ScienceAlert.Gui
{
    public class GuiConfiguration : IGuiConfiguration
    {
        [ReeperPersistent] public float ButtonFramerate = 15f;

        public float Framerate
        {
            get { return ButtonFramerate; }
            set { ButtonFramerate = Mathf.Clamp(value, 1f, 60f); }
        }
    }
}
