using System;

namespace ScienceAlert.Game
{
    public class KspCelestialBody : ICelestialBody
    {
        public KspCelestialBody(CelestialBody body)
        {
            if (body == null) throw new ArgumentNullException("body");
            Body = body;
        }

        public CelestialBody Body { get; private set; }

        public string BodyName
        {
            get { return Body.bodyName; }
        }
    }
}
