using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;

namespace ScienceAlertTests
{
    public class DomainCustomization : CompositeCustomization
    {
        public DomainCustomization()
            : base(new MultipleCustomization(), new AutoNSubstituteCustomization())
        {
        }
    }
}
