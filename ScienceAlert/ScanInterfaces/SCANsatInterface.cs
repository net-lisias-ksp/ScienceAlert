//#define DEBUGSCANSAT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ReeperCommon;


namespace ScienceAlert
{
    
    internal class SCANsatInterface : ScanInterface
    {
        private const string SCANutilTypeName = "SCANsat.SCANUtil";
        delegate bool IsCoveredDelegate(double lat, double lon, CelestialBody body, int mask);

        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------
#if DEBUG
        private static IsCoveredDelegate _isCovered;
#else
        private static IsCoveredDelegate _isCovered = delegate(double lat, double lon, CelestialBody body, int mask) { return true; };
#endif
        private static MethodInfo _method;
        private static bool _ran = false;

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/
        void OnDestroy()
        {
            _ran = false;
        }



        /// <summary>
        /// Returns true if SCANsat has biome data for the given latitude,
        /// longitude and body
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public override bool HaveScanData(double lat, double lon, CelestialBody body)
        {
#if DEBUG
            bool covered = _isCovered(lat, lon, body, 8 /* biome */);
            //Log.Write("SCANsat.HaveScanData: {0}", covered ? "Yes" : "No data");
            return covered;
#else
            return _isCovered(lat, lon, body, 8 /* biome */);
#endif
        }



        /// <summary>
        /// Checks to ensure the SCANsat interface is available
        /// </summary>
        /// <returns></returns>
        public static bool IsAvailable()
        {
            if (_method != null && _isCovered != null) return true;
            if (_ran)
            {
                //Log.Debug("SCANsatInterface.IsAvailable already ran; not available.");
                return false;
            }

            Log.Verbose("Performing SCANsat check...");
            _ran = true;

            try
            {
                Type utilType = AssemblyLoader.loadedAssemblies
                    .SelectMany(loaded => loaded.assembly.GetExportedTypes())
                    .SingleOrDefault(t => t.FullName == SCANutilTypeName);
#if DEBUG
                AssemblyLoader.loadedAssemblies.ToList().ForEach(a =>
                {
                    if (a.name == "SCANsat")
                    {
                        Log.Debug("Assembly {0}:", a.assembly.FullName);

                        a.assembly.GetExportedTypes().ToList().ForEach(t =>
                        {
                            Log.Debug("   exports: {0}", t.FullName);
                        });
                    }
                    else Log.Debug("Assembly [brief]: {0}", a.name);
                });
#endif

                if (utilType == null)
                {
                    Log.Warning("SCANsatInterface.IsAvailable: Failed to find SCANsat.SCANutil type. SCANsat interface will not be available.");
                    return false;
                }

                _method = utilType.GetMethod("isCovered", new Type[] { typeof(double), typeof(double), typeof(CelestialBody), typeof(int) });

                if (_method == null)
                {
                    Log.Error("SCANsatInterface: Failed to locate SCANutil.isCovered!");
                    return false;
                }
                else
                { 
                    _isCovered = (IsCoveredDelegate)Delegate.CreateDelegate(typeof(IsCoveredDelegate), _method);

                    if (_isCovered == null)
                    {
                        Log.Error("SCANsatInterface: Failed to create method delegate");
                    }
                    else Log.Normal("SCANsatInterface: Interface available");

                    return _isCovered != null;
                }
            } catch (Exception e)
            {
                Log.Error("Exception in SCANsatInterface.IsAvailable: {0}", e);
            }

            return false;
        }
    }
}
