using System;

namespace ScienceAlert.Game
{
    public class KspScienceSubject : IScienceSubject
    {
        public KspScienceSubject(ScienceSubject subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            Subject = subject;
        }


        public ScienceSubject Subject { get; private set; }


        public string Id
        {
            get { return Subject.id; }
        }

        public float DataScale
        {
            get { return Subject.dataScale; }
        }

        public float Science
        {
            get { return Subject.science; }
        }

        public float ScientificValue
        {
            get { return Subject.scientificValue; }
        }

        public float ScienceCap
        {
            get { return Subject.scienceCap; }
        }

        public float SubjectValue
        {
            get { return Subject.subjectValue; }
        }
    }
}
