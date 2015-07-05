using System;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Experiments
{
    public class BiomeFilter : MonoBehaviour
    {
        private const int HalfSearchDimensions = 3;
        private const float ColorThreshold = 0.005f;

        private string _currentBiome;
        private Texture2D _cleanedBiomeMap;

        private System.Collections.IEnumerator _mapProjector;
        

        private void Start()
        {
            GameEvents.onDominantBodyChange.Add(DominantBodyChanged);
            DominantBodyChanged(new GameEvents.FromToAction<CelestialBody, CelestialBody>(null,
                FlightGlobals.currentMainBody));
            Log.Debug("BiomeFilter created");
        }


        private void OnDestroy()
        {
            GameEvents.onDominantBodyChange.Remove(DominantBodyChanged);
            if (_cleanedBiomeMap != null) Destroy(_cleanedBiomeMap);
            Log.Debug("BiomeFilter destroyed");
        }


        private void Update()
        {
            var vessel = FlightGlobals.ActiveVessel;

            if (vessel == null || vessel.mainBody.BiomeMap == null)
            {
                _currentBiome = string.Empty;
                return;
            }

            if (_mapProjector != null)
                if (!_mapProjector.MoveNext())
                    _mapProjector = null;

            double lat = vessel.latitude * Mathf.Deg2Rad;
            double lon = vessel.longitude * Mathf.Deg2Rad;

            var stockMatch = vessel.mainBody.BiomeMap.GetAtt(lat, lon);

            // bandaid fix: if we're below 2km, ignore the valid result check. This should prevent ScienceAlert
            // from failing to warn while in "incorrect" biomes, e.g. should warn about tundra even though tundra
            // is wrong because all the regular experiments are incorrectly reporting tundra
            if (IsResultValid(stockMatch, lat, lon) || FlightGlobals.ActiveVessel.GetHeightFromSurface() <= 2000f)
                _currentBiome = stockMatch.name;
#if DEBUG
            else Log.Warning("{0} looks like a bad biome result", stockMatch.name);
#endif
        }

        private void DominantBodyChanged(GameEvents.FromToAction<CelestialBody, CelestialBody> bodies)
        {
            _currentBiome = string.Empty;

            Log.Debug("BiomeFilter.DominantBodyChanged: from {0} to {1}",
                bodies.from != null ? bodies.from.GetName() : "<null>", bodies.to.GetName());

#if DEBUG
            bodies.to.BiomeMap.CompileToTexture().CreateReadable().SaveToDisk("body_" + bodies.to.bodyName + " _map.png");
#endif
            
            _mapProjector = ProjectMap(bodies.to);
        }


        private System.Collections.IEnumerator ProjectMap(CelestialBody body)
        {
            float startTime = Time.realtimeSinceStartup;
            var bm = body.BiomeMap;

            if (_cleanedBiomeMap != null)
                Destroy(_cleanedBiomeMap);

            if (body.BiomeMap == null)
            {
                Log.Debug("No biome map associated with " + body.GetName() + "; not creating cleaned biome map for it");
                yield break;
            }

            _cleanedBiomeMap = new Texture2D(body.BiomeMap.Width, body.BiomeMap.Height, TextureFormat.RGBA32, false);

            for (int y = 0; y < bm.Height; ++y)
            {
                for (int x = 0; x < bm.Width; ++x)
                {
                    // convert x and y into uv coordinates
                    float u = (float)x / bm.Width;
                    float v = (float)y / bm.Height;

                    // convert uv coordinates into latitude and longitude
                    double lat = Math.PI * v - Math.PI * 0.5;
                    double lon = 2d * Math.PI * u + Math.PI * 0.5;

                    // set biome color in our clean texture
                    // note we're not using a fast and easy array to do this because users will look at
                    // the GC and not understand what's happening
                    _cleanedBiomeMap.SetPixel(x, y, body.BiomeMap.GetAtt(lat, lon).mapColor);
                }

                yield return 0;
            }

            _cleanedBiomeMap.Apply(false);

            Log.Normal("Projected biome map in {0} seconds", (Time.realtimeSinceStartup - startTime).ToString("F2"));

            _mapProjector = null;
        }


        bool IsResultValid(CBAttributeMapSO.MapAttribute attr, double lat, double lon)
        {
            if (_mapProjector != null || _cleanedBiomeMap == null)
                return true; // can't prove it's wrong

            lon -= Mathf.PI * 0.5f;
            if (lon < 0d) lon += Mathf.PI * 2d;
            lon %= Mathf.PI * 2d;

            int x_center = (int)Math.Round(_cleanedBiomeMap.width * (float)(lon / (Mathf.PI * 2)), 0);
            int y_center = (int)Math.Round(_cleanedBiomeMap.height * ((float)(lat / Mathf.PI) + 0.5f), 0);

            for (int y = y_center - HalfSearchDimensions; y < y_center + HalfSearchDimensions; ++y)
                if (y >= 0 && y < _cleanedBiomeMap.height)
                    for (int x = x_center - HalfSearchDimensions; x < x_center + HalfSearchDimensions; ++x)
                        if (x >= 0 && x < _cleanedBiomeMap.width)
                            if (Similar(attr.mapColor, _cleanedBiomeMap.GetPixel(x, y)))
                                return true;

            return false;
        }



        private bool Similar(Color first, Color second)
        {
            return Mathf.Abs(first.r - second.r) < ColorThreshold && Mathf.Abs(first.g - second.g) < ColorThreshold && Mathf.Abs(first.b - second.b) < ColorThreshold;
        }

        public string GetCurrentBiome()
        {
            if (FlightGlobals.ActiveVessel == null) return string.Empty;

            return string.IsNullOrEmpty(FlightGlobals.ActiveVessel.landedAt)
                ? _currentBiome
                : Vessel.GetLandedAtString(FlightGlobals.ActiveVessel.landedAt);
        }
    }
}
