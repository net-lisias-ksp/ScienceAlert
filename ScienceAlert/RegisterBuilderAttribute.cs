using System;
using JetBrains.Annotations;

namespace ScienceAlert
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegisterBuilderAttribute : Attribute
    {
        public readonly Type BuilderInterface;
        public readonly int Priority = 0;

        public RegisterBuilderAttribute([NotNull] Type builderInterface, int priority = 0)
        {
            if (builderInterface == null) throw new ArgumentNullException("builderInterface");

            BuilderInterface = builderInterface;
            Priority = priority;
        }
    }
}
