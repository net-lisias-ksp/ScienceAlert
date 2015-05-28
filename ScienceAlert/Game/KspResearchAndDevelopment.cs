using System.Collections.Generic;
using System.Linq;

namespace ScienceAlert.Game
{
    public class KspResearchAndDevelopment : IResearchAndDevelopment
    {
        public IEnumerable<ScienceExperiment> Experiments
        {
            get { return ResearchAndDevelopment.GetExperimentIDs().Select(ResearchAndDevelopment.GetExperiment); }
        }
    }
}
