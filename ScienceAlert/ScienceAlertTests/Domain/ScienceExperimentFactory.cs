using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlertTests.Domain
{
    public static class ScienceExperimentFactory
    {
        private static readonly Dictionary<string, ScienceExperiment> ScienceExperiments = new Dictionary
            <string, ScienceExperiment>
                {
                    {
                        "mysteryGoo", new ScienceExperiment
                        {
                            id = "mysteryGoo",
                            experimentTitle = "Mystery Goo",

                            baseValue = 10f,
                            dataScale = 1f,
                            scienceCap = 13f,
                            
                            biomeMask = 3,
                            situationMask = 63
                        }

                    },
                    {
                        "mobileMaterialsLab", new ScienceExperiment
                        {
                            id = "mobileMaterialsLab",
                            experimentTitle = "Materials Study",

                            baseValue = 25f,
                            dataScale = 1f,
                            scienceCap = 34f,

                            biomeMask = 3,
                            situationMask = 63
                        }
                    }
                };

        public static ScienceExperiment Create(string name)
        {
            ScienceExperiment result;

            if (!ScienceExperiments.TryGetValue(name, out result))
                throw new ArgumentException("No ScienceExperiment defined for " + name, "name");

            return result;
        }
    }
}
