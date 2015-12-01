//using System.Linq;
//using ReeperCommon.Containers;

//namespace ScienceAlert.Experiments
//{
//    public class ActiveVesselProvider : IActiveVesselProvider
//    {
//        private Maybe<IVessel> _currentVessel;

//        public void SetVessel(IVessel vessel)
//        {
//            _currentVessel = vessel.ToMaybe();
//        }


//        public Maybe<IVessel> Get()
//        {
//            return _currentVessel;
//        }


//        public void OnVesselDestroyed(IVessel vessel)
//        {
//            if (_currentVessel.Any() && _currentVessel.Single().Equals(vessel))
//                _currentVessel = Maybe<IVessel>.None;
            
//        }

//        public void OnVesselChanged(IVessel newVessel)
//        {
//            SetVessel(newVessel);
//        }
//    }
//}
