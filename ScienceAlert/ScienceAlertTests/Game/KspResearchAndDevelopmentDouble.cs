using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlertTests.Game
{
    public class KspResearchAndDevelopmentDouble : IQueryScienceValue
    {
        public float GetScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar)
        {
            float num = Mathf.Min(GetReferenceDataValue(dataAmount, subject) * subject.ScientificValue * xmitScalar, subject.ScienceCap);
            float b = Mathf.Lerp(GetReferenceDataValue(dataAmount, subject), subject.ScienceCap, xmitScalar) * xmitScalar;
            float num2 = Mathf.Min(subject.Science + num, b);
            return Mathf.Max(num2 - subject.Science, 0f);
        }

        public float GetNextScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar)
        {
            float num = subject.Science + GetScienceValue(dataAmount, subject, xmitScalar);
            float subjectValue = GetSubjectValue(num, subject);
            float num2 = Mathf.Min(GetReferenceDataValue(dataAmount, subject) * subjectValue * xmitScalar, subject.ScienceCap);
            float b = Mathf.Lerp(GetReferenceDataValue(dataAmount, subject), subject.ScienceCap, xmitScalar) * xmitScalar;
            float num3 = Mathf.Min(num + num2, b);
            return Mathf.Max(num3 - num, 0f);
        }


        public static float GetReferenceDataValue(float dataAmount, IScienceSubject subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");

            return dataAmount / subject.DataScale * subject.SubjectValue;
        }


        public static float GetSubjectValue(float subjectScience, IScienceSubject subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");

            return Mathf.Max(0f, 1f - subjectScience / subject.ScienceCap);
        }
    }
}
