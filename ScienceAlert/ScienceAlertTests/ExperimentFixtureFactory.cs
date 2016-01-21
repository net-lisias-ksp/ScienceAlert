//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NSubstitute;
//using Ploeh.AutoFixture;
//using ScienceAlert.Game;

//namespace ScienceAlertTests
//{
//    class ExperimentFixtureFactory
//    {
//        public ExperimentFixtureFactory(Fixture fixture, ExperimentSituations situation)
//        {
//            // MysteryGoo
//            fixture.Register(() => new ScienceExperiment
//            {
//                baseValue = 10f,
//                dataScale = 1f,
//                scienceCap = 13f,
//                biomeMask = 3,
//                situationMask = 63,
//                experimentTitle = "Mystery Goo",
//                id = "mysteryGoo"
//            });


//            var gooSubject = Substitute.For<IScienceSubject>();

//            gooSubject.Id.Returns("mysteryGoo@Kerbin" + situation);
//            gooSubject.Science.Returns(0f);
//            gooSubject.ScienceCap.Returns(13f);
//            gooSubject.

//        }
//    }
//}
