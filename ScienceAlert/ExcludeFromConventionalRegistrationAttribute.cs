using System;

namespace ScienceAlert
{
    /// <summary>
    /// Applied to builders that should not be auto-registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    class ExcludeFromConventionalRegistrationAttribute : Attribute
    {
    }
}
