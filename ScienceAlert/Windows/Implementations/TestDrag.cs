using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Windows.Implementations
{
    class TestDrag : ReeperCommon.Window.DraggableWindow
    {

        protected override Rect Setup()
        {
            Log.Write("TestDrag.Setup");

            //backstop.SetZ(MessageSystem.Instance.hoverComponent.transform.position.z);

            return new Rect(300, 300, 300, 300);
        }

        protected override void DrawUI()
        {
            //Log.Write("TestDrag.DrawUI");
            GUILayout.BeginVertical();
            GUILayout.Label("TestDrag.DrawUI");
            GUILayout.EndVertical();
        }

        protected override void OnCloseClick()
        {
            Log.Warning("Window close attempt");
        }
    }
}
