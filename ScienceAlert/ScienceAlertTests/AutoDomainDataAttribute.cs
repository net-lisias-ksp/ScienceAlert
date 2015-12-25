using System.Linq;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using ScienceAlert.VesselContext.Experiments.Rules;

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

            var ruleProvider = Substitute.For<IExperimentRuleTypeProvider>();
            ruleProvider.Get()
                .Returns(
                    typeof (IExperimentRule).Assembly.GetTypes()
                        .Where(t => typeof (IExperimentRule).IsAssignableFrom(t) && !t.IsAbstract));

            Fixture.Register(() => new ConfigNode("root"));
            Fixture.Register(() => rules);
            Fixture.Register(() => new RuleDefinitionFactory(ruleProvider));
            Fixture.Register(() => new ProtoCrewMember(ProtoCrewMember.KerbalType.Crew) {name = "Test Kerman"});
            
            //Fixture.Register(() => new SimplePersistentObject());
            //Fixture.Register(() => new ComplexPersistentObject());
            //Fixture.Register(() => new GetSerializableFields());
            ////Fixture.Register(() => new DefaultSurrogateProvider());
            //Fixture.Register(() => new Rect(0f, 0f, 100f, 100f));
            ////Fixture.Register(
            ////    () =>
            ////        new ConfigNodeSerializer(
            ////            new DefaultConfigNodeItemSerializerSelector(new SurrogateProvider(new[] {Assembly.GetExecutingAssembly(), typeof(ConfigNodeSerializer).Assembly})),
            ////            new GetSerializableFields()));

            ////Fixture.Register(() => new DefaultConfigNodeItemSerializerSelector());
            //Fixture.Register(() => new GetSurrogateSupportedTypes());
            ////Fixture.Register(() => new PrimitiveSurrogateSerializer());
            //Fixture.Register(() => new NativeSerializer());
            ////Fixture.Register(() => new PersistenceMethodCaller(Substitute.For<IConfigNodeItemSerializer>()));

            //Fixture.Register(() =>
            //{
            //    var serializer = new ConfigNodeSerializer(
            //        new SerializerSelectorDecorator(
            //            new PreferNativeSerializer(
            //                new SerializerSelector(
            //                    new SurrogateProvider(
            //                        new GetSerializationSurrogates(new GetSurrogateSupportedTypes()),
            //                        new GetSurrogateSupportedTypes(),
            //                        AppDomain.CurrentDomain.GetAssemblies()
            //                            .Where(a => a.GetName().Name.StartsWith("ReeperCommon")).ToArray()))),
            //        result => Maybe<IConfigNodeItemSerializer>.With(new FieldSerializer(result, new GetSerializableFields()))));

            //    return serializer;
            //});

            //Fixture.Register(
            //    () => new Quaternion((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()));
        }
    }
    
}
