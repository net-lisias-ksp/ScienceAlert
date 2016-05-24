using strange.extensions.signal.impl;
using ScienceAlert.UI;
using ScienceAlert.UI.ExperimentWindow;

namespace ScienceAlert.VesselContext.Gui
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class SignalLoadGuiSettings : Signal
    {

    }


    // ReSharper disable once ClassNeverInstantiated.Global
    class SignalSaveGuiSettings : Signal
    {

    }


    class SignalSetTooltip : Signal<IExperimentIdentifier, ExperimentWindowView.ExperimentIndicatorTooltipType>
    {
    }
}
