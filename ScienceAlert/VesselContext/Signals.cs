using ScienceAlert.VesselContext.Gui;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext
{

// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalLoadGuiSettings : Signal
    {

    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalSaveGuiSettings : Signal
    {
        
    }


    public class SignalUpdateExperimentListPopup : Signal<ExperimentStatusReport, ExperimentListView.PopupType, Vector2>
    {
        
    }
}
