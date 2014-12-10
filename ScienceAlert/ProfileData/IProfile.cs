﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.ProfileData
{
    interface IProfile
    {
        string Name { get; }
        string DisplayName { get; }
        float ScienceThreshold { get; }
        bool Modified { get; }
    

        IProfile Clone();
        SensorSettings GetSensorSettings(string expid);
        void SetSensorSettings(string expid);
        void OnSave(ConfigNode node);
        void OnLoad(ConfigNode node);
    }
}