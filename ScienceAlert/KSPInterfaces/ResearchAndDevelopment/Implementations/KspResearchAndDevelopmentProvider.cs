using System.Collections.Generic;

namespace ScienceAlert.KSPInterfaces.ResearchAndDevelopment.Implementations
{
    class KspResearchAndDevelopmentProvider : IResearchAndDevelopmentProvider
    {
        public Maybe<IResearchAndDevelopment> Instance
        {
            get { return Maybe<IResearchAndDevelopment>.With(new KspResearchAndDevelopment(global::ResearchAndDevelopment.Instance)); }
        }

        public IEnumerable<string> GetExperimentIDs()
        {
            return global::ResearchAndDevelopment.GetExperimentIDs();
        }
    }
}
