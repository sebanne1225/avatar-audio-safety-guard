using UnityEditor;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyObjectActions
    {
        public static void Select(AudioSource audioSource)
        {
            if (audioSource == null)
            {
                return;
            }

            Selection.activeObject = audioSource.gameObject;
        }

        public static void Ping(AudioSource audioSource)
        {
            if (audioSource == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(audioSource.gameObject);
        }
    }
}
