using System;
using System.Linq;

namespace ScienceAlert.Game
{
    public class KspScienceLabModule : IScienceLab
    {
        private readonly ModuleScienceLab _lab;

        public KspScienceLabModule(ModuleScienceLab lab)
        {
            if (lab == null) throw new ArgumentNullException("lab");
            _lab = lab;
        }

        public bool HasAnalyzedSubject(IScienceSubject subject)
        {
            return _lab.ExperimentData.Any(data => data == subject.Id);
        }

        public float SurfaceBonus
        {
            get { return _lab.SurfaceBonus; }
        }

        public float ContextBonus
        {
            get { return _lab.ContextBonus; }
        }

        public float HomeworldMultiplier
        {
            get { return _lab.homeworldMultiplier; }
        }
    }
}
