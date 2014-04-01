using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DebugTools;
#if DEBUG
using ResourceTools;
#endif

namespace ScienceAlert
{
    /// <summary>
    /// Filtering at the edges of the original biome maps can cause some
    /// inaccurate biome reporting. That's not good, because it'll trick
    /// experiment observers into believing they entered a new situation
    /// but only for a split second.
    /// 
    /// To solve the problem, we'll work it backwards: query the original
    /// map by converting from texture coordinates into lat and lon, and
    /// refer to this stored texture whenever possible to identify biomes
    /// </summary>
    internal class BiomeFilter : MonoBehaviour
    {
        private CelestialBody current;      // which CelestialBody we've got a cached biome map texture for
        private Texture2D projectedMap;     // this is the cleaned biome map of the current CelestialBody
        private System.Collections.IEnumerator projector;   // this coroutine constructs the projectedMap from current CelestialBody

        internal BiomeFilter()
        {
            GameEvents.onDominantBodyChange.Add(OnDominantBodyChanged);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            ReprojectBiomeMap(FlightGlobals.currentMainBody);
        }

        ~BiomeFilter()
        {
            GameEvents.onVesselChange.Remove(OnVesselChanged);
            GameEvents.onDominantBodyChange.Remove(OnDominantBodyChanged);
        }


        public void Update()
        {
            if (projector != null)
                projector.MoveNext();
        }


        /// <summary>
        /// A little helper to determine biome.  It's not a straight biome
        /// map check: KSC, Launchpad and the runway are considered to be
        /// biomes when landed on yet have no entry in the biome map.
        /// Vessel.landedAt seems to be updated correctly when it's in
        /// these locations so we'll rely on that when it has a value.
        /// 
        /// This version will use a clean reverse-projected map of
        /// biomes to avoid inaccuracies if it's available. In the meantime,
        /// it will query the BiomeMap of the active vessel's main body
        /// directly.
        /// </summary>
        /// <param name="latRad"></param>
        /// <param name="lonRad"></param>
        /// <returns></returns>
        public string GetBiome(double latRad, double lonRad)
        {
            var vessel = FlightGlobals.ActiveVessel;

            if (!IsBusy && string.IsNullOrEmpty(vessel.landedAt))
            {
#if DEBUG
                if (FlightGlobals.ActiveVessel.mainBody != current)
                    Log.Error("BiomeFilter.GetBome body mismatch: active {0} does not match cached {1}!", FlightGlobals.currentMainBody, current);
#endif

                // projected map is complete; let's use that instead of the
                // CB's original biome map
                //
                // note: this is a simple equirectangular projection,
                // although for some reason, KSP offsets its biome 
                // maps by 90 degrees

                lonRad -= Mathf.PI * 0.5f; 
                if (lonRad < 0d) lonRad += Mathf.PI * 2d;
                lonRad %= Mathf.PI * 2d;

                float u = (float)(lonRad / (Mathf.PI * 2));
			    float v = (float)(latRad / Mathf.PI) + 0.5f;

                Color c = projectedMap.GetPixel((int)(u * projectedMap.width), (int)(v * projectedMap.height));
                CBAttributeMap.MapAttribute attr = current.BiomeMap.defaultAttribute;

                if (attr.mapColor != c)
                    attr = current.BiomeMap.Attributes.SingleOrDefault(mapAttr => mapAttr.mapColor == c);

                if (attr == null)
                {
                    Log.Error("Did not find a match for color {0} on body {1}", c.ToString(), current.name);
                }
                else
                {
                    return attr.name;
                }
            }
            return string.IsNullOrEmpty(vessel.landedAt) ? vessel.mainBody.BiomeMap.GetAtt(latRad, lonRad).name : vessel.landedAt;
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

            Log.Debug("ScienceAlert.BiomeFilter: Reprojecting biome map for {0}", newBody.name);

#if DEBUG
            if (newBody != FlightGlobals.currentMainBody)
                Log.Error("Error: BiomeFilter.ReprojectBiomeMap was given wrong target! Given {0}, it should be {1}", newBody.name, FlightGlobals.currentMainBody.name);
#endif
            current = newBody;
            projectedMap = new Texture2D(current.BiomeMap.Map.width, current.BiomeMap.Map.height, TextureFormat.ARGB32, false);
            projectedMap.filterMode = FilterMode.Point;

            yield return null;

            float timer = Time.realtimeSinceStartup;
            Color32[] pixels = projectedMap.GetPixels32();

            for (int y = 0; y < projectedMap.height; ++y)
            {
                for (int x = 0; x < projectedMap.width; ++x)
                {
                    // convert x and y into uv coordinates
                    float u = (float)x / projectedMap.width;
                    float v = (float)y / projectedMap.height;

                    // convert uv coordinates into latitude and longitude
                    double lat = Math.PI * v - Math.PI * 0.5;
                    double lon = 2d * Math.PI * u + Math.PI * 0.5;

                    // set biome color in our clean texture
                    pixels[y * projectedMap.width + x] = (Color32)current.BiomeMap.GetAtt(lat, lon).mapColor;
                }

                Log.Debug("ScienceAlert.BiomeFilter: still working");

                if (y % 5 == 0)
                    yield return null;
            }

            projectedMap.SetPixels32(pixels);
            projectedMap.Apply();
#if DEBUG
            projectedMap.SaveToDisk(string.Format("ScienceAlert/projected_{0}.png", current.name));

            var original = new Texture2D(current.BiomeMap.Map.width, current.BiomeMap.Map.height, TextureFormat.ARGB32, false);
            var origPixels = current.BiomeMap.Map.GetPixels32();
            original.SetPixels32(origPixels);
            original.Apply();

            original.SaveToDisk(string.Format("ScienceAlert/original_{0}.png", current.name));
#endif   
            Log.Verbose("Filtering biome map of {0} took {1} seconds", current.name, Time.realtimeSinceStartup - timer);

            projector = null; // we're finished!
        }



        /// <summary>
        /// When the player enters a new orbit, this function will be invoked
        /// to generate an aliasing/filtering-free biome map texture to avoid
        /// inaccurate results.
        /// </summary>
        /// <param name="bodies"></param>
        private void OnDominantBodyChanged(GameEvents.FromToAction<CelestialBody, CelestialBody> bodies)
        {
            Log.Write("BiomeFilter.OnDominantBodyChanged from {0} to {1}", bodies.from, bodies.to);
            ReprojectBiomeMap(bodies.to);
        }


        private void OnVesselChanged(Vessel v)
        {
            ReprojectBiomeMap(v.mainBody);
        }
        

        private bool IsBusy
        {
            get
            {
                return projector != null;
            }
        }
    }
}
