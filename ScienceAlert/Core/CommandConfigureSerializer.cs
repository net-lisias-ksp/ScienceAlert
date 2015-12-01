using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Serialization;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandConfigureSerializer : Command
    {
        public override void Execute()
        {
            var assembliesToScanForSurrogates = new[] { typeof(IConfigNodeSerializer).Assembly, Assembly.GetExecutingAssembly() }
            .Distinct() // in release mode, ReeperCommon gets mashed into executing assembly
            .ToArray();

            var supportedTypeQuery = new GetSurrogateSupportedTypes();
            var surrogateQuery = new GetSerializationSurrogates(supportedTypeQuery);
            var serializableFieldQuery = new GetObjectFieldsIncludingBaseTypes();

            var standardSerializerSelector =
                new SerializerSelector(
                    new CompositeSurrogateProvider(
                        new GenericSurrogateProvider(surrogateQuery, supportedTypeQuery, assembliesToScanForSurrogates),
                        new SurrogateProvider(surrogateQuery, supportedTypeQuery, assembliesToScanForSurrogates)));


            var preferNativeSelector = new PreferNativeSerializer(standardSerializerSelector);

            var selectorOrder = new CompositeSerializerSelector(
                preferNativeSelector,          // always prefer native serializer first 
                standardSerializerSelector);   // otherwise, find any surrogate


            var includePersistentFieldsSelector = new SerializerSelectorDecorator(
                selectorOrder,
                s => Maybe<IConfigNodeItemSerializer>.With(new FieldSerializer(s, serializableFieldQuery)));

            injectionBinder.Bind<IConfigNodeSerializer>()
                .To(new ConfigNodeSerializer(includePersistentFieldsSelector))
                .CrossContext();
        }
    }
}
