using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ReeperKSP.Serialization;
using ScienceAlert.UI.ExperimentWindow;
using ScienceAlert.UI.OptionsWindow;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    // ReSharper disable once UnusedMember.Global
    public class WindowSerializationSurrogate : 
        IConfigNodeItemSerializer<OptionsWindowView>,
        IConfigNodeItemSerializer<ExperimentWindowView>
    {
        // Returns true if target type matches one of the types this surrogate is for
        private static bool IsTypeHandled(Type target)
        {
            return typeof (WindowSerializationSurrogate).GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigNodeItemSerializer<>))
                .SelectMany(surrogateInterface => surrogateInterface.GetGenericArguments())
                .Any(handledType => target == handledType);
        }


        public void Serialize([NotNull] Type type, ref object target, string key, [NotNull] ConfigNode config,
            [NotNull] IConfigNodeSerializer serializer)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!IsTypeHandled(type))
                throw new ArgumentException("wrong serializer for " + type.Name);
            if (target == null) return;
            if (config == null) throw new ArgumentNullException("config");
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("must not be empty or null", "key");

            var mbTarget = target as MonoBehaviour;

            if (mbTarget == null)
            {
                Log.Warning("Cannot serialize " + type.Name + " because its MonoBehaviour has been destroyed");
                return;
            }

            var rtToSerialize = mbTarget.GetComponent<RectTransform>();

            if (rtToSerialize == null)
                throw new ArgumentException("Expected an attached RectTransform on " + type.Name);

            config.Set("position", KSPUtil.WriteVector(rtToSerialize.anchoredPosition));
            config.Set("size", KSPUtil.WriteVector(rtToSerialize.sizeDelta));
            config.Set("visible", mbTarget.gameObject.activeSelf);
        }


        public void Deserialize(Type type, ref object target, string key, ConfigNode config,
            IConfigNodeSerializer serializer)
        {
            Log.Warning(GetType().Name + " deserialize");
            Log.Warning("Deserializing from: " + config.ToSafeString());

            if (type == null) throw new ArgumentNullException("type");
            if (!IsTypeHandled(type))
                throw new ArgumentException("wrong deserializer for " + type.Name);
            if (target == null) return;
            if (config == null) throw new ArgumentNullException("config");
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("must not be empty or null", "key");

            var rtToDeserialize =
                target.With(tar => tar as MonoBehaviour).With(mb => mb.gameObject.GetComponent<RectTransform>());

            if (rtToDeserialize == null)
                throw new ArgumentException("Expected an attached RectTransform on " + type.Name);

            config.GetValueEx("position")
                .With(KSPUtil.ParseVector2)
                .Do(v => rtToDeserialize.anchoredPosition = v);

            config.GetValueEx("size")
                .With(KSPUtil.ParseVector2)
                .Do(s => rtToDeserialize.sizeDelta = s);

            config.GetValueEx("visible")
                .With<string, bool>(s =>
                {
                    bool result;
                    return !Boolean.TryParse(s, out result) || result;
                })
                .Do(b => rtToDeserialize.gameObject.SetActive(b));
        }
    }
}
