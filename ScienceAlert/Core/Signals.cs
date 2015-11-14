using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using strange.extensions.signal.impl;

namespace ScienceAlert.Core
{
    public class SignalStart : Signal
    {
    }

    public class SignalScenarioModuleSave : Signal<ConfigNode>
    {
        
    }

    public class SignalScenarioModuleLoad : Signal<ConfigNode>
    {
        
    }
}
