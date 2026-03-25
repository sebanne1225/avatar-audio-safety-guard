using System.Collections.Generic;
using UnityEditor;
using Sebanne.AvatarAudioSafetyGuard;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal sealed class AvatarAudioSafetyCachedBuildResult
    {
        public AvatarAudioSafetyBuildResultSnapshot snapshot = new AvatarAudioSafetyBuildResultSnapshot();
        public string sourceSettingsName = string.Empty;
        public AvatarAudioSafetySettings sourceSettingsObject;
        public string sourceAvatarRootName = string.Empty;
        public GameObject sourceAvatarRootObject;
        public List<GameObject> sourceEntryTargetObjects = new List<GameObject>();
    }

    internal static class AvatarAudioSafetySessionState
    {
        private const string LastSettingsKey = "Sebanne.AvatarAudioSafetyGuard.LastSettings";
        private const string LastBuildResultKey = "Sebanne.AvatarAudioSafetyGuard.LastBuildResult";
        private const string LastBuildResultVersionKey = "Sebanne.AvatarAudioSafetyGuard.LastBuildResult.Version";

        [System.Serializable]
        private sealed class BuildResultSourceHeaderPayload
        {
            public string settingsName = string.Empty;
            public string settingsGlobalId = string.Empty;
            public string avatarRootName = string.Empty;
            public string avatarRootGlobalId = string.Empty;
        }

        [System.Serializable]
        private sealed class BuildResultSessionPayload
        {
            public BuildResultSourceHeaderPayload sourceHeader = new BuildResultSourceHeaderPayload();
            public string settingsName = string.Empty;
            public string avatarRootName = string.Empty;
            public string avatarRootGlobalId = string.Empty;
            public List<string> entryTargetObjectGlobalIds = new List<string>();
            public List<string> entrySourceAnchorPaths = new List<string>();
            public AvatarAudioSafetyBuildResultSnapshot snapshot = new AvatarAudioSafetyBuildResultSnapshot();
        }

        public static void RememberSettings(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return;
            }

            // Keep the last known edit-mode settings reference intact while playing.
            if (EditorApplication.isPlaying)
            {
                return;
            }

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(settings);
            SessionState.SetString(LastSettingsKey, globalObjectId.ToString());
        }

        public static AvatarAudioSafetySettings RestoreLastSettings()
        {
            string raw = SessionState.GetString(LastSettingsKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }

            GlobalObjectId globalObjectId;
            if (!GlobalObjectId.TryParse(raw, out globalObjectId))
            {
                return null;
            }

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) as AvatarAudioSafetySettings;
        }

        public static void RememberBuildResult(
            AvatarAudioSafetySettings sourceSettings,
            AvatarAudioSafetySettings buildSettings,
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            IReadOnlyList<string> sourceAnchorPaths)
        {
            if (buildSettings == null || snapshot == null)
            {
                return;
            }

            BuildResultSessionPayload payload = new BuildResultSessionPayload();
            payload.sourceHeader = CreateSourceHeader(sourceSettings, buildSettings);
            payload.snapshot = snapshot.Clone();
            CacheBuildSourceAnchorPaths(payload.entrySourceAnchorPaths, sourceAnchorPaths, snapshot);
            CacheBuildTargetObjectIds(payload.entryTargetObjectGlobalIds, sourceSettings, payload.entrySourceAnchorPaths);

            SessionState.SetString(LastBuildResultKey, JsonUtility.ToJson(payload));
            SessionState.SetInt(LastBuildResultVersionKey, GetLastBuildResultVersion() + 1);
        }

        public static int GetLastBuildResultVersion()
        {
            return SessionState.GetInt(LastBuildResultVersionKey, 0);
        }

        public static bool TryRestoreLastBuildResult(out AvatarAudioSafetyCachedBuildResult cachedResult)
        {
            cachedResult = null;

            string raw = SessionState.GetString(LastBuildResultKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return false;
            }

            BuildResultSessionPayload payload = JsonUtility.FromJson<BuildResultSessionPayload>(raw);
            if (payload == null || payload.snapshot == null || !payload.snapshot.HasData)
            {
                return false;
            }

            cachedResult = new AvatarAudioSafetyCachedBuildResult();
            cachedResult.snapshot = payload.snapshot.Clone();
            BuildResultSourceHeaderPayload sourceHeader = ResolveSourceHeader(payload);
            cachedResult.sourceSettingsName = sourceHeader != null ? sourceHeader.settingsName ?? string.Empty : string.Empty;
            cachedResult.sourceSettingsObject = TryResolveGlobalObject<AvatarAudioSafetySettings>(
                sourceHeader != null ? sourceHeader.settingsGlobalId : string.Empty);
            cachedResult.sourceAvatarRootName = sourceHeader != null ? sourceHeader.avatarRootName ?? string.Empty : string.Empty;
            cachedResult.sourceAvatarRootObject = ResolveAvatarRootObject(sourceHeader, cachedResult.sourceSettingsObject);
            cachedResult.sourceEntryTargetObjects = RestoreEntryTargetObjects(
                payload.entryTargetObjectGlobalIds,
                payload.entrySourceAnchorPaths,
                cachedResult.sourceAvatarRootObject,
                cachedResult.snapshot.entries != null ? cachedResult.snapshot.entries.Count : 0);
            return true;
        }

        public static void ClearRememberedBuildResult()
        {
            SessionState.EraseString(LastBuildResultKey);
            SessionState.SetInt(LastBuildResultVersionKey, GetLastBuildResultVersion() + 1);
        }

        private static void CacheBuildTargetObjectIds(
            List<string> targetIds,
            AvatarAudioSafetySettings sourceSettings,
            IReadOnlyList<string> sourceAnchorPaths)
        {
            if (targetIds == null)
            {
                return;
            }

            targetIds.Clear();

            int expectedCount = sourceAnchorPaths != null ? sourceAnchorPaths.Count : 0;
            if (sourceSettings == null || sourceSettings.transform == null || expectedCount <= 0)
            {
                return;
            }

            for (int i = 0; i < expectedCount; i++)
            {
                string sourceAnchorPath = sourceAnchorPaths[i];
                GameObject targetObject = ResolveSourceTargetObject(sourceSettings.transform, sourceAnchorPath);
                targetIds.Add(TryGetGlobalObjectIdString(targetObject));
            }
        }

        private static void CacheBuildSourceAnchorPaths(
            List<string> cachedAnchorPaths,
            IReadOnlyList<string> sourceAnchorPaths,
            AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (cachedAnchorPaths == null)
            {
                return;
            }

            cachedAnchorPaths.Clear();

            int expectedCount = snapshot != null && snapshot.entries != null ? snapshot.entries.Count : 0;
            for (int i = 0; i < expectedCount; i++)
            {
                string anchorPath = sourceAnchorPaths != null && i < sourceAnchorPaths.Count
                    ? AvatarAudioSafetyPathUtility.Normalize(sourceAnchorPaths[i])
                    : string.Empty;
                cachedAnchorPaths.Add(anchorPath);
            }
        }

        private static GameObject ResolveSourceTargetObject(Transform avatarRoot, string sourceAnchorPath)
        {
            if (avatarRoot == null || string.IsNullOrEmpty(sourceAnchorPath))
            {
                return null;
            }

            Transform target = AvatarAudioSafetyPathUtility.ResolveRelativePath(avatarRoot, sourceAnchorPath);
            return target != null ? target.gameObject : null;
        }

        private static string TryGetGlobalObjectIdString(Object targetObject)
        {
            if (targetObject == null)
            {
                return string.Empty;
            }

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(targetObject);
            if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) != targetObject)
            {
                return string.Empty;
            }

            return globalObjectId.ToString();
        }

        private static T TryResolveGlobalObject<T>(string rawGlobalId) where T : Object
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

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) as T;
        }

        private static List<GameObject> RestoreEntryTargetObjects(
            List<string> targetIds,
            List<string> sourceAnchorPaths,
            GameObject sourceAvatarRootObject,
            int expectedCount)
        {
            List<GameObject> restoredTargets = new List<GameObject>(expectedCount);
            Transform sourceAvatarRoot = sourceAvatarRootObject != null ? sourceAvatarRootObject.transform : null;

            for (int i = 0; i < expectedCount; i++)
            {
                string rawGlobalId = targetIds != null && i < targetIds.Count ? targetIds[i] : string.Empty;
                GameObject targetObject = TryResolveGlobalObject<GameObject>(rawGlobalId);
                if (targetObject == null)
                {
                    string sourceAnchorPath = sourceAnchorPaths != null && i < sourceAnchorPaths.Count ? sourceAnchorPaths[i] : string.Empty;
                    targetObject = ResolveSourceTargetObject(sourceAvatarRoot, sourceAnchorPath);
                }

                restoredTargets.Add(targetObject);
            }

            return restoredTargets;
        }

        private static BuildResultSourceHeaderPayload CreateSourceHeader(
            AvatarAudioSafetySettings sourceSettings,
            AvatarAudioSafetySettings buildSettings)
        {
            AvatarAudioSafetySettings headerSettings = sourceSettings;
            GameObject headerAvatarRoot = headerSettings != null ? headerSettings.gameObject : null;

            BuildResultSourceHeaderPayload header = new BuildResultSourceHeaderPayload();
            header.settingsName = headerSettings != null ? headerSettings.name : typeof(AvatarAudioSafetySettings).Name;
            header.settingsGlobalId = TryGetGlobalObjectIdString(headerSettings);
            header.avatarRootName = headerAvatarRoot != null
                ? headerAvatarRoot.name
                : NormalizeAvatarName(buildSettings != null && buildSettings.gameObject != null ? buildSettings.gameObject.name : string.Empty);
            header.avatarRootGlobalId = TryGetGlobalObjectIdString(headerAvatarRoot);
            return header;
        }

        private static BuildResultSourceHeaderPayload ResolveSourceHeader(BuildResultSessionPayload payload)
        {
            if (payload == null)
            {
                return null;
            }

            if (payload.sourceHeader != null
                && (!string.IsNullOrEmpty(payload.sourceHeader.settingsName)
                    || !string.IsNullOrEmpty(payload.sourceHeader.settingsGlobalId)
                    || !string.IsNullOrEmpty(payload.sourceHeader.avatarRootName)
                    || !string.IsNullOrEmpty(payload.sourceHeader.avatarRootGlobalId)))
            {
                return payload.sourceHeader;
            }

            if (string.IsNullOrEmpty(payload.settingsName)
                && string.IsNullOrEmpty(payload.avatarRootName)
                && string.IsNullOrEmpty(payload.avatarRootGlobalId))
            {
                return payload.sourceHeader;
            }

            BuildResultSourceHeaderPayload legacyHeader = new BuildResultSourceHeaderPayload();
            legacyHeader.settingsName = payload.settingsName ?? string.Empty;
            legacyHeader.avatarRootName = payload.avatarRootName ?? string.Empty;
            legacyHeader.avatarRootGlobalId = payload.avatarRootGlobalId ?? string.Empty;
            return legacyHeader;
        }

        private static GameObject ResolveAvatarRootObject(
            BuildResultSourceHeaderPayload sourceHeader,
            AvatarAudioSafetySettings sourceSettingsObject)
        {
            if (sourceSettingsObject != null && sourceSettingsObject.gameObject != null)
            {
                return sourceSettingsObject.gameObject;
            }

            return TryResolveGlobalObject<GameObject>(sourceHeader != null ? sourceHeader.avatarRootGlobalId : string.Empty);
        }

        private static string NormalizeAvatarName(string avatarName)
        {
            const string CloneSuffix = "(Clone)";
            if (!string.IsNullOrEmpty(avatarName) && avatarName.EndsWith(CloneSuffix, System.StringComparison.Ordinal))
            {
                return avatarName.Substring(0, avatarName.Length - CloneSuffix.Length);
            }

            return avatarName ?? string.Empty;
        }
    }
}
