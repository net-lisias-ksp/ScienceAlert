using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.ProfileData
{
    public interface IProfile
    {
        string Name { get; set; }
        string DisplayName { get; }
        float ScienceThreshold { get; set; }
        bool Modified { get; set; }
    

        IProfile Clone();
        SensorSettings GetSensorSettings(string expid);
        void SetSensorSettings(string expid, SensorSettings settings);
        void OnSave(ConfigNode node);
        void OnLoad(ConfigNode node);
    }
}
