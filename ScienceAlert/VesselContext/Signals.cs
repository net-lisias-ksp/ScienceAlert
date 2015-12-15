﻿using ScienceAlert.VesselContext.Experiments;
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


    public class SignalSensorValueChanged : Signal<ScienceExperiment, ISensorValues>
    {
        
    }
}
