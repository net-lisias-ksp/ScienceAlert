using System;
using ReeperCommon.Containers;
using ScienceAlert.Annotations;

namespace ScienceAlert.Game
{
    public class KspActiveVesselProvider : IVesselProvider
    {
        private readonly IKspFactory _kspFactory;

        public KspActiveVesselProvider([NotNull] IKspFactory kspFactory)
        {
            if (kspFactory == null) throw new ArgumentNullException("kspFactory");
            _kspFactory = kspFactory;
        }


        public Maybe<IVessel> Get()
        {
            return FlightGlobals.ActiveVessel == null
                ? Maybe<IVessel>.None
                : Maybe<IVessel>.With(_kspFactory.Create(FlightGlobals.ActiveVessel));
        }
    }
}
