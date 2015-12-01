using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using strange.extensions.signal.impl;

namespace ScienceAlert.Gui
{
    public class SignalAppButtonCreated : Signal
    {
    }

    public class SignalAppButtonToggled : Signal<bool>
    {
        
    }

    public class SignalAlertPanelViewVisibilityChanged : Signal<bool>
    {
        
    }

    public class SignalLoadGuiSettings : Signal<ConfigNode>
    {
        
    }

    public class SignalSaveGuiSettings : Signal<ConfigNode>
    {
        
    }
}
