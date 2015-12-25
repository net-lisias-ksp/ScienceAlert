using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Experience;
using NSubstitute;
using Ploeh.AutoFixture;
using ScienceAlert.Game;
using Xunit;
using Xunit.Extensions;

namespace ScienceAlertTests.Game
{
    public class KspScienceUtilTests
    {
        public class Factory
        {
            public IFixture Fixture { get; private set; }

            public Factory(IFixture fixture)
            {
                if (fixture == null) throw new ArgumentNullException("fixture");
                Fixture = fixture;
            }

            public IVessel BuildControllableVessel(IPart part)
            {
                var item = Fixture.Create<IVessel>();

                //item.EvaCapableCrew.Returns(new ReadOnlyCollection<ProtoCrewMember>(
                //    new [] { new ProtoCrewMember(ProtoCrewMember.KerbalType.Crew) { name = "Test Kerman" }}));
                item.EvaCapableCrew.Returns(ci => new ReadOnlyCollection<ProtoCrewMember>(part.EvaCapableCrew));

                item.IsControllable.Returns(true);

                return item;
            }

            public IVessel BuildUncontrollableVessel(IPart part)
            {
                var item = BuildControllableVessel(part);

                item.IsControllable.Returns(false);

                return item;
            }

            public IPart BuildCrewedPart()
            {
                var item = Fixture.Create<IPart>();

                var crew =
                    new List<ProtoCrewMember>(new[]
                    {new ProtoCrewMember(ProtoCrewMember.KerbalType.Crew) {name = "Test Kerman"}});

                item.EvaCapableCrew.Returns(crew);

                return item;
            }

            public IPart BuildUncrewedPart()
            {
                var item = Fixture.Create<IPart>();

                item.EvaCapableCrew.Returns(new List<ProtoCrewMember>());
                return item;
            }
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_CrewInPart_WithUncrewedPart_Test(KspScienceUtil sut, IFixture fixture)
        {
            var factory = new Factory(fixture);

            var part = factory.BuildUncrewedPart();
            var vessel = factory.BuildControllableVessel(part);

            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.CrewInPart);

            Assert.False(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_CrewInPart_WithCrewedPart_Test(KspScienceUtil sut, IFixture fixture)
        {
            var factory = new Factory(fixture);

            var part = factory.BuildCrewedPart();
            var vessel = factory.BuildControllableVessel(part);

            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.CrewInPart);

            Assert.True(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_CrewInVessel_WithUncrewedVessel_Test(KspScienceUtil sut, IFixture fixture)
        {
            var factory = new Factory(fixture);

            var part = factory.BuildUncrewedPart();
            var vessel = factory.BuildControllableVessel(part);

            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.CrewInVessel);

            Assert.False(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_CrewInVessel_WithCrewedVessel_Test(KspScienceUtil sut, IFixture fixture)
        {
            var factory = new Factory(fixture);

            var part = factory.BuildCrewedPart();
            var vessel = factory.BuildControllableVessel(part);

            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.CrewInVessel);

            Assert.True(result);
        }

        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_NeverAllow_Test(KspScienceUtil sut, IVessel vessel, IPart part)
        {
            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.Never);

            Assert.False(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_AlwaysAllow_Test(KspScienceUtil sut, IVessel vessel, IPart part)
        {
            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.Always);

            Assert.True(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_VesselControllable_WithUncontrollableVessel_Test(KspScienceUtil sut, IFixture fixture)
        {
            var factory = new Factory(fixture);

            var part = factory.BuildUncrewedPart();
            var vessel = factory.BuildUncontrollableVessel(part);

            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.VesselControl);

            Assert.False(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_VesselControllable_WithControllableVessel_Test(KspScienceUtil sut, IFixture fixture)
        {
            var factory = new Factory(fixture);

            var part = factory.BuildUncrewedPart();
            var vessel = factory.BuildControllableVessel(part);

            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.VesselControl);

            Assert.True(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_ScientistCrew_WithNoScientistOnboard_Test(KspScienceUtil sut,
            IFixture fixture)
        {
            var factory = new Factory(fixture);

            var part = factory.BuildCrewedPart();
            var vessel = factory.BuildControllableVessel(part);

            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.ScientistCrew);

            Assert.False(result);
        }


        [Theory, AutoDomainData]
        public void RequiredUsageInternalAvailable_ScientistCrew_WithScientistOnboard_Test(KspScienceUtil sut,
            IFixture fixture)
        {
            var factory = new Factory(fixture);
            var part = Substitute.For<IPart>();
            var scientistCrew = new ProtoCrewMember(ProtoCrewMember.KerbalType.Crew);
            var traitConfigNode = new ConfigNode("EXPERIENCE_TRAIT");

            traitConfigNode.AddValue("name", "Scientist");
            traitConfigNode.AddValue("title", "Scientist");

            var traitConfig = ExperienceTraitConfig.Create(traitConfigNode);

            var trait = ExperienceTrait.Create(typeof (ExperienceTrait), traitConfig, scientistCrew);
            scientistCrew.experienceTrait = trait;

            part.EvaCapableCrew.Returns(new List<ProtoCrewMember>(new [] { scientistCrew }));

            var vessel = factory.BuildControllableVessel(part);



            var result = sut.RequiredUsageInternalAvailable(vessel, part, ExperimentUsageReqs.ScientistCrew);

            Assert.True(result);
        }
    }
}
