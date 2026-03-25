using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using Object = UnityEngine.Object;
using Sebanne.AvatarAudioSafetyGuard.Editor;

namespace Sebanne.AvatarAudioSafetyGuard.Editor.NDMF
{
    internal sealed class AvatarAudioSafetySourceSettingsPreBuildHook : IVRCSDKPreprocessAvatarCallback
    {
        private const string CloneSuffix = "(Clone)";

        public int callbackOrder
        {
            get { return -12000; }
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            if (avatarGameObject == null)
            {
                return true;
            }

            AvatarAudioSafetySettings buildSettings = avatarGameObject.GetComponent<AvatarAudioSafetySettings>();
            if (buildSettings == null)
            {
                return true;
            }

            AvatarAudioSafetySettings sourceSettings = ResolveSourceSettingsCandidate(avatarGameObject, buildSettings);
            if (sourceSettings == null)
            {
                return true;
            }

            if (!AvatarAudioSafetySettingsIdUtility.TryEnsureSourceSettingsId(sourceSettings))
            {
                return true;
            }

            string resolvedId = sourceSettings.SourceSettingsGlobalId;
            if (!string.IsNullOrEmpty(resolvedId) && buildSettings.SourceSettingsGlobalId != resolvedId)
            {
                buildSettings.SetSourceSettingsGlobalId(resolvedId);
            }

            return true;
        }

        private static AvatarAudioSafetySettings ResolveSourceSettingsCandidate(
            GameObject avatarRootObject,
            AvatarAudioSafetySettings buildSettings)
        {
            AvatarAudioSafetySettings sourceSettings = TryResolveStoredSettings(buildSettings.SourceSettingsGlobalId);
            if (sourceSettings != null)
            {
                return sourceSettings;
            }

            if (avatarRootObject == null || buildSettings == null)
            {
                return null;
            }

            if (!avatarRootObject.name.EndsWith(CloneSuffix, StringComparison.Ordinal))
            {
                return buildSettings;
            }

            string sourceAvatarName = avatarRootObject.name.Substring(0, avatarRootObject.name.Length - CloneSuffix.Length);
            List<AvatarAudioSafetySettings> matches = FindSettingsCandidates(sourceAvatarName, buildSettings);
            if (matches.Count == 1)
            {
                return matches[0];
            }

            AvatarAudioSafetySettings rememberedSettings = AvatarAudioSafetySessionState.RestoreLastSettings();
            if (rememberedSettings != null)
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    if (matches[i] == rememberedSettings)
                    {
                        return rememberedSettings;
                    }
                }
            }

            return null;
        }

        private static AvatarAudioSafetySettings TryResolveStoredSettings(string rawGlobalId)
        {
            if (string.IsNullOrEmpty(rawGlobalId))
            {
                return null;
            }

            GlobalObjectId globalObjectId;
            if (!GlobalObjectId.TryParse(rawGlobalId, out globalObjectId))
            {
                return null;
            }

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) as AvatarAudioSafetySettings;
        }

        private static List<AvatarAudioSafetySettings> FindSettingsCandidates(
            string sourceAvatarName,
            AvatarAudioSafetySettings buildSettings)
        {
            List<AvatarAudioSafetySettings> matches = new List<AvatarAudioSafetySettings>();
            AvatarAudioSafetySettings[] allSettings =
                Object.FindObjectsByType<AvatarAudioSafetySettings>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < allSettings.Length; i++)
            {
                AvatarAudioSafetySettings candidate = allSettings[i];
                if (candidate == null || candidate == buildSettings)
                {
                    continue;
                }

                GameObject candidateObject = candidate.gameObject;
                if (candidateObject == null || !candidateObject.scene.IsValid() || !candidateObject.scene.isLoaded)
                {
                    continue;
                }

                if (EditorUtility.IsPersistent(candidate))
                {
                    continue;
                }

                if (!string.Equals(candidateObject.name, sourceAvatarName, StringComparison.Ordinal))
                {
                    continue;
                }

                matches.Add(candidate);
            }

            return matches;
        }
    }
}
