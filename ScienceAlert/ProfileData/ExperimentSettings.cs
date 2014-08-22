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
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        public bool Enabled = true;
        public bool SoundOnDiscovery = true;
        public bool AnimationOnDiscovery = true;
        public bool StopWarpOnDiscovery = false;

        public FilterMethod Filter = FilterMethod.Unresearched;

        public bool AssumeOnboard = false;      // part of a workaround I'm thinking of for
        // modded "experiments" that don't actually
        // inherit from ModuleScienceExperiment
        //
        // Those I've looked at seem to define the biome and
        // situation masks in ScienceDefs correctly, so although
        // we won't be able to interact directly with the experiment,
        // we should at least be able to tell when it could run under
        // our desired filter
        public bool IsDefault = false;

        public ExperimentSettings() { }
        public ExperimentSettings(ExperimentSettings other)
        {
            Enabled = other.Enabled;
            SoundOnDiscovery = other.SoundOnDiscovery;
            AnimationOnDiscovery = other.AnimationOnDiscovery;
            StopWarpOnDiscovery = other.StopWarpOnDiscovery;
            Filter = other.Filter;
            AssumeOnboard = other.AssumeOnboard;
            IsDefault = other.IsDefault;
        }



        public void OnLoad(ConfigNode node)
        {
            Enabled = ConfigUtil.Parse<bool>(node, "Enabled", true);
            SoundOnDiscovery = ConfigUtil.Parse<bool>(node, "SoundOnDiscovery", true);
            AnimationOnDiscovery = ConfigUtil.Parse<bool>(node, "AnimationOnDiscovery", true);
            AssumeOnboard = ConfigUtil.Parse<bool>(node, "AssumeOnboard", false);
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
            node.AddValue("AssumeOnboard", AssumeOnboard);
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
