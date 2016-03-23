using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;

namespace ScienceAlert
{
    public class DefaultObjectFromConfigNodeBuilder<TObjectType> : IConfigNodeObjectBuilder<TObjectType>
    {
        protected readonly IConfigNodeSerializer Serializer;

        public DefaultObjectFromConfigNodeBuilder(IConfigNodeSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            Serializer = serializer;
        }


        public TObjectType Build(ConfigNode config)
        {
            if (!CanHandle(config) && config != null)
                throw new ArgumentException("Builder cannot handle object defined by ConfigNode: " +
                                            (config.ToSafeString()));

            EnsureTypeCanBeCreated();

            if (typeof(TObjectType).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .All(ci => ci.GetParameters().Length != 0))
                throw new InvalidOperationException("The type " + typeof(TObjectType) + " cannot be created because it does not have a public parameterless constructor");

            var instance = Activator.CreateInstance<TObjectType>();

            if (config != null)
                Serializer.LoadObjectFromConfigNode(ref instance, config);

            return instance;
        }


        protected static void EnsureTypeCanBeCreated()
        {
            if (typeof(TObjectType).IsAbstract || typeof(TObjectType).IsInterface)
                throw new InvalidOperationException("The type " + typeof(TObjectType) + " can't be created");
        }


        public virtual bool CanHandle(ConfigNode config)
        {
            if (config == null) return false;

            return typeof(TObjectType).With(rt => new[] { rt.FullName.ToUpperInvariant(), rt.Name.ToUpperInvariant() })
                .Any(typeName => typeName.Equals(config.name.ToUpperInvariant()));
        }
    }

    class BuilderConstructedTypeCannotBeAssignedToReturnTypeException : Exception
    {
        public BuilderConstructedTypeCannotBeAssignedToReturnTypeException(Type builderType, Type returnType)
            : base(builderType.FullName + " cannot be assigned to return type " + returnType.FullName)
        {
            
        }
    }

    // This builder is very simple: given a ConfigNode with a name that matches the Type name,
    // create an instance (using a temporary binding factory, if provided) and deserialize the object from that config
    // note: the return type argument is needed to avoid having to re-implement the Build method in any
    // derived classes that build a family of objects (such as the rule builder: it would need a build method with
    // a signature of IExperimentRule Build(ConfigNode, TParameterType, ItemporaryBindingFactory) and hide the existing
    // ConcreteRuleType Build(etc)
    public class DefaultObjectFromConfigNodeBuilder<TConcreteType, TObjectReturnType, TParameterType> : 
        DefaultObjectFromConfigNodeBuilder<TConcreteType>,
        IConfigNodeObjectBuilder<TObjectReturnType, TParameterType, ITemporaryBindingFactory> 
    {
        public DefaultObjectFromConfigNodeBuilder(IConfigNodeSerializer serializer) : base(serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");

            if (!typeof (TObjectReturnType).IsAssignableFrom(typeof (TConcreteType)))
                throw new BuilderConstructedTypeCannotBeAssignedToReturnTypeException(typeof (TConcreteType),
                    typeof (TObjectReturnType));
        }


        public override string ToString()
        {
            return typeof(DefaultObjectFromConfigNodeBuilder<TConcreteType, TObjectReturnType, TParameterType>).Name + "[ " + typeof(TConcreteType).Name + "]";
        }


        // note to self: default object builder doesn't allow inner types
        public virtual TObjectReturnType Build(ConfigNode config, TParameterType unused, ITemporaryBindingFactory bindingFactory)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (bindingFactory == null) throw new ArgumentNullException("bindingFactory");

            EnsureTypeCanBeCreated();

            if (!CanHandle(config)) throw new ArgumentException(this + " cannot handle " + config);

            using (var binding = bindingFactory.Create(typeof(TConcreteType)))
            {
                var instance = (TConcreteType)binding.GetInstance();

                Serializer.LoadObjectFromConfigNode(ref instance, config);

                return (TObjectReturnType)(object)instance;
            }
        }
    }
}
