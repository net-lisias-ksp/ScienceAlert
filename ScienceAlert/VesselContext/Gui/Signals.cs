using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
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
    public class SignalUpdateExperimentListEntryPopup : Signal<ExperimentListEntry, ExperimentPopupType, Vector2>
    {

    }


    // ReSharper disable once ClassNeverInstantiated.Global
    public class SignalCloseExperimentListEntryPopup : Signal
    {
        
    }
}
