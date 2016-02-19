using System;
using System.Collections.Generic;

namespace ScienceAlert.Game
{
    // Note to self: make sure this isn't in a cross context binding; it's meant to have the same lifetime as the
    // dependencies its given (this prevents us from having to explicitly unregister listeners)
// ReSharper disable once ClassNeverInstantiated.Global
    public class KspFactory : IGameFactory
    {
        //private readonly SignalCrewOnEva _evaSignal;
        //private readonly SignalCrewTransferred _transferredSignal;

        //private readonly Dictionary<Vessel, KspVessel> _vesselDictionary = new Dictionary<Vessel, KspVessel>();
 
        //public KspFactory(
        //    SignalCrewOnEva evaSignal,
        //    SignalCrewTransferred transferredSignal)
        //{
        //    if (evaSignal == null) throw new ArgumentNullException("evaSignal");
        //    if (transferredSignal == null) throw new ArgumentNullException("transferredSignal");

        //    _evaSignal = evaSignal;
        //    _transferredSignal = transferredSignal;
        //}


        //public void OnVesselDestroyed(IVessel vessel)
        //{
        //    KspVessel kspVessel;

        //    if (!_vesselDictionary.TryGetValue(vessel, out kspVessel))
        //        return;

        //    _vesselDictionary.Remove(vessel);

        //    _evaSignal.RemoveListener(kspVessel.OnCrewOnEva);
        //    _transferredSignal.RemoveListener(kspVessel.OnCrewTransferred);
        //}


        //private IVessel GetOrCreateVessel(Vessel v)
        //{
        //    KspVessel vessel;

        //    if (_vesselDictionary.TryGetValue(v, out vessel)) return vessel;

        //    vessel = new KspVessel(this, v);
        //    _vesselDictionary.Add(v, vessel);

        //    _modifiedSignal.AddListener(vessel.OnVesselModified);
        //    _evaSignal.AddListener(vessel.OnCrewOnEva);
        //    _transferredSignal.AddListener(vessel.OnCrewTransferred);

        //    vessel.Rescan();

        //    return vessel;
        //}


        //public IVessel Create(Vessel vessel)
        //{
        //    return GetOrCreateVessel(vessel);
        //}


        public IModuleScienceExperiment Create(ModuleScienceExperiment mse)
        {
            if (mse == null) throw new ArgumentNullException("mse");

            return new KspScienceExperimentModule(this, mse);
        }


        public IPart Create(Part part)
        {
            if (part == null) throw new ArgumentNullException("Part");

            return new KspPart(part);
        }


        public IScienceSubject Create(ScienceSubject subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");

            return new KspScienceSubject(subject);
        }


        public ICelestialBody Create(CelestialBody body)
        {
            if (body == null) throw new ArgumentNullException("body");

            return new KspCelestialBody(body);
        }


        public IScienceLab Create(ModuleScienceLab lab)
        {
            if (lab == null) throw new ArgumentNullException("lab");

            return new KspScienceLabModule(lab);
        }


        public IUrlConfig Create(UrlDir.UrlConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");

            return new KspUrlConfig(config);
        }
    }
}
