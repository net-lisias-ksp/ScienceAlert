using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface IOnboardSensor : ISensor
    {
        bool Value { get; }
    }
}
