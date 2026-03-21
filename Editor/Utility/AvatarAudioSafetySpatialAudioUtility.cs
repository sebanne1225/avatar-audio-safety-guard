using System;
using System.Reflection;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal readonly struct AvatarAudioSpatialAudioData
    {
        public AvatarAudioSpatialAudioData(bool hasComponent, float gain, float farDistance)
        {
            HasComponent = hasComponent;
            Gain = gain;
            FarDistance = farDistance;
        }

        public bool HasComponent { get; }

        public float Gain { get; }

        public float FarDistance { get; }
    }

    internal static class AvatarAudioSafetySpatialAudioUtility
    {
        private static readonly string[] GainMemberNames = { "Gain", "gain", "_Gain" };
        private static readonly string[] FarMemberNames = { "Far", "far", "_Far" };

        public static AvatarAudioSpatialAudioData Read(AudioSource audioSource)
        {
            if (audioSource == null)
            {
                return new AvatarAudioSpatialAudioData(false, 0f, 0f);
            }

            Component spatialAudio = audioSource.GetComponent("VRCSpatialAudioSource");
            if (spatialAudio == null)
            {
                return new AvatarAudioSpatialAudioData(false, 0f, audioSource.maxDistance);
            }

            float gain = ReadFloat(spatialAudio, GainMemberNames, 0f);
            float farDistance = ReadFloat(spatialAudio, FarMemberNames, audioSource.maxDistance);
            return new AvatarAudioSpatialAudioData(true, gain, farDistance);
        }

        public static bool TryWriteGain(AudioSource audioSource, float value)
        {
            return TryWrite(audioSource, GainMemberNames, value);
        }

        public static bool TryWriteFarDistance(AudioSource audioSource, float value)
        {
            return TryWrite(audioSource, FarMemberNames, value);
        }

        private static bool TryWrite(AudioSource audioSource, string[] memberNames, float value)
        {
            if (audioSource == null)
            {
                return false;
            }

            Component spatialAudio = audioSource.GetComponent("VRCSpatialAudioSource");
            if (spatialAudio == null)
            {
                return false;
            }

            Type type = spatialAudio.GetType();
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            for (int i = 0; i < memberNames.Length; i++)
            {
                string memberName = memberNames[i];

                PropertyInfo property = type.GetProperty(memberName, Flags);
                if (property != null && property.CanWrite && TrySetMemberValue(() => property.SetValue(spatialAudio, Convert.ChangeType(value, property.PropertyType), null)))
                {
                    return true;
                }

                FieldInfo field = type.GetField(memberName, Flags);
                if (field != null && TrySetMemberValue(() => field.SetValue(spatialAudio, Convert.ChangeType(value, field.FieldType))))
                {
                    return true;
                }
            }

            return false;
        }

        private static float ReadFloat(Component component, string[] memberNames, float fallback)
        {
            Type type = component.GetType();
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            for (int i = 0; i < memberNames.Length; i++)
            {
                string memberName = memberNames[i];

                PropertyInfo property = type.GetProperty(memberName, Flags);
                if (property != null && property.CanRead && TryConvertToSingle(property.GetValue(component, null), out float propertyValue))
                {
                    return propertyValue;
                }

                FieldInfo field = type.GetField(memberName, Flags);
                if (field != null && TryConvertToSingle(field.GetValue(component), out float fieldValue))
                {
                    return fieldValue;
                }
            }

            return fallback;
        }

        private static bool TryConvertToSingle(object value, out float number)
        {
            if (value == null)
            {
                number = 0f;
                return false;
            }

            try
            {
                number = Convert.ToSingle(value);
                return true;
            }
            catch
            {
                number = 0f;
                return false;
            }
        }

        private static bool TrySetMemberValue(Action setter)
        {
            if (setter == null)
            {
                return false;
            }

            try
            {
                setter();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
