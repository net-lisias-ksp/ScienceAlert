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


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalUpdateExperimentListPopup : Signal<ExperimentSensorState, ExperimentListView.PopupType, Vector2>
    {
        
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalExperimentSensorStatusChanged : Signal<ExperimentSensorState>
    {
    }
}
