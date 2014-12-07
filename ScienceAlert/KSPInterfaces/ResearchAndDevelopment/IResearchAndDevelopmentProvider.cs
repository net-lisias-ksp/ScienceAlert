using System.Collections.Generic;

namespace ScienceAlert.KSPInterfaces.ResearchAndDevelopment
{
    public interface IResearchAndDevelopmentProvider
    {
        Maybe<IResearchAndDevelopment> Instance { get; }
        IEnumerable<string> GetExperimentIDs();
    }
}
