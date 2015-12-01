using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Experiments
{
    //[Flags]
    //public enum SensorStatus
    //{
    //    None = 1 << 0,

    //    AlertCollect = 1 << 1,          // the "usual" ScienceAlert alert -- has value for collection
    //    AlertTransmit = 1 << 2,         // Current result is worth transmission points
    //    AlertLabAnalysis = 1 << 3,      // lab data could be created out of a report
    //    Runnable = 1 << 3,

    //}

    public interface IExperimentSensor
    {
        void Update();

        float CollectionValue { get; }
        float TransmissionValue { get; }
        float LabDataValue { get; }

        float TransmissionMultiplier { get; set; }
    }
}
