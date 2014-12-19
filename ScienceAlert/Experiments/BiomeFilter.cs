using System;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Experiments
{
    public class BiomeFilter : MonoBehaviour
    {
        private const int HalfSearchDimensions = 3;

        private string _currentBiome;
        private Color[,] _mapColors;
        private bool _mapFinished = false;
        private System.Collections.IEnumerator _mapProjector;
        private const float ColorThreshold = 0.005f;

        private void Start()
        {
            GameEvents.onDominantBodyChange.Add(DominantBodyChanged);
            DominantBodyChanged(new GameEvents.FromToAction<CelestialBody, CelestialBody>(null,
                FlightGlobals.currentMainBody));
        }

        private void OnDestroy()
        {
            GameEvents.onDominantBodyChange.Remove(DominantBodyChanged);
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
                _mapProjector.MoveNext();

            double lat = vessel.latitude * Mathf.Deg2Rad;
            double lon = vessel.longitude * Mathf.Deg2Rad;

            var stockMatch = vessel.mainBody.BiomeMap.GetAtt(lat, lon);

            if (IsResultValid(stockMatch, lat, lon))
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

            _mapProjector = ProjectMap(bodies.to);
        }


        private System.Collections.IEnumerator ProjectMap(CelestialBody body)
        {
            float startTime = Time.realtimeSinceStartup;
            var bm = body.BiomeMap;
            _mapFinished = false;

            _mapColors = new Color[bm.Width, bm.Height];

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
                    _mapColors[x,y] = body.BiomeMap.GetAtt(lat, lon).mapColor;
                }

                yield return 0;
            }

            Log.Normal("Projected biome map in {0} seconds", (Time.realtimeSinceStartup - startTime).ToString("F2"));

            _mapProjector = null;
        }


        bool IsResultValid(CBAttributeMapSO.MapAttribute attr, double lat, double lon)
        {
            if (_mapProjector != null)
                return true; // can't prove it's wrong

            lon -= Mathf.PI * 0.5f;
            if (lon < 0d) lon += Mathf.PI * 2d;
            lon %= Mathf.PI * 2d;

            int x_center = (int)Math.Round(_mapColors.GetLength(0) * (float)(lon / (Mathf.PI * 2)), 0);
            int y_center = (int)Math.Round(_mapColors.GetLength(1) * ((float)(lat / Mathf.PI) + 0.5f), 0);

            for (int y = y_center - HalfSearchDimensions; y < y_center + HalfSearchDimensions; ++y)
                if (y >= 0 && y < _mapColors.GetLength(1))
                    for (int x = x_center - HalfSearchDimensions; x < x_center + HalfSearchDimensions; ++x)
                        if (x >= 0 && x < _mapColors.GetLength(0))
                            if (Similar(attr.mapColor, _mapColors[x, y]))
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
