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
            ManualReview = 5,
        }

        [SerializeField]
        private AvatarAudioSafetySettings settings;

        [SerializeField]
        private ResultFilter filter = ResultFilter.All;

        [SerializeField]
        private Vector2 scrollPosition;

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

        private void OnGUI()
        {
            EditorGUILayout.Space();
            RestoreSettingsReference();
            DrawTargetSection();

            if (settings == null)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.MissingSettingsMessage, MessageType.Info);
                return;
            }

            AvatarAudioSafetySessionState.RememberSettings(settings);

            EditorGUILayout.Space();
            DrawFilterSection();
            AvatarAudioSafetyResultTable.Draw(settings.DetectedAudioSources, ref scrollPosition, FilterResult);
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

        private void DrawTargetSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.ReportTargetSectionTitle, EditorStyles.boldLabel);
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.ReportTargetDescription, EditorStyles.wordWrappedMiniLabel);
            settings = (AvatarAudioSafetySettings)EditorGUILayout.ObjectField(AvatarAudioSafetyUiText.SettingsLabel, settings, typeof(AvatarAudioSafetySettings), true);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    AvatarAudioSafetyUiText.AvatarRootLabel,
                    settings != null ? settings.gameObject : null,
                    typeof(GameObject),
                    true);
            }

            if (settings == null)
            {
                EditorGUILayout.EndVertical();
                return;
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
            EditorGUILayout.EndVertical();
        }

        private void DrawFilterSection()
        {
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
                case ResultFilter.ManualReview:
                    return result.result == AvatarAudioSafetyResultKind.ManualReview;
                case ResultFilter.All:
                default:
                    return true;
            }
        }
    }
}
