using System;
using System.Linq;
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.ProfileData
{
    public class SensorSettings
    {
        public enum FilterMethod : int
        {
            Unresearched = 0,                           // only light on subjects for which no science has been confirmed at all
            NotMaxed = 1,                               // light whenever the experiment subject isn't maxed
            LessThanFiftyPercent = 2,                   // less than 50% researched
            LessThanNinetyPercent = 3                   // less than 90% researched
        }

        public event Callback OnChanged = delegate() { };

/******************************************************************************
*                    Implementation Details
******************************************************************************/


        private bool _enabled = true;
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    OnChanged();
                }
            }
        }



        private bool _soundOnDiscovery = true;
        public bool SoundOnDiscovery
        {
            get
            {
                return _soundOnDiscovery;
            }
            set
            {
                if (_soundOnDiscovery != value)
                {
                    _soundOnDiscovery = value;
                    OnChanged();
                }
            }
        }



        private bool _animationOnDiscovery = true;
        public bool AnimationOnDiscovery
        {
            get
            {
                return _animationOnDiscovery;
            }
            set
            {
                if (value != _animationOnDiscovery)
                {
                    _animationOnDiscovery = value;
                    OnChanged();
                }
            }
        }



        private bool _stopWarpOnDiscovery = false;
        public bool StopWarpOnDiscovery
        {
            get
            {
                return _stopWarpOnDiscovery;
            }
            set
            {
                if (value != _stopWarpOnDiscovery)
                {
                    _stopWarpOnDiscovery = value;
                    OnChanged();
                }
            }
        }



        private FilterMethod _filter = FilterMethod.Unresearched;
        public FilterMethod Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                if (value != _filter)
                {
                    _filter = value;
                    OnChanged();
                }
            }
        }


      


        public bool IsDefault = false;

        public SensorSettings() { }
        public SensorSettings(SensorSettings other)
        {
            Enabled = other.Enabled;
            SoundOnDiscovery = other.SoundOnDiscovery;
            AnimationOnDiscovery = other.AnimationOnDiscovery;
            StopWarpOnDiscovery = other.StopWarpOnDiscovery;
            Filter = other.Filter;
            IsDefault = other.IsDefault;
        }



        public void OnLoad(ConfigNode node)
        {
            Enabled = ConfigUtil.Parse<bool>(node, "Enabled", true);
            SoundOnDiscovery = ConfigUtil.Parse<bool>(node, "SoundOnDiscovery", true);
            StopWarpOnDiscovery = ConfigUtil.Parse<bool>(node, "StopWarpOnDiscovery", false);

            var strFilterName = node.GetValue("Filter");
            if (string.IsNullOrEmpty(strFilterName))
            {
                Log.Error("Settings: invalid experiment filter");
                strFilterName = Enum.GetValues(typeof(FilterMethod)).GetValue(0).ToString();
            }

            Filter = (FilterMethod)Enum.Parse(typeof(FilterMethod), strFilterName);
            IsDefault = ConfigUtil.Parse<bool>(node, "IsDefault", false);
        }



        public void OnSave(ConfigNode node)
        {
            node.AddValue("Enabled", Enabled);
            node.AddValue("SoundOnDiscovery", SoundOnDiscovery);
            node.AddValue("AnimationOnDiscovery", AnimationOnDiscovery);
            node.AddValue("StopWarpOnDiscovery", StopWarpOnDiscovery);
            node.AddValue("Filter", Filter);
            node.AddValue("IsDefault", IsDefault);
        }



        public override string ToString()
        {
            ConfigNode node = new ConfigNode();
            OnSave(node);
            return node.ToString();
        }
    }
}
