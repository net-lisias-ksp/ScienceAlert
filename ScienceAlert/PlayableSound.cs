using System;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using UnityEngine;

namespace ScienceAlert
{
    class PlayableSound
    {
        private readonly AudioClip _clip;
        private readonly AudioSource _source;
        private readonly float _minimumTimeBetweenPlaysSeconds;

        private float _lastTimePlayed = 0f;

        public PlayableSound([NotNull] AudioClip clip, [NotNull] AudioSource source, [Name(CrossContextKeys.MinSoundDelay)] float minimumTimeBetweenPlaysSeconds)
        {
            if (clip == null) throw new ArgumentNullException("clip");
            if (source == null) throw new ArgumentNullException("source");
            if (minimumTimeBetweenPlaysSeconds < 0f)
                throw new ArgumentOutOfRangeException("minimumTimeBetweenPlaysSeconds", "must be >= 0");

            _clip = clip;
            _source = source;
            _minimumTimeBetweenPlaysSeconds = minimumTimeBetweenPlaysSeconds;
        }


        public void Play(float volumeScale = 1f)
        {
            //Log.Verbose(GetType().Name + ".Play");   

            if (_lastTimePlayed + _minimumTimeBetweenPlaysSeconds > Time.realtimeSinceStartup) return; // haven't waited long enough yet
            //Log.Verbose("Playing sound " + _clip.name);

            _source.Do(s => _clip.Do(c =>
            {
                s.PlayOneShot(c, volumeScale);
                _lastTimePlayed = Time.realtimeSinceStartup;
            }));
        }
    }
}
