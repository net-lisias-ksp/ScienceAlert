using UnityEngine;

namespace ScienceAlert
{
    /// <summary>
    /// I'm sure there's a better name for this, but essentially this abstracts
    /// away the exact mechanism that determines whether a location (lat/lon-wise)
    /// can be checked for an experiment. 
    /// </summary>
    public class ScanInterface : MonoBehaviour
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

    public class DefaultScanInterface : ScanInterface
    {
    }
}
