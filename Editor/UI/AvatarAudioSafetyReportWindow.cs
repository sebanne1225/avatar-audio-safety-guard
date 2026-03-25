using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal sealed class AvatarAudioSafetyReportWindow : EditorWindow
    {
        private enum ResultFilter
        {
            All = 0,
            IssuesOnly = 1,
            WouldClamp = 2,
            ReportOnly = 3,
            Ignored = 4,
        }

        [SerializeField]
        private AvatarAudioSafetySettings settings;

        [SerializeField]
        private ResultFilter filter = ResultFilter.All;

        [SerializeField]
        private Vector2 scrollPosition;

        [SerializeField]
        private Vector2 buildResultScrollPosition;

        [SerializeField]
        private Vector2 windowScrollPosition;

        [SerializeField]
        private bool buildResultFoldout;

        [SerializeField]
        private AvatarAudioSafetyBuildResultSnapshot cachedBuildResult = new AvatarAudioSafetyBuildResultSnapshot();

        [SerializeField]
        private string cachedSourceSettingsName = string.Empty;

        [SerializeField]
        private AvatarAudioSafetySettings cachedSourceSettingsObject;

        [SerializeField]
        private string cachedSourceAvatarRootName = string.Empty;

        [SerializeField]
        private int cachedBuildResultSessionVersion = -1;

        [SerializeField]
        private GameObject cachedSourceAvatarRootObject;

        [SerializeField]
        private List<GameObject> cachedSourceBuildTargetObjects = new List<GameObject>();

        [SerializeField]
        private string cachedBuildTargetSnapshotKey = string.Empty;

        private void OnEnable()
        {
            titleContent = new GUIContent(AvatarAudioSafetyUiText.ReportWindowTitle);
            minSize = new Vector2(760f, 380f);
            RestoreSettingsReference();
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        public static void Open(AvatarAudioSafetySettings settings)
        {
            AvatarAudioSafetyReportWindow window = GetWindow<AvatarAudioSafetyReportWindow>();
            window.settings = settings;
            AvatarAudioSafetySessionState.RememberSettings(settings);
            window.Show();
        }

        public static void Open()
        {
            AvatarAudioSafetyReportWindow window = GetWindow<AvatarAudioSafetyReportWindow>();
            window.RestoreSettingsReference();
            window.Show();
            window.Focus();
        }

        internal static void RepaintOpenWindows()
        {
            AvatarAudioSafetyReportWindow[] windows = Resources.FindObjectsOfTypeAll<AvatarAudioSafetyReportWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                AvatarAudioSafetyReportWindow window = windows[i];
                if (window != null)
                {
                    window.Repaint();
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            RestoreSettingsReference();
            windowScrollPosition = EditorGUILayout.BeginScrollView(windowScrollPosition);
            DrawTargetSection();

            if (settings == null)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.MissingSettingsMessage, MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            AvatarAudioSafetySessionState.RememberSettings(settings);

            EditorGUILayout.Space();
            DrawFilterSection();
            AvatarAudioSafetyResultTable.Draw(settings.DetectedAudioSources, ref scrollPosition, FilterResult, false, settings);
            EditorGUILayout.EndScrollView();
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    AvatarAudioSafetySessionState.RememberSettings(settings);
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    settings = AvatarAudioSafetySessionState.RestoreLastSettings();
                    if (settings == null)
                    {
                        RestoreSettingsReference();
                    }
                    Repaint();
                    break;
            }
        }

        private void RestoreSettingsReference()
        {
            if (settings != null)
            {
                return;
            }

            settings = AvatarAudioSafetySessionState.RestoreLastSettings();

            if (settings == null && Selection.activeGameObject != null)
            {
                settings = Selection.activeGameObject.GetComponentInParent<AvatarAudioSafetySettings>();
            }
        }

        private void EnsureCacheDefaults()
        {
            if (cachedBuildResult == null)
            {
                cachedBuildResult = new AvatarAudioSafetyBuildResultSnapshot();
            }

            if (cachedSourceBuildTargetObjects == null)
            {
                cachedSourceBuildTargetObjects = new List<GameObject>();
            }
        }

        private void RefreshBuildResultCacheFromSession()
        {
            int sessionVersion = AvatarAudioSafetySessionState.GetLastBuildResultVersion();
            if (sessionVersion == cachedBuildResultSessionVersion)
            {
                return;
            }

            cachedBuildResultSessionVersion = sessionVersion;

            AvatarAudioSafetyCachedBuildResult cachedResult;
            if (!AvatarAudioSafetySessionState.TryRestoreLastBuildResult(out cachedResult))
            {
                cachedBuildResult.Reset();
                cachedSourceSettingsName = string.Empty;
                cachedSourceSettingsObject = null;
                cachedSourceAvatarRootName = string.Empty;
                cachedSourceAvatarRootObject = null;
                cachedSourceBuildTargetObjects.Clear();
                cachedBuildTargetSnapshotKey = string.Empty;
                return;
            }

            cachedBuildResult = cachedResult.snapshot;
            cachedSourceSettingsName = cachedResult.sourceSettingsName;
            cachedSourceSettingsObject = cachedResult.sourceSettingsObject;
            cachedSourceAvatarRootName = cachedResult.sourceAvatarRootName;
            cachedSourceAvatarRootObject = cachedResult.sourceAvatarRootObject;
            cachedSourceBuildTargetObjects = cachedResult.sourceEntryTargetObjects ?? new List<GameObject>();
            cachedBuildTargetSnapshotKey = GetBuildSnapshotCacheKey(cachedBuildResult);
        }

        private void CacheVisibleStateFromLiveIfNeeded()
        {
            if (settings == null)
            {
                return;
            }

            EnsureCacheDefaults();
            cachedSourceSettingsName = settings.name;
            cachedSourceSettingsObject = settings;
            cachedSourceAvatarRootName = settings.gameObject != null ? settings.gameObject.name : string.Empty;
            cachedSourceAvatarRootObject = settings.gameObject;

            AvatarAudioSafetyBuildResultSnapshot liveSnapshot = settings.LastBuildResult;
            if (!ShouldPromoteLiveBuildResult(liveSnapshot))
            {
                return;
            }

            cachedBuildResult = liveSnapshot != null
                ? liveSnapshot.Clone()
                : new AvatarAudioSafetyBuildResultSnapshot();

            string visibleSnapshotKey = GetBuildSnapshotCacheKey(liveSnapshot);
            if (cachedBuildTargetSnapshotKey != visibleSnapshotKey)
            {
                cachedSourceBuildTargetObjects.Clear();
            }

            cachedBuildTargetSnapshotKey = visibleSnapshotKey;
        }

        private bool HasCachedBuildResult()
        {
            return cachedBuildResult != null && cachedBuildResult.HasData;
        }

        private AvatarAudioSafetyBuildResultSnapshot GetVisibleBuildResultSnapshot(out bool usingCachedSnapshot)
        {
            AvatarAudioSafetyBuildResultSnapshot liveSnapshot = settings != null ? settings.LastBuildResult : null;

            if (ShouldUseCachedBuildResult(liveSnapshot))
            {
                usingCachedSnapshot = true;
                return cachedBuildResult;
            }

            usingCachedSnapshot = false;
            return liveSnapshot;
        }

        private bool ShouldUseCachedBuildResult(AvatarAudioSafetyBuildResultSnapshot liveSnapshot)
        {
            if (!HasCachedBuildResult())
            {
                return false;
            }

            if (settings == null)
            {
                return true;
            }

            if (!IsCachedBuildResultForCurrentSettings())
            {
                return false;
            }

            if (liveSnapshot == null || !liveSnapshot.HasData)
            {
                return true;
            }

            string liveSnapshotKey = GetBuildSnapshotCacheKey(liveSnapshot);
            string cachedSnapshotKey = GetBuildSnapshotCacheKey(cachedBuildResult);
            if (string.Equals(liveSnapshotKey, cachedSnapshotKey, StringComparison.Ordinal))
            {
                return false;
            }

            return IsSnapshotNewer(cachedBuildResult, liveSnapshot);
        }

        private bool ShouldPromoteLiveBuildResult(AvatarAudioSafetyBuildResultSnapshot liveSnapshot)
        {
            if (!HasCachedBuildResult())
            {
                return true;
            }

            if (!IsCachedBuildResultForCurrentSettings())
            {
                return true;
            }

            if (liveSnapshot == null || !liveSnapshot.HasData)
            {
                return false;
            }

            string liveSnapshotKey = GetBuildSnapshotCacheKey(liveSnapshot);
            string cachedSnapshotKey = GetBuildSnapshotCacheKey(cachedBuildResult);
            if (string.Equals(liveSnapshotKey, cachedSnapshotKey, StringComparison.Ordinal))
            {
                return true;
            }

            return !IsSnapshotNewer(cachedBuildResult, liveSnapshot);
        }

        private bool IsCachedBuildResultForCurrentSettings()
        {
            if (settings == null)
            {
                return true;
            }

            if (cachedSourceSettingsObject != null)
            {
                return cachedSourceSettingsObject == settings;
            }

            if (cachedSourceAvatarRootObject != null && settings.gameObject != null)
            {
                return cachedSourceAvatarRootObject == settings.gameObject;
            }

            return false;
        }

        private static bool IsSnapshotNewer(
            AvatarAudioSafetyBuildResultSnapshot candidateSnapshot,
            AvatarAudioSafetyBuildResultSnapshot baselineSnapshot)
        {
            if (candidateSnapshot == null || !candidateSnapshot.HasData)
            {
                return false;
            }

            if (baselineSnapshot == null || !baselineSnapshot.HasData)
            {
                return true;
            }

            string candidateTime = candidateSnapshot.executedLocalTime ?? string.Empty;
            string baselineTime = baselineSnapshot.executedLocalTime ?? string.Empty;
            return string.CompareOrdinal(candidateTime, baselineTime) > 0;
        }

        private IReadOnlyList<GameObject> GetVisibleBuildRuleTargets(AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.HasData)
            {
                return null;
            }

            string visibleSnapshotKey = GetBuildSnapshotCacheKey(snapshot);
            if (string.IsNullOrEmpty(visibleSnapshotKey) || visibleSnapshotKey != cachedBuildTargetSnapshotKey)
            {
                return null;
            }

            return cachedSourceBuildTargetObjects;
        }

        private AvatarAudioSafetySettings GetVisibleBuildRuleSettings(
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            IReadOnlyList<GameObject> visibleCachedTargetObjects)
        {
            if (snapshot == null || !snapshot.HasData || visibleCachedTargetObjects == null)
            {
                return null;
            }

            if (settings != null && IsCachedBuildResultForCurrentSettings())
            {
                return settings;
            }

            return cachedSourceSettingsObject;
        }

        private static string GetBuildSnapshotCacheKey(AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.HasData)
            {
                return string.Empty;
            }

            int entryCount = snapshot.entries != null ? snapshot.entries.Count : 0;
            return string.Format(
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                snapshot.executedLocalTime,
                snapshot.avatarName,
                (int)snapshot.mode,
                snapshot.scanned,
                snapshot.changed,
                snapshot.skipped,
                snapshot.unchanged,
                snapshot.errors,
                entryCount);
        }

        private void DrawTargetSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.ReportTargetSectionTitle, EditorStyles.boldLabel);
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.ReportTargetDescription, EditorStyles.wordWrappedMiniLabel);

            if (settings != null)
            {
                settings = (AvatarAudioSafetySettings)EditorGUILayout.ObjectField(AvatarAudioSafetyUiText.SettingsLabel, settings, typeof(AvatarAudioSafetySettings), true);

                if (settings != null)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField(
                            AvatarAudioSafetyUiText.AvatarRootLabel,
                            settings.gameObject,
                            typeof(GameObject),
                            true);
                    }
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    if (cachedSourceSettingsObject != null)
                    {
                        EditorGUILayout.ObjectField(
                            AvatarAudioSafetyUiText.SettingsLabel,
                            cachedSourceSettingsObject,
                            typeof(AvatarAudioSafetySettings),
                            true);
                    }
                    else
                    {
                        EditorGUILayout.TextField(AvatarAudioSafetyUiText.SettingsLabel, cachedSourceSettingsName);
                    }

                    if (cachedSourceAvatarRootObject != null)
                    {
                        EditorGUILayout.ObjectField(
                            AvatarAudioSafetyUiText.AvatarRootLabel,
                            cachedSourceAvatarRootObject,
                            typeof(GameObject),
                            true);
                    }
                    else
                    {
                        EditorGUILayout.TextField(AvatarAudioSafetyUiText.AvatarRootLabel, cachedSourceAvatarRootName);
                    }
                }
            }

            if (settings != null)
            {
                EditorGUILayout.Space(2f);
                using (new EditorGUI.DisabledScope(settings == null))
                {
                    if (GUILayout.Button(AvatarAudioSafetyUiText.ScanAudioSourcesButton))
                    {
                        if (AvatarAudioSafetyScanActions.RunDryRun(settings))
                        {
                            CacheVisibleStateFromLiveIfNeeded();
                            AvatarAudioSafetyUiFeedback.ShowInfoDialog(
                                AvatarAudioSafetyUiText.GetScanCompletedDialogMessage(settings.DetectedAudioSources));
                        }
                    }
                }
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(AvatarAudioSafetyUiText.SummaryLabel, AvatarAudioSafetyUiText.GetSummaryText(settings.LastScanSummary), EditorStyles.wordWrappedLabel);

                if (settings.LastScanSummary != null && !string.IsNullOrEmpty(settings.LastScanSummary.lastScanLocalTime))
                {
                    EditorGUILayout.LabelField(AvatarAudioSafetyUiText.LastScanLabel, settings.LastScanSummary.lastScanLocalTime);
                }

                DrawGuidance(
                    AvatarAudioSafetyUiText.GetDryRunBehaviorDescription(settings.ToolEnabled, settings.Mode),
                    AvatarAudioSafetyUiText.GetDryRunBehaviorMessageType(settings.ToolEnabled, settings.Mode));
                EditorGUILayout.LabelField(AvatarAudioSafetyUiText.ReportUsesStoredResultsDescription, EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFilterSection()
        {
            if (filter != ResultFilter.All
                && filter != ResultFilter.IssuesOnly
                && filter != ResultFilter.WouldClamp
                && filter != ResultFilter.ReportOnly
                && filter != ResultFilter.Ignored)
            {
                filter = ResultFilter.IssuesOnly;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.ReportFilterSectionTitle, EditorStyles.boldLabel);
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.ReportWindowDescription, EditorStyles.wordWrappedMiniLabel);
            filter = (ResultFilter)GUILayout.Toolbar((int)filter, AvatarAudioSafetyUiText.ReportFilterOptions);
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.FilterDescription, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private static void DrawGuidance(string text, MessageType messageType)
        {
            if (messageType == MessageType.None)
            {
                EditorGUILayout.LabelField(text, EditorStyles.wordWrappedMiniLabel);
                return;
            }

            EditorGUILayout.HelpBox(text, messageType);
        }

        private bool FilterResult(AvatarAudioScanResult result)
        {
            switch (filter)
            {
                case ResultFilter.IssuesOnly:
                    return result.result != AvatarAudioSafetyResultKind.Safe;
                case ResultFilter.WouldClamp:
                    return result.result == AvatarAudioSafetyResultKind.WouldClamp;
                case ResultFilter.ReportOnly:
                    return result.result == AvatarAudioSafetyResultKind.ReportOnly;
                case ResultFilter.Ignored:
                    return result.result == AvatarAudioSafetyResultKind.Ignored;
                case ResultFilter.All:
                default:
                    return true;
            }
        }

    }
}
