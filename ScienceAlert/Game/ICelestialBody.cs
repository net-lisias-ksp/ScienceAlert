using System;

namespace ScienceAlert.Game
{
    public interface ICelestialBody : IEquatable<ICelestialBody>
    {
        CelestialBody Body { get; }

        string BodyName { get; }
    }
}
