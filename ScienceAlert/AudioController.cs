/******************************************************************************
                   Science Alert for Kerbal Space Program                    
 ******************************************************************************
    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert
{
    //internal class AudioController
    //{
    //    private Dictionary<string /* sound name */, PlayableSound> sounds = new Dictionary<string, PlayableSound>();
    //    private const float LOADING_TIMEOUT = 5f;
        
    //    private GameObject gameObject;


    //    internal class PlayableSound
    //    {
    //        private AudioClip clip;
    //        private AudioSource source;
    //        private float lastPlayed = 0f;
    //        private Settings.SoundSettings settings;

    //        internal PlayableSound(AudioClip aclip, AudioSource player, Settings.SoundSettings ssettings)
    //        {
    //            clip = aclip;
    //            source = player;
    //            settings = ssettings;
    //        }

    //        public bool Play()
    //        {
    //            if (!source.gameObject.activeSelf)
    //                source.gameObject.SetActive(true);

    //            if (settings.Enabled)
    //                if (Time.realtimeSinceStartup - lastPlayed > settings.MinDelay)
    //                {
    //                    if (clip != null)
    //                    {
    //                        source.PlayOneShot(clip, GameSettings.UI_VOLUME);
    //                        lastPlayed = Time.realtimeSinceStartup;
    //                        Log.Debug("PlayableSound: played at {0}, next available at {1} (minDelay is {2})", lastPlayed, lastPlayed + settings.MinDelay, settings.MinDelay);
    //                        return true;
    //                    }
    //                    else
    //                    {
    //                        Log.Error("Cannot play sound; clip is null!");
    //                        return false;
    //                    }
    //                }
    //                else
    //                {
    //                    // too early for this sound
    //                    //Log.Verbose("Too early to play {0}", clip.name);
    //                    return false;
    //                }

    //            Log.Verbose("Cannot play sound; this sound is disabled");
    //            return false; // sound disabled
    //        }
    //    }


    //    public AudioController()
    //    {
    //        gameObject = new GameObject("ScienceAlert.AudioSource");
    //        gameObject.AddComponent<AudioSource>();
    //        gameObject.transform.parent = Camera.main.transform;


    //        string soundDir = KSPUtil.ApplicationRootPath + "GameData/ScienceAlert/sounds/";

    //        string[] files = Directory.GetFiles(soundDir, "*.wav");

    //        Log.Debug("Found {0} files in directory {1}", files.Length, soundDir);
    //        foreach (var f in files)
    //            Log.Debug("File: {0}", f);

    //        foreach (var file in files)
    //        {
    //            AudioClip newClip = LoadSound(file, false);
    //            if (newClip != null)
    //            {
    //                string name = file.Substring(file.LastIndexOf('/') + 1);
    //                if (name.EndsWith(".wav"))
    //                    name = name.Substring(0, name.Length - ".wav".Length);

    //                Log.Debug("Loaded sound; adding as {0}", name);

    //                try
    //                {
    //                    sounds.Add(name, new PlayableSound(newClip, gameObject.audio, Settings.Instance.GetSoundSettings(name)));
    //                } catch (Exception e)
    //                {
    //                    Log.Error("AudioController: cannot add {0} due to {1}", name, e);
    //                }
    //            }
    //        }

    //        Log.Debug("The following sounds are loaded:");
    //        foreach (var kvp in sounds)
    //            Log.Debug("Sound: {0}", kvp.Key);
    //    }


    //    public bool PlaySound(string name)
    //    {
    //        if (!sounds.ContainsKey(name))
    //        {
    //            Log.Error("Cannot play '{0}'; does not exist!", name);
    //            return false;
    //        }
    //        else
    //        {
    //            if (sounds[name].Play())
    //            {
    //                Log.Debug("Played '{0}'", name);
    //                return true;
    //            }
    //            else return false;
    //        }
    //    }



    //    /// <summary>
    //    /// The GameDatabase audio clips all seem to be intended for use with
    //    /// 3D. It causes a problem with our UI sounds because the player's
    //    /// viewpoint is moving. Even if we attach an audio source to the
    //    /// player camera, strange effects due to that movement (like much
    //    /// louder in one ear in certain orientations) seem to occur.
    //    /// 
    //    /// This allows us to load the sounds ourselves with the parameters
    //    /// we want.
    //    /// </summary>
    //    /// <param name="path"></param>
    //    /// <returns></returns>
    //    private AudioClip LoadSound(string path, bool relativeToGameData = true)
    //    {
    //        if (relativeToGameData)
    //        {
    //            if (path.StartsWith("/"))
    //                path = path.Substring(1);

    //            path = KSPUtil.ApplicationRootPath + "GameData/" + path;
    //        }
    //        Log.Verbose("Loading sound {0}", path);

    //        // windows requires three slashes.  see:
    //        // http://docs.unity3d.com/Documentation/ScriptReference/WWW.html
    //        if (Application.platform == RuntimePlatform.WindowsPlayer)
    //        {
    //            if (!path.StartsWith("file:///"))
    //                path = "file:///" + path;
    //        }
    //        else if (!path.StartsWith("file://")) path = "file://" + path;

    //        Log.Debug("sound path: {0}, escaped {1}", path, System.Uri.EscapeUriString(path));

    //        // WWW.EscapeURL doesn't seem to work all that great.  I couldn't get
    //        // AudioClips to come out of it correctly.  Non-escaped local urls
    //        // worked just fine but the docs say they should be escaped and this
    //        // works so I think it's the best solution currently
    //        //WWW clipData = new WWW(WWW.EscapeURL(path));
    //        WWW clipData = new WWW(System.Uri.EscapeUriString(path));

    //        float start = Time.realtimeSinceStartup;

    //        while (!clipData.isDone && Time.realtimeSinceStartup - start < LOADING_TIMEOUT)
    //        {
    //        }

    //        if (!clipData.isDone)
    //            Log.Error("Audio.LoadSounds() - timed out in {0} seconds", Time.realtimeSinceStartup - start);

    //        return clipData.GetAudioClip(false, false, AudioType.WAV);
    //    }
    //}
}
