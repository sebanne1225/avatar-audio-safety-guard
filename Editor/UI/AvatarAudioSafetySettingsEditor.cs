using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    [CustomEditor(typeof(AvatarAudioSafetySettings))]
    internal sealed class AvatarAudioSafetySettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty toolEnabledProperty;
        private SerializedProperty modeProperty;
        private SerializedProperty profileProperty;
        private SerializedProperty customThresholdsProperty;
        private SerializedProperty warnOnMissingVrcSpatialAudioSourceProperty;
        private SerializedProperty warnOnCustomRolloffProperty;
        private SerializedProperty warnOnLoopWithLongRangeProperty;
        private SerializedProperty warnOnNon3DAudioProperty;
        private SerializedProperty perSourceRulesProperty;
        private Vector2 resultScrollPosition;

        private void OnEnable()
        {
            toolEnabledProperty = serializedObject.FindProperty("toolEnabled");
            modeProperty = serializedObject.FindProperty("mode");
            profileProperty = serializedObject.FindProperty("profile");
            customThresholdsProperty = serializedObject.FindProperty("customThresholds");
            warnOnMissingVrcSpatialAudioSourceProperty = serializedObject.FindProperty("warnOnMissingVrcSpatialAudioSource");
            warnOnCustomRolloffProperty = serializedObject.FindProperty("warnOnCustomRolloff");
            warnOnLoopWithLongRangeProperty = serializedObject.FindProperty("warnOnLoopWithLongRange");
            warnOnNon3DAudioProperty = serializedObject.FindProperty("warnOnNon3DAudio");
            perSourceRulesProperty = serializedObject.FindProperty("perSourceRules");
        }

        public override void OnInspectorGUI()
        {
            AvatarAudioSafetySettings settings = (AvatarAudioSafetySettings)target;
            AvatarAudioSafetySessionState.RememberSettings(settings);
            string previousClassificationSignature = AvatarAudioSafetyScanActions.GetClassificationSignature(settings);

            serializedObject.Update();
            DrawSection(AvatarAudioSafetyUiText.BasicSectionTitle, AvatarAudioSafetyUiText.BasicSectionDescription, DrawBasicSection);
            serializedObject.ApplyModifiedProperties();

            DrawSection(AvatarAudioSafetyUiText.ToolsSectionTitle, AvatarAudioSafetyUiText.ToolsSectionDescription, () => DrawToolsSection(settings));
            DrawSection(AvatarAudioSafetyUiText.DetectedAudioSourcesSectionTitle, AvatarAudioSafetyUiText.DetectedAudioSourcesSectionDescription, () => DrawDetectedAudioSourcesSection(settings));

            serializedObject.Update();
            DrawSection(AvatarAudioSafetyUiText.PerSourceRulesSectionTitle, AvatarAudioSafetyUiText.PerSourceRulesSectionDescription, () => DrawPerSourceRulesSection(settings));
            DrawSection(AvatarAudioSafetyUiText.ThresholdSectionTitle, AvatarAudioSafetyUiText.ThresholdSectionDescription, () => DrawThresholdSection(settings));
            DrawSection(AvatarAudioSafetyUiText.DiagnosticsSectionTitle, AvatarAudioSafetyUiText.DiagnosticsSectionDescription, DrawDiagnosticsSection);
            serializedObject.ApplyModifiedProperties();
            AvatarAudioSafetyScanActions.RefreshDetectedResultsIfClassificationChanged(settings, previousClassificationSignature);
        }

        private void DrawBasicSection()
        {
            EditorGUILayout.PropertyField(toolEnabledProperty, AvatarAudioSafetyUiText.EnabledLabel);
            DrawEnumPopup(modeProperty, AvatarAudioSafetyUiText.ModeLabel, AvatarAudioSafetyUiText.ModeOptions);
            DrawEnumPopup(profileProperty, AvatarAudioSafetyUiText.ProfileLabel, AvatarAudioSafetyUiText.ProfileOptions);

            AvatarAudioSafetyMode mode = (AvatarAudioSafetyMode)modeProperty.enumValueIndex;

            DrawGuidance(
                AvatarAudioSafetyUiText.GetDryRunBehaviorDescription(toolEnabledProperty.boolValue, mode),
                AvatarAudioSafetyUiText.GetDryRunBehaviorMessageType(toolEnabledProperty.boolValue, mode));
        }

        private void DrawThresholdSection(AvatarAudioSafetySettings settings)
        {
            AvatarAudioSafetyProfile profile = (AvatarAudioSafetyProfile)profileProperty.enumValueIndex;

            if (profile == AvatarAudioSafetyProfile.Custom)
            {
                DrawDescription(AvatarAudioSafetyUiText.CustomThresholdDescription);
                DrawThresholdProperty(customThresholdsProperty);
                return;
            }

            AvatarAudioThresholdPreset preset = settings != null
                ? AvatarAudioThresholdPresets.Resolve(profile, settings.CustomThresholds)
                : AvatarAudioThresholdPresets.CreateStandard();

            DrawDescription(AvatarAudioSafetyUiText.GetThresholdReadonlyDescription(profile));

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField(AvatarAudioSafetyUiText.MaxGainLabel, preset.maxGain);
                EditorGUILayout.FloatField(AvatarAudioSafetyUiText.MaxFarDistanceLabel, preset.maxFarDistance);
                EditorGUILayout.FloatField(AvatarAudioSafetyUiText.MaxVolumeLabel, preset.maxVolume);
            }
        }

        private void DrawDiagnosticsSection()
        {
            EditorGUILayout.PropertyField(warnOnMissingVrcSpatialAudioSourceProperty, AvatarAudioSafetyUiText.WarnMissingSpatialAudioLabel);
            EditorGUILayout.PropertyField(warnOnCustomRolloffProperty, AvatarAudioSafetyUiText.WarnCustomRolloffLabel);
            EditorGUILayout.PropertyField(warnOnLoopWithLongRangeProperty, AvatarAudioSafetyUiText.WarnLoopLongRangeLabel);
            EditorGUILayout.PropertyField(warnOnNon3DAudioProperty, AvatarAudioSafetyUiText.WarnNon3DAudioLabel);
        }

        private void DrawPerSourceRulesSection(AvatarAudioSafetySettings settings)
        {
            DrawDescription(AvatarAudioSafetyUiText.PathRuleHelpText);
            DrawDescription(AvatarAudioSafetyUiText.PerSourceRulesReportAddHint);

            if (GUILayout.Button(AvatarAudioSafetyUiText.AddRuleButton))
            {
                int newIndex = perSourceRulesProperty.arraySize;
                perSourceRulesProperty.InsertArrayElementAtIndex(newIndex);
                AvatarAudioSafetyRuleActions.InitializeRuleElement(perSourceRulesProperty.GetArrayElementAtIndex(newIndex));
            }

            if (perSourceRulesProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.NoRulesMessage, MessageType.Info);
                return;
            }

            for (int i = 0; i < perSourceRulesProperty.arraySize; i++)
            {
                SerializedProperty element = perSourceRulesProperty.GetArrayElementAtIndex(i);
                SerializedProperty pathProperty = element.FindPropertyRelative("path");
                SerializedProperty ruleProperty = element.FindPropertyRelative("rule");
                SerializedProperty customThresholds = element.FindPropertyRelative("customThresholds");
                SerializedProperty memoProperty = element.FindPropertyRelative("memo");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(AvatarAudioSafetyUiText.GetRuleEntryTitle(i), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(AvatarAudioSafetyUiText.RemoveRuleButton, GUILayout.Width(72f)))
                {
                    perSourceRulesProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                DrawRuleTargetField(settings, pathProperty);
                EditorGUILayout.PropertyField(pathProperty, AvatarAudioSafetyUiText.PathLabel);
                DrawEnumPopup(ruleProperty, AvatarAudioSafetyUiText.RuleLabel, AvatarAudioSafetyUiText.RuleOptions);
                DrawDescription(AvatarAudioSafetyUiText.GetRuleDescription((AvatarAudioSafetyRule)ruleProperty.enumValueIndex));

                if ((AvatarAudioSafetyRule)ruleProperty.enumValueIndex == AvatarAudioSafetyRule.CustomThreshold)
                {
                    DrawThresholdProperty(customThresholds);
                }

                EditorGUILayout.PropertyField(memoProperty, AvatarAudioSafetyUiText.MemoLabel);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawToolsSection(AvatarAudioSafetySettings settings)
        {
            DrawDescription(AvatarAudioSafetyUiText.ResultsPersistenceHint);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(AvatarAudioSafetyUiText.ScanAudioSourcesButton))
            {
                RunDryRunScan(settings);
            }

            if (GUILayout.Button(AvatarAudioSafetyUiText.OpenReportButton))
            {
                AvatarAudioSafetyReportWindow.Open(settings);
            }

            if (GUILayout.Button(AvatarAudioSafetyUiText.ClearResultsButton))
            {
                ClearResults(settings);
            }

            EditorGUILayout.EndHorizontal();

            if (settings != null && settings.LastScanSummary != null)
            {
                EditorGUILayout.LabelField(AvatarAudioSafetyUiText.SummaryLabel, AvatarAudioSafetyUiText.GetSummaryText(settings.LastScanSummary), EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrEmpty(settings.LastScanSummary.lastScanLocalTime))
                {
                    EditorGUILayout.LabelField(AvatarAudioSafetyUiText.LastScanLabel, settings.LastScanSummary.lastScanLocalTime);
                }
            }
        }

        private void DrawDetectedAudioSourcesSection(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.MissingSettingsComponentMessage, MessageType.Error);
                return;
            }

            AvatarAudioSafetyResultTable.Draw(settings.DetectedAudioSources, ref resultScrollPosition);
        }

        private static void DrawSection(string title, string description, System.Action drawAction)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(2f);
            DrawDescription(description);
            drawAction();
            EditorGUILayout.EndVertical();
        }

        private static void DrawDescription(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            EditorGUILayout.LabelField(text, EditorStyles.wordWrappedMiniLabel);
        }

        private static void DrawGuidance(string text, MessageType messageType)
        {
            if (messageType == MessageType.None)
            {
                DrawDescription(text);
                return;
            }

            EditorGUILayout.HelpBox(text, messageType);
        }

        private static void DrawThresholdProperty(SerializedProperty thresholdProperty)
        {
            if (thresholdProperty == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(thresholdProperty.FindPropertyRelative("maxGain"), AvatarAudioSafetyUiText.MaxGainLabel);
            EditorGUILayout.PropertyField(thresholdProperty.FindPropertyRelative("maxFarDistance"), AvatarAudioSafetyUiText.MaxFarDistanceLabel);
            EditorGUILayout.PropertyField(thresholdProperty.FindPropertyRelative("maxVolume"), AvatarAudioSafetyUiText.MaxVolumeLabel);
        }

        private static void DrawEnumPopup(SerializedProperty enumProperty, GUIContent label, string[] options)
        {
            if (enumProperty == null || options == null || options.Length == 0)
            {
                return;
            }

            int selected = Mathf.Clamp(enumProperty.enumValueIndex, 0, options.Length - 1);
            int next = EditorGUILayout.Popup(label, selected, options);
            enumProperty.enumValueIndex = next;
        }

        private static void DrawRuleTargetField(AvatarAudioSafetySettings settings, SerializedProperty pathProperty)
        {
            if (pathProperty == null)
            {
                return;
            }

            GameObject currentTarget = ResolveRuleTarget(settings, pathProperty.stringValue);

            EditorGUI.BeginChangeCheck();
            GameObject nextTarget = (GameObject)EditorGUILayout.ObjectField(
                AvatarAudioSafetyUiText.RuleTargetLabel,
                currentTarget,
                typeof(GameObject),
                true);

            if (EditorGUI.EndChangeCheck())
            {
                if (nextTarget == null)
                {
                    pathProperty.stringValue = string.Empty;
                }
                else if (settings != null && AvatarAudioSafetyPathUtility.IsDescendantOrSelf(settings.transform, nextTarget.transform))
                {
                    pathProperty.stringValue = AvatarAudioSafetyPathUtility.GetRelativePath(settings.transform, nextTarget.transform);
                }
                else
                {
                    Debug.LogWarning("[Avatar Audio Safety Guard] " + AvatarAudioSafetyUiText.InvalidRuleTargetMessage, settings);
                }
            }

            if (!string.IsNullOrEmpty(pathProperty.stringValue) && currentTarget == null)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.UnresolvedRuleTargetMessage, MessageType.Warning);
            }
        }

        private static GameObject ResolveRuleTarget(AvatarAudioSafetySettings settings, string path)
        {
            if (settings == null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            Transform target = AvatarAudioSafetyPathUtility.ResolveRelativePath(settings.transform, path);
            return target != null ? target.gameObject : null;
        }

        private void RunDryRunScan(AvatarAudioSafetySettings settings)
        {
            if (AvatarAudioSafetyScanActions.RunDryRun(settings))
            {
                AvatarAudioSafetyUiFeedback.ShowInfoDialog(
                    AvatarAudioSafetyUiText.GetScanCompletedDialogMessage(settings != null ? settings.DetectedAudioSources : null));
            }
        }

        private void ClearResults(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return;
            }

            if (!AvatarAudioSafetyUiFeedback.ConfirmClearResults())
            {
                return;
            }

            Undo.RecordObject(settings, "Avatar Audio Safety Guard Clear Results");
            settings.ClearScanResults();
            EditorUtility.SetDirty(settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
        }

    }
}
