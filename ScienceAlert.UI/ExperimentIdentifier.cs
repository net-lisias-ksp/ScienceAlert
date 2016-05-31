using System;

namespace ScienceAlert.UI
{
    public interface IExperimentIdentifier : IEquatable<string>, IEquatable<IExperimentIdentifier>
    {
    }
}
