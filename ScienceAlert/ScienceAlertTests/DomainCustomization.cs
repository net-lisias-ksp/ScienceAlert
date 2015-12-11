using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
