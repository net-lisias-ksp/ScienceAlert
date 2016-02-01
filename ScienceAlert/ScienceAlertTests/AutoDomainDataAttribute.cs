using System.Linq;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using ScienceAlert.VesselContext.Experiments.Rules;
using ScienceAlertTests.Domain;

namespace ScienceAlertTests
{
    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
            : base(new Fixture().Customize(new DomainCustomization()))
        {
            //var rnd = new Random();
            var rules = typeof (IExperimentRule).Assembly.GetTypes()
                .Where(t => typeof (IExperimentRule).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            //var ruleProvider = Substitute.For<IExperimentRuleTypeProvider>();
            //ruleProvider.Get()
            //    .Returns(
            //        typeof (IExperimentRule).Assembly.GetTypes()
            //            .Where(t => typeof (IExperimentRule).IsAssignableFrom(t) && !t.IsAbstract));

            Fixture.Register(() => new ConfigNode("root"));
            Fixture.Register(() => rules);
            Fixture.Register(() => new ProtoCrewMember(ProtoCrewMember.KerbalType.Crew) {name = "Test Kerman"});
            
            Fixture.Register(() => 
                new ScienceExperiment
                {
                    baseValue = 0f, 
                    biomeMask = 0,
                    dataScale = 0f, 
                    experimentTitle = "Experiment Title", 
                    id = "scienceExperiment", 
                    requireAtmosphere = false,
                    requiredExperimentLevel = 0f,
                    scienceCap = 100f, 
                    situationMask = 0
                });

            Fixture.Register(() => new ScienceSubject("testId", "test title", 0f, 0f, 100f));
            Fixture.Register(() => new ScienceSubjectFactory());
        }
    }
    
}
