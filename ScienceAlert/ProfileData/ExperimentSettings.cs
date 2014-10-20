/******************************************************************************
                   Science Alert for Kerbal Space Program                    
 ******************************************************************************
    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/
using System;
using System.Linq;
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.ProfileData
{
    // per-experiment settings
    public class ExperimentSettings
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


        
        // Those I've looked at seem to define the biome and
        // situation masks in ScienceDefs correctly, so although
        // we won't be able to interact directly with the experiment,
        // we should at least be able to tell when it could run under
        // our desired filter
        //private bool _assumeOnboard = false;
        //public bool AssumeOnboard
        //{
        //    get
        //    {
        //        return _assumeOnboard;
        //    }
        //    set
        //    {
        //        if (value != _assumeOnboard)
        //        {
        //            _assumeOnboard = value;
        //            OnChanged();
        //        }
        //    }
        //}



        public bool IsDefault = false;

        public ExperimentSettings() { }
        public ExperimentSettings(ExperimentSettings other)
        {
            Enabled = other.Enabled;
            SoundOnDiscovery = other.SoundOnDiscovery;
            AnimationOnDiscovery = other.AnimationOnDiscovery;
            StopWarpOnDiscovery = other.StopWarpOnDiscovery;
            Filter = other.Filter;
            //AssumeOnboard = other.AssumeOnboard;
            IsDefault = other.IsDefault;
        }



        public void OnLoad(ConfigNode node)
        {
            Enabled = ConfigUtil.Parse<bool>(node, "Enabled", true);
            SoundOnDiscovery = ConfigUtil.Parse<bool>(node, "SoundOnDiscovery", true);
            AnimationOnDiscovery = ConfigUtil.Parse<bool>(node, "AnimationOnDiscovery", true);
            //AssumeOnboard = ConfigUtil.Parse<bool>(node, "AssumeOnboard", false);
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
            //node.AddValue("AssumeOnboard", AssumeOnboard);
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
