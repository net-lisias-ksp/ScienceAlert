using ScienceAlert.VesselContext.Gui;
using strange.extensions.signal.impl;

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


    public class SignalSpawnExperimentReportPopup : Signal<ExperimentStatusReport, ExperimentView.PopupType>
    {
        
    }


    public class SignalDestroyExperimentReportPopup : Signal
    {
        
    }

    public class SignalExperimentValueChanged : Signal<ScienceExperiment>
    {
        
    }
}
