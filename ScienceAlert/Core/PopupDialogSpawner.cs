using strange.extensions.implicitBind;
using strange.extensions.injector.api;

namespace ScienceAlert.Core
{
    [Implements(typeof(IPopupDialogSpawner), InjectionBindingScope.CROSS_CONTEXT)]
// ReSharper disable once UnusedMember.Global
    public class PopupDialogSpawner : IPopupDialogSpawner
    {
        public void CriticalError(string details)
        {
            PopupDialog.SpawnPopupDialog("Critical Problem", details, "Okay", true, HighLogic.Skin);
        }
    }
}
