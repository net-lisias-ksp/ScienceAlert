//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using ScienceAlert.Experiments;
//using strange.extensions.command.impl;

//namespace ScienceAlert.Core
//{
//    public class CommandConfigureVesselServices : Command
//    {
//        private readonly SignalVesselChanged _vesselChanged;
//        private readonly SignalVesselDestroyed _vesselDestroyed;

//        public CommandConfigureVesselServices(SignalVesselChanged vesselChanged, SignalVesselDestroyed vesselDestroyed)
//        {
//            if (vesselChanged == null) throw new ArgumentNullException("vesselChanged");
//            if (vesselDestroyed == null) throw new ArgumentNullException("vesselDestroyed");

//            _vesselChanged = vesselChanged;
//            _vesselDestroyed = vesselDestroyed;
//        }


//        public override void Execute()
//        {
            
//        }


//        private IActiveVesselProvider ConfigureActiveVesselService()
//        {
//            var getActiveService = new ActiveVesselProvider();

//            _vesselDestroyed.AddListener(getActiveService.OnVesselDestroyed)
//        }
//    }
//}
