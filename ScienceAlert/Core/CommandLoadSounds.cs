using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Repositories;
using ReeperCommon.Utilities;
using ReeperKSP.AssetBundleLoading;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.Core
{
    class CommandLoadSounds : Command
    {
        private const string AlertSoundNodeName = "AlertSound";
        private const float MinTimeBetweenAlertSounds = 1f;

        private readonly GameObject _coreContext;
        private readonly IResourceRepository _resourceRepo;
        private readonly ConfigNode _config;
        private readonly ICriticalShutdownEvent _shutdownEvent;

#pragma warning disable 649
        [AssetBundleAsset("assets/sciencealert/sounds/bubbles.wav", "sciencealert.ksp", AssetBundleAssetAttribute.AssetCreationStyle.Instance)]
        private AudioClip _defaultAlertSound;

        [AssetBundleAsset("assets/sciencealert/sounds/soundsource.prefab", "sciencealert.ksp", AssetBundleAssetAttribute.AssetCreationStyle.Instance)]
        private GameObject _audioGameObject;
#pragma warning restore 649
        

        public CommandLoadSounds(
            [NotNull, Name(ContextKeys.CONTEXT_VIEW)] GameObject coreContext, 
            [NotNull] IResourceRepository resourceRepo,
            [NotNull, Name(CoreContextKeys.SoundConfig)] ConfigNode config,
            [NotNull] ICriticalShutdownEvent shutdownEvent)
        {
            if (coreContext == null) throw new ArgumentNullException("coreContext");
            if (resourceRepo == null) throw new ArgumentNullException("resourceRepo");
            if (config == null) throw new ArgumentNullException("config");
            if (shutdownEvent == null) throw new ArgumentNullException("shutdownEvent");

            _coreContext = coreContext;
            _resourceRepo = resourceRepo;
            _config = config;
            _shutdownEvent = shutdownEvent;
        }


        public override void Execute()
        {
            Retain();

            CoroutineHoster.Instance.StartCoroutine(LoadSounds());
        }


        private IEnumerator LoadSounds()
        {
            Log.Verbose("Loading sounds");

            var loadRoutine = AssetBundleAssetLoader.InjectAssetsAsync(this);
            yield return loadRoutine.YieldUntilComplete;

            bool failed = loadRoutine.Error.Any();
            var audioSource = _audioGameObject.With(go => go.GetComponent<AudioSource>());

            if (failed || (_defaultAlertSound == null || _audioGameObject == null || audioSource == null))
            {
                if (failed)
                    Log.Error("Failed to load sounds from AssetBundle: " + loadRoutine.Error.Value);
                else Log.Error("Failed to load sounds: a required asset wasn't loaded");

                Fail();
                Release();
                _shutdownEvent.Dispatch();
                yield break;
            }

            _audioGameObject.transform.NestToParent(_coreContext.transform);

            var alertSound = GetAlertSound().Or(_defaultAlertSound);

            injectionBinder.Bind<PlayableSound>().To(new PlayableSound(alertSound, audioSource, MinTimeBetweenAlertSounds)).ToName(CrossContextKeys.AlertSound).CrossContext();
            injectionBinder.Bind<AudioSource>().To(audioSource).CrossContext();

            Log.Verbose("Successfully loaded sounds");

            Release();
        }


        private Maybe<AudioClip> GetAlertSound()
        {
            if (!_config.HasValue(AlertSoundNodeName))
                return Maybe<AudioClip>.None;

            var soundUrl = _config.GetValue(AlertSoundNodeName);

            var clip = _resourceRepo.GetClip(_config.GetValue(AlertSoundNodeName));

            if (!clip.Any())
                Log.Warning("Could not find specified sound: " + soundUrl);

            return clip;
        }
    }
}
