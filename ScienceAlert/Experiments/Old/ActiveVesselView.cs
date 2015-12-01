//using System.Collections.Generic;
//using ReeperCommon.Containers;
//using strange.extensions.implicitBind;
//using strange.extensions.injector;
//using strange.extensions.mediation.impl;
//using strange.extensions.signal.impl;
//using UnityEngine;

//namespace ScienceAlert.Experiments
//{
//    [MediatedBy(typeof(ActiveVesselMediator))]
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class ActiveVesselView : View
//    {
//        [Inject] public Vessel OurVessel { get; set; }


//        [PostConstruct(0)]
//        public void InitialSetup()
//        {
//            print("VesselView.InitialSetup");

//            if (OurVessel == null)
//                Debug.LogWarning("ActiveVesselView created with a null Vessel");
//            else print("We are now monitoring " + OurVessel.GetName());
//        }


//        protected override void OnDestroy()
//        {
//            base.OnDestroy();
//        }


//        /// <summary>
//        /// Called every frame by Unity
//        /// </summary>
//        private void Update()
//        {
            
//        }


  


//        //private bool IsOurVessel(Vessel vessel)
//        //{
//        //    return ReferenceEquals(vessel, OurVessel);
//        //}
//    }
//}
