using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert
{
    /// <summary>
    /// Filtering at the edges of the original biome maps can cause some
    /// inaccurate biome reporting. That's not good, because it'll trick
    /// experiment observers into believing they entered a new subjectid
    /// but only for a split second if the filtered color just happens
    /// to match any of the biomes.
    /// 
    /// This class will generate a "clean" version with no aliasing. If
    /// the stock method result doesn't match any on the "clean" version
    /// within a particular search radius, it's suspicious and the last 
    /// good result should be used instead.
    /// </summary>
    public class BiomeFilter
    {
        private const int HALF_SEARCH_DIMENSIONS = 2;    // box around the point on the biome map to
                                                         // use to verify biome map results


        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------

        private CelestialBody current;                      // which CelestialBody we've got a cached biome map texture for
        private Texture2D projectedMap;                     // this is the cleaned biome map of the current CelestialBody
        private System.Collections.IEnumerator projector;   // this coroutine constructs the projectedMap from current CelestialBody
        private const float COLOR_THRESHOLD = 0.005f;       // Maximum color difference for two colors to be considered the same
        private string currentBiome = "";                   // current biome, updated every frame



        /******************************************************************************
         *                    Implementation Details
         ******************************************************************************/


        public BiomeFilter()
        {
            ReprojectBiomeMap(FlightGlobals.currentMainBody);
        }



        public void UpdateBiomeData()
        {
            if (projector != null)
                projector.MoveNext();

            if (FlightGlobals.ActiveVessel == null) return;

            string possibleBiome = "";

            if (GetCurrentBiome(out possibleBiome))
                CurrentBiome = possibleBiome;
            // else use existing, last-known-good value by not overwriting it with the garbage we just received
        }



        /// <summary>
        /// Returns the biome of the body the active vessel is orbiting, located under its current
        /// latitude and longitude. If the result may be incorrect (based on filter), the method will 
        /// return false
        /// </summary>
        /// <param name="biome"></param>
        /// <returns></returns>
        public bool GetCurrentBiome(out string biome)
        {
            biome = string.Empty;
            Vessel vessel = FlightGlobals.ActiveVessel;
            double lat = vessel.latitude * Mathf.Deg2Rad;
            double lon = vessel.longitude * Mathf.Deg2Rad;

            if (vessel == null || vessel.mainBody.BiomeMap == null || vessel.mainBody.BiomeMap.Map == null)
                return true;

            // vessel.landedAt gets priority since there are some special
            // factors it will take into account for us; specifically, that
            // the vessel is on the launchpad, ksc, etc which are treated
            // as biomes even though they don't exist on the biome map.
            if (!string.IsNullOrEmpty(vessel.landedAt))
            {
                biome = Vessel.GetLandedAtString(vessel.landedAt);
                return true;
            }
            else
            {
                // we don't use ScienceUtil version here because we want to know the biome's attribute map color
                var possibleBiome = vessel.mainBody.BiomeMap.GetAtt(lat, lon);

                // if the cleaned map is finished, let's test the validity
                // of the result by checking some surrounding areas
                if (!IsBusy)
                {
                    if (VerifyBiomeResult(lat, lon, possibleBiome))
                    {
                        // looks good!
                        biome = possibleBiome.name;
                        return true;
                    }
                    else
                    {
#if DEBUG
                        Log.Error("Biome '{0}' is probably an error", possibleBiome.name);
#endif
                        
                        return false;
                    }
                }
                else
                {
                    // not finished building the cleaned map so we'll
                    // just use what we have
                    biome = possibleBiome.name;
                    return true;
                }
            }
        }



        /// <summary>
        /// It basically does what it says
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private bool Similar(Color first, Color second)
        {
            return Mathf.Abs(first.r - second.r) < COLOR_THRESHOLD && Mathf.Abs(first.g - second.g) < COLOR_THRESHOLD && Mathf.Abs(first.b - second.b) < COLOR_THRESHOLD;
        }



        /// <summary>
        /// Takes the result of the stock biome method and compares it against the cleaned version.
        /// If the cleaned version has the same biome on the same point or within a small search
        /// area, returns true indicating that the stock result is most likely correct.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool VerifyBiomeResult(double lat, double lon, CBAttributeMap.MapAttribute target)
        {
            if (projectedMap == null)
            {
                Log.Debug("Cannot verify biome result; no projected map exists");
                return true; // we'll have to assume it's accurate since we can't prove otherwise
            }

            if (target == null || target.mapColor == null)
                return true; // this shouldn't happen 
            
            // longitude to correct domain
            lon -= Mathf.PI * 0.5f;
            if (lon < 0d) lon += Mathf.PI * 2d;
            lon %= Mathf.PI * 2d;

            // simple math to convert lat/lon coordinates into rectangular xy
            int x_center = (int)Math.Round(projectedMap.width * (float)(lon / (Mathf.PI * 2)), 0);
            int y_center = (int)Math.Round(projectedMap.height * ((float)(lat / Mathf.PI) + 0.5f), 0);

            for (int y = y_center - HALF_SEARCH_DIMENSIONS; y < y_center + HALF_SEARCH_DIMENSIONS; ++y)
            for (int x = x_center - HALF_SEARCH_DIMENSIONS; x < x_center + HALF_SEARCH_DIMENSIONS; ++x)
            {
                Color c = projectedMap.GetPixel(x, y);

#if DEBUG
                //Log.Write("Projected is {0}, comparing to {1}, comparison is {2}", c, target.mapColor, Similar(c, target.mapColor));
#endif
                if (Similar(c, target.mapColor))
                    return true; // we have a match, no need to look further
            }

            return false;
        }



        /// <summary>
        /// Exists solely to kick off the ReprojectMap function and
        /// overwrite any old version, if one happens to be in progress
        /// at the time.
        /// </summary>
        /// <param name="newBody"></param>
        private void ReprojectBiomeMap(CelestialBody newBody)
        {
            projector = ReprojectMap(newBody);
        }



        /// <summary>
        /// This function will run in short segments every Update() loop until
        /// it finishes the new projectedMap texture. Until then, the
        /// BiomeFilter should use the "normal" fallback method of querying
        /// the original biome map in the meantime.
        /// </summary>
        /// <param name="newBody"></param>
        /// <returns></returns>
        private System.Collections.IEnumerator ReprojectMap(CelestialBody newBody)
        {
            if (current == newBody)
            {
                projector = null;
                yield break;
            }

            if (newBody == null)
            {
                Log.Error("BiomeFilter.ReprojectMap failure: newBody is null!");
                projector = null;
                current = null;
                yield break;
            }

            Log.Debug("ScienceAlert.BiomeFilter: Reprojecting biome map for {0}", newBody.name);

#if DEBUG
            if (newBody != FlightGlobals.currentMainBody)
                Log.Error("Error: BiomeFilter.ReprojectBiomeMap was given wrong target! Given {0}, it should be {1}", newBody.name, FlightGlobals.currentMainBody.name);
#endif
            current = null;

            if (newBody.BiomeMap == null || newBody.BiomeMap.Map == null)
            {
                projectedMap = null;
                projector = null;
                yield break;
            }

            Texture2D projection = new Texture2D(newBody.BiomeMap.Map.width, newBody.BiomeMap.Map.height, TextureFormat.ARGB32, false);
            projection.filterMode = FilterMode.Point;


            yield return null;

            float timer = Time.realtimeSinceStartup;
            Color32[] pixels = projection.GetPixels32();

            for (int y = 0; y < projection.height; ++y)
            {
                for (int x = 0; x < projection.width; ++x)
                {
                    // convert x and y into uv coordinates
                    float u = (float)x / projection.width;
                    float v = (float)y / projection.height;

                    // convert uv coordinates into latitude and longitude
                    double lat = Math.PI * v - Math.PI * 0.5;
                    double lon = 2d * Math.PI * u + Math.PI * 0.5;

                    // set biome color in our clean texture
                    pixels[y * projection.width + x] = (Color32)newBody.BiomeMap.GetAtt(lat, lon).mapColor;
                }

                if (y % 5 == 0)
                {
                    Log.Performance("ScienceAlert.BiomeFilter: still working ({0} complete)", ((y * projection.width) / (float)pixels.Length).ToString("P"));
                    yield return null;
                }    
            }

            projection.SetPixels32(pixels);
            projection.Apply();

            Log.Debug("Finished projection; storing data");
#if DEBUG
            projection.SaveToDisk(string.Format("ScienceAlert/projected_{0}.png", newBody.name));

            var original = new Texture2D(newBody.BiomeMap.Map.width, newBody.BiomeMap.Map.height, TextureFormat.ARGB32, false);
            var origPixels = newBody.BiomeMap.Map.GetPixels32();
            original.SetPixels32(origPixels);
            original.Apply();

            original.SaveToDisk(string.Format("ScienceAlert/original_{0}.png", newBody.name));
#endif   
            Log.Verbose("Filtering biome map of {0} took {1} seconds", newBody.name, Time.realtimeSinceStartup - timer);

            current = newBody;
            projectedMap = projection;
            projector = null; // we're finished!
        }



        /// <summary>
        /// When the player enters a new orbit, this function will be invoked
        /// to generate an aliasing/filtering-free biome map texture to avoid
        /// inaccurate results.
        /// </summary>
        /// <param name="bodies"></param>
        public void OnDominantBodyChanged(GameEvents.FromToAction<CelestialBody, CelestialBody> bodies)
        {
            Log.Normal("BiomeFilter.OnDominantBodyChanged from {0} to {1}", bodies.from, bodies.to);
            ReprojectBiomeMap(bodies.to);
        }


        public void OnVesselChanged(Vessel v)
        {
            ReprojectBiomeMap(v.mainBody);
        }


        #region properties

        /// <summary>
        /// Returns true if the biome filter is currently projecting a clean version of the dominant
        /// body's attribute map
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return projector != null;
            }
        }


        
        /// <summary>
        /// Current vessel biome. Correctly accounts for landedAt string (LaunchPad, KSC, etc)
        /// </summary>
        public string CurrentBiome
        {
            get { return currentBiome; }
            private set
            {
                currentBiome = value;
            }
        }

        #endregion
    }
}
