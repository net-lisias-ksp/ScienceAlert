using System.Collections.Generic;

namespace ScienceAlert.Game
{
    public interface IResearchAndDevelopment
    {
        IEnumerable<ScienceExperiment> Experiments { get; }
    }
}
