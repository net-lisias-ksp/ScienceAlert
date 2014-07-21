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
using UnityEngine;

namespace ScienceAlert
{
    /// <summary>
    /// I'm sure there's a better name for this, but essentially this abstracts
    /// away the exact mechanism that determines whether a location (lat/lon-wise)
    /// can be checked for an experiment. 
    /// </summary>
    internal class ScanInterface : MonoBehaviour
    {
        /// <summary>
        /// Returns true if the projected location on the biome map has been 
        /// revealed.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public virtual bool HaveScanData(double lat, double lon, CelestialBody body) { return true; }
    }

    internal class DefaultScanInterface : ScanInterface
    {
    }
}
