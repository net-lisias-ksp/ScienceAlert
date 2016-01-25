using System;

namespace ScienceAlert.Game
{
    public class KspResearchAndDevelopment : IQueryScienceValue
    {
        public float GetScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (dataAmount < 0f) throw new ArgumentException("Negative data amount", "dataAmount");
            if (xmitScalar < 0f) throw new ArgumentException("Negative transmission multiplier", "xmitScalar");

            return ResearchAndDevelopment.GetScienceValue(dataAmount, subject.Subject, xmitScalar);
        }

        public float GetNextScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (dataAmount < 0f) throw new ArgumentException("Negative data amount", "dataAmount");
            if (xmitScalar < 0f) throw new ArgumentException("Negative transmission multiplier", "xmitScalar");

            return ResearchAndDevelopment.GetNextScienceValue(dataAmount, subject.Subject, xmitScalar);
        }

        public float GetReferenceDataValue(float dataAmount, IScienceSubject subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (dataAmount < 0f) throw new ArgumentException("Negative data amount", "dataAmount");

            return ResearchAndDevelopment.GetReferenceDataValue(dataAmount, subject.Subject);
        }
    }
}
