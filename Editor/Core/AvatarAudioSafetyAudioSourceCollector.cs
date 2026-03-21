using System.Collections.Generic;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyAudioSourceCollector
    {
        public static List<AudioSource> Collect(AvatarAudioSafetySettings settings)
        {
            List<AudioSource> audioSources = new List<AudioSource>();

            if (settings == null)
            {
                return audioSources;
            }

            AudioSource[] found = settings.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < found.Length; i++)
            {
                AudioSource audioSource = found[i];
                if (audioSource != null)
                {
                    audioSources.Add(audioSource);
                }
            }

            return audioSources;
        }
    }
}
