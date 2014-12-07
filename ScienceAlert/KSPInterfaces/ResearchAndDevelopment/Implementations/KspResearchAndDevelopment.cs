namespace ScienceAlert.KSPInterfaces.ResearchAndDevelopment.Implementations
{
    internal class KspResearchAndDevelopment : IResearchAndDevelopment
    {
        private global::ResearchAndDevelopment _kspResearchAndDevelopment;

        public KspResearchAndDevelopment(global::ResearchAndDevelopment ksp)
        {
            _kspResearchAndDevelopment = ksp;    
        }
    }
}