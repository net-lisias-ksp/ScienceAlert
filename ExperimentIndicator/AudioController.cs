using UnityEngine;
using DebugTools;

namespace ExperimentIndicator
{
    internal class AudioController
    {
        private const string BUBBLES_PATH = "ExperimentIndicator/sounds/bubbles.wav";
        private const string CLICK_PATH = "ExperimentIndicator/sounds/click1.wav";

        private const float LOADING_TIMEOUT = 3f;
        private const float MIN_TIME_BETWEEN_SOUNDS = 2f;

        private UnityEngine.AudioClip bubbles;
        private UnityEngine.AudioClip click;

        private GameObject gameObject;
        private float timeAtLastSound = 0f;

        public enum AvailableSounds
        {
            Bubbles,
            UIClick // does not trigger minimum time counter
        }



        public AudioController()
        {
            gameObject = new GameObject("ExperimentIndicator.AudioSource");
            gameObject.AddComponent<AudioSource>();

            bubbles = LoadSound(BUBBLES_PATH);
            click = LoadSound(CLICK_PATH);
        }



        public bool PlaySound(AvailableSounds sound)
        {
            if (Time.realtimeSinceStartup - timeAtLastSound < MIN_TIME_BETWEEN_SOUNDS)
            {
                Log.Debug("Too early for sound");
                return false; // too early for another sound
            }

            try
            {
                gameObject.transform.position = Camera.main.transform.position;
                timeAtLastSound = Time.realtimeSinceStartup;

                switch (sound)
                {
                    case AvailableSounds.Bubbles:
                        gameObject.audio.PlayOneShot(bubbles, GameSettings.UI_VOLUME);

                        return true;

                    case AvailableSounds.UIClick:
                        gameObject.audio.PlayOneShot(click, GameSettings.UI_VOLUME);
                        timeAtLastSound = 0f; // don't trigger countodwn
                        return true;

                    default:
                        Log.Error("Unhandled sound in AudioController");
                        break;
                }
            } catch 
            {
                Log.Error("Error playing sound {0}", sound);
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
