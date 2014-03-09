using UnityEngine;

namespace ExperimentIndicator
{
    internal class AudioController
    {
        private const string BUBBLES_PATH = "ExperimentIndicator/sounds/bubbles.wav";
        private const float LOADING_TIMEOUT = 3f;
        private const float MIN_TIME_BETWEEN_SOUNDS = 2f;

        private UnityEngine.AudioClip bubbles;
        private GameObject gameObject;
        private float timeAtLastSound = 0f;

        public enum AvailableSounds
        {
            Bubbles
        }

        public AudioController()
        {
            gameObject = new GameObject("ExperimentIndicator.AudioSource");
            gameObject.AddComponent<AudioSource>();

            bubbles = LoadSound(BUBBLES_PATH);
        }

        public bool PlaySound(AvailableSounds sound)
        {
            if (Time.realtimeSinceStartup - timeAtLastSound < MIN_TIME_BETWEEN_SOUNDS)
            {
                Log.Debug("Too early for sound");
                return false; // too early for another sound
            }

            switch (sound)
            {
                case AvailableSounds.Bubbles:
                    gameObject.transform.position = Camera.main.transform.position;
                    gameObject.audio.PlayOneShot(bubbles, GameSettings.UI_VOLUME);
                    break;

                default:
                    Log.Error("Unhandled sound in AudioController");
                    break;
            }

            return false;
        }

        private AudioClip LoadSound(string path)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);

            path = KSPUtil.ApplicationRootPath + "GameData/" + path;
            Log.Verbose("Loading sound {0}", path);

            // windows requires three slashes.  see:
            // http://docs.unity3d.com/Documentation/ScriptReference/WWW.html
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = "file:///" + path;
            }
            else path = "file://" + path;

            Log.Debug("sound path: {0}, escaped {1}", path, System.Uri.EscapeUriString(path));

            // WWW.EscapeURL doesn't seem to work all that great.  I couldn't get
            // AudioClips to come out of it correctly.  Non-escaped local urls
            // worked just fine but the docs say they should be escaped and this
            // works so I think it's the best solution currently
            //WWW clipData = new WWW(WWW.EscapeURL(path));
            WWW clipData = new WWW(System.Uri.EscapeUriString(path));

            float start = Time.realtimeSinceStartup;

            while (!clipData.isDone && Time.realtimeSinceStartup - start < LOADING_TIMEOUT)
            {
            }

            if (!clipData.isDone)
                Log.Error("Audio.LoadSounds() - timed out in {0} seconds", Time.realtimeSinceStartup - start);

            return clipData.GetAudioClip(false, false, AudioType.WAV);
        }
    }
}
