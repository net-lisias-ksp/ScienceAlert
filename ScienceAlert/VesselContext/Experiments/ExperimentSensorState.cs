﻿using System;
using System.Text;
using JetBrains.Annotations;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    public struct ExperimentSensorState
    {
        public ScienceExperiment Experiment { get; private set; }
        public IScienceSubject Subject { get; private set; }

        public float RecoveryValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }

        public bool Onboard { get; private set; }
        public bool Available { get; private set; }
        public bool ConditionsMet { get; private set; }

        public ExperimentSensorState([NotNull] ScienceExperiment experiment, [NotNull] IScienceSubject subject, 
            float recoveryValue, 
            float transmissionValue, 
            float labValue, 
            bool onboard, 
            bool available, 
            bool conditionsMet) : this()
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (subject == null) throw new ArgumentNullException("subject");
            if (recoveryValue < 0f) throw new ArgumentOutOfRangeException("recoveryValue", "must be >= 0f");
            if (transmissionValue < 0f) throw new ArgumentOutOfRangeException("transmissionValue", "must be >= 0f");
            if (labValue < 0f) throw new ArgumentOutOfRangeException("labValue", "must be >= 0f");

            Experiment = experiment;
            Subject = subject;
            RecoveryValue = recoveryValue;
            TransmissionValue = transmissionValue;
            LabValue = labValue;
            Onboard = onboard;
            Available = available;
            ConditionsMet = conditionsMet;
        }


        public override string ToString()
        {
            var builder = new StringBuilder(100);

            builder.AppendFormat(
                "ExperimentSensorState: R {0}, T {1}, L {2}, Onboard {3}, Available {4}, Conditions {5}, Subject {6}",
                RecoveryValue, TransmissionValue, LabValue, Onboard, Available, ConditionsMet, Subject.Id);

            return builder.ToString();
        }
    }
}
