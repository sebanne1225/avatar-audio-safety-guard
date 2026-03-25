using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

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

    internal static class AvatarAudioSafetyUiFeedback
    {
        public static void ShowInfoDialog(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            EditorUtility.DisplayDialog(
                AvatarAudioSafetyUiText.OperationDialogTitle,
                message,
                AvatarAudioSafetyUiText.OperationDialogOkButton);
        }

        public static bool ConfirmClearLastBuildResult()
        {
            return EditorUtility.DisplayDialog(
                AvatarAudioSafetyUiText.ClearLastBuildResultDialogTitle,
                AvatarAudioSafetyUiText.ClearLastBuildResultDialogMessage,
                AvatarAudioSafetyUiText.ClearDialogConfirmButton,
                AvatarAudioSafetyUiText.CancelButtonLabel);
        }

        public static bool ConfirmClearResults()
        {
            return EditorUtility.DisplayDialog(
                AvatarAudioSafetyUiText.ClearResultsDialogTitle,
                AvatarAudioSafetyUiText.ClearResultsDialogMessage,
                AvatarAudioSafetyUiText.ClearDialogConfirmButton,
                AvatarAudioSafetyUiText.CancelButtonLabel);
        }
    }

    internal static class AvatarAudioSafetyRuleActions
    {
        public static void InitializeRuleElement(SerializedProperty element, string path = "")
        {
            if (element == null)
            {
                return;
            }

            AvatarAudioThresholdPreset defaults = AvatarAudioThresholdPresets.CreateCustomDefault();

            element.FindPropertyRelative("path").stringValue = path ?? string.Empty;
            element.FindPropertyRelative("rule").enumValueIndex = (int)AvatarAudioSafetyRule.Default;
            element.FindPropertyRelative("memo").stringValue = string.Empty;

            SerializedProperty thresholds = element.FindPropertyRelative("customThresholds");
            if (thresholds != null)
            {
                thresholds.FindPropertyRelative("maxGain").floatValue = defaults.maxGain;
                thresholds.FindPropertyRelative("maxFarDistance").floatValue = defaults.maxFarDistance;
                thresholds.FindPropertyRelative("maxVolume").floatValue = defaults.maxVolume;
            }
        }

        public static bool CanAddRuleDraft(
            AvatarAudioSafetySettings settings,
            GameObject targetObject,
            out string resolvedPath,
            out string unavailableReason)
        {
            resolvedPath = string.Empty;
            unavailableReason = string.Empty;

            if (settings == null)
            {
                unavailableReason = AvatarAudioSafetyUiText.AddRuleUnavailableMissingSettings;
                return false;
            }

            if (targetObject == null || targetObject.transform == null)
            {
                unavailableReason = AvatarAudioSafetyUiText.AddRuleUnavailableMissingTarget;
                return false;
            }

            if (!AvatarAudioSafetyPathUtility.IsDescendantOrSelf(settings.transform, targetObject.transform))
            {
                unavailableReason = AvatarAudioSafetyUiText.InvalidRuleTargetMessage;
                return false;
            }

            resolvedPath = AvatarAudioSafetyPathUtility.GetRelativePath(settings.transform, targetObject.transform);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                unavailableReason = AvatarAudioSafetyUiText.AddRuleUnavailableMissingTarget;
                return false;
            }

            return true;
        }

        public static bool TryAddRuleDraft(AvatarAudioSafetySettings settings, GameObject targetObject)
        {
            string resolvedPath;
            string unavailableReason;
            if (!CanAddRuleDraft(settings, targetObject, out resolvedPath, out unavailableReason))
            {
                return false;
            }

            SerializedObject serializedObject = new SerializedObject(settings);
            SerializedProperty rulesProperty = serializedObject.FindProperty("perSourceRules");
            if (rulesProperty == null)
            {
                return false;
            }

            int existingIndex = FindRuleIndex(rulesProperty, resolvedPath);
            if (existingIndex >= 0)
            {
                FocusSettings(settings);
                Debug.Log(
                    string.Format(
                        "[Avatar Audio Safety Guard] 既に path '{0}' の個別ルールがあります。settings を選択しました。",
                        resolvedPath),
                    settings);
                return false;
            }

            Undo.RecordObject(settings, "Avatar Audio Safety Guard Add Rule Draft");
            int newIndex = rulesProperty.arraySize;
            rulesProperty.InsertArrayElementAtIndex(newIndex);
            InitializeRuleElement(rulesProperty.GetArrayElementAtIndex(newIndex), resolvedPath);
            serializedObject.ApplyModifiedProperties();
            AvatarAudioSafetyScanActions.RefreshDetectedResults(settings);
            EditorUtility.SetDirty(settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
            FocusSettings(settings);

            Debug.Log(
                string.Format(
                    "[Avatar Audio Safety Guard] path '{0}' の個別ルール下書きを追加しました。settings で内容を確認してください。",
                    resolvedPath),
                settings);
            AvatarAudioSafetyUiFeedback.ShowInfoDialog(AvatarAudioSafetyUiText.AddRuleCompletedDialogMessage);
            return true;
        }

        public static AvatarAudioSafetyRule GetEffectiveRule(AvatarAudioSafetySettings settings, GameObject targetObject)
        {
            string resolvedPath;
            string unavailableReason;
            if (!CanAddRuleDraft(settings, targetObject, out resolvedPath, out unavailableReason))
            {
                return AvatarAudioSafetyRule.Default;
            }

            SerializedObject serializedObject = new SerializedObject(settings);
            SerializedProperty rulesProperty = serializedObject.FindProperty("perSourceRules");
            if (rulesProperty == null)
            {
                return AvatarAudioSafetyRule.Default;
            }

            int existingIndex = FindRuleIndex(rulesProperty, resolvedPath);
            if (existingIndex < 0)
            {
                return AvatarAudioSafetyRule.Default;
            }

            SerializedProperty element = rulesProperty.GetArrayElementAtIndex(existingIndex);
            SerializedProperty ruleProperty = element.FindPropertyRelative("rule");
            if (ruleProperty == null)
            {
                return AvatarAudioSafetyRule.Default;
            }

            return (AvatarAudioSafetyRule)ruleProperty.enumValueIndex;
        }

        public static bool TrySetRule(AvatarAudioSafetySettings settings, GameObject targetObject, AvatarAudioSafetyRule nextRule)
        {
            string resolvedPath;
            string unavailableReason;
            if (!CanAddRuleDraft(settings, targetObject, out resolvedPath, out unavailableReason))
            {
                return false;
            }

            SerializedObject serializedObject = new SerializedObject(settings);
            SerializedProperty rulesProperty = serializedObject.FindProperty("perSourceRules");
            if (rulesProperty == null)
            {
                return false;
            }

            int existingIndex = FindRuleIndex(rulesProperty, resolvedPath);
            bool changed = false;

            Undo.RecordObject(settings, "Avatar Audio Safety Guard Change Rule");

            if (nextRule == AvatarAudioSafetyRule.Default)
            {
                if (existingIndex >= 0)
                {
                    rulesProperty.DeleteArrayElementAtIndex(existingIndex);
                    changed = true;
                }
            }
            else
            {
                SerializedProperty element = existingIndex >= 0
                    ? rulesProperty.GetArrayElementAtIndex(existingIndex)
                    : CreateRuleElement(rulesProperty, resolvedPath);

                SerializedProperty ruleProperty = element.FindPropertyRelative("rule");
                if (ruleProperty != null && ruleProperty.enumValueIndex != (int)nextRule)
                {
                    ruleProperty.enumValueIndex = (int)nextRule;
                    changed = true;
                }
                else if (existingIndex < 0)
                {
                    changed = true;
                }
            }

            if (!changed)
            {
                return false;
            }

            serializedObject.ApplyModifiedProperties();
            AvatarAudioSafetyScanActions.RefreshDetectedResults(settings);
            EditorUtility.SetDirty(settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
            return true;
        }

        public static AvatarAudioThresholdPreset GetCustomThresholdsOrDefault(
            AvatarAudioSafetySettings settings,
            GameObject targetObject)
        {
            AvatarAudioThresholdPreset thresholds;
            if (TryGetCustomThresholds(settings, targetObject, out thresholds) && thresholds != null)
            {
                return thresholds;
            }

            return AvatarAudioThresholdPresets.CreateCustomDefault();
        }

        public static bool TryGetCustomThresholds(
            AvatarAudioSafetySettings settings,
            GameObject targetObject,
            out AvatarAudioThresholdPreset thresholds)
        {
            thresholds = null;

            SerializedProperty element;
            if (!TryGetRuleElement(settings, targetObject, false, out element))
            {
                return false;
            }

            SerializedProperty ruleProperty = element.FindPropertyRelative("rule");
            if (ruleProperty == null || (AvatarAudioSafetyRule)ruleProperty.enumValueIndex != AvatarAudioSafetyRule.CustomThreshold)
            {
                return false;
            }

            SerializedProperty thresholdsProperty = element.FindPropertyRelative("customThresholds");
            if (thresholdsProperty == null)
            {
                return false;
            }

            thresholds = new AvatarAudioThresholdPreset
            {
                maxGain = thresholdsProperty.FindPropertyRelative("maxGain").floatValue,
                maxFarDistance = thresholdsProperty.FindPropertyRelative("maxFarDistance").floatValue,
                maxVolume = thresholdsProperty.FindPropertyRelative("maxVolume").floatValue,
            };
            return true;
        }

        public static bool TrySetCustomThresholds(
            AvatarAudioSafetySettings settings,
            GameObject targetObject,
            AvatarAudioThresholdPreset thresholds)
        {
            if (thresholds == null)
            {
                return false;
            }

            SerializedProperty element;
            if (!TryGetRuleElement(settings, targetObject, true, out element))
            {
                return false;
            }

            SerializedProperty ruleProperty = element.FindPropertyRelative("rule");
            SerializedProperty thresholdsProperty = element.FindPropertyRelative("customThresholds");
            if (ruleProperty == null || thresholdsProperty == null)
            {
                return false;
            }

            bool changed = false;
            Undo.RecordObject(settings, "Avatar Audio Safety Guard Update Custom Threshold");

            if (ruleProperty.enumValueIndex != (int)AvatarAudioSafetyRule.CustomThreshold)
            {
                ruleProperty.enumValueIndex = (int)AvatarAudioSafetyRule.CustomThreshold;
                changed = true;
            }

            changed |= SetFloatProperty(thresholdsProperty.FindPropertyRelative("maxGain"), thresholds.maxGain);
            changed |= SetFloatProperty(thresholdsProperty.FindPropertyRelative("maxFarDistance"), thresholds.maxFarDistance);
            changed |= SetFloatProperty(thresholdsProperty.FindPropertyRelative("maxVolume"), thresholds.maxVolume);

            if (!changed)
            {
                return false;
            }

            element.serializedObject.ApplyModifiedProperties();
            AvatarAudioSafetyScanActions.RefreshDetectedResults(settings);
            EditorUtility.SetDirty(settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
            return true;
        }

        private static int FindRuleIndex(SerializedProperty rulesProperty, string path)
        {
            if (rulesProperty == null)
            {
                return -1;
            }

            string normalizedPath = AvatarAudioSafetyPathUtility.Normalize(path);

            for (int i = 0; i < rulesProperty.arraySize; i++)
            {
                SerializedProperty element = rulesProperty.GetArrayElementAtIndex(i);
                SerializedProperty pathProperty = element.FindPropertyRelative("path");
                string rulePath = pathProperty != null ? AvatarAudioSafetyPathUtility.Normalize(pathProperty.stringValue) : string.Empty;
                if (rulePath == normalizedPath)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool TryGetRuleElement(
            AvatarAudioSafetySettings settings,
            GameObject targetObject,
            bool createIfMissing,
            out SerializedProperty element)
        {
            element = null;

            string resolvedPath;
            string unavailableReason;
            if (!CanAddRuleDraft(settings, targetObject, out resolvedPath, out unavailableReason))
            {
                return false;
            }

            SerializedObject serializedObject = new SerializedObject(settings);
            SerializedProperty rulesProperty = serializedObject.FindProperty("perSourceRules");
            if (rulesProperty == null)
            {
                return false;
            }

            int existingIndex = FindRuleIndex(rulesProperty, resolvedPath);
            if (existingIndex >= 0)
            {
                element = rulesProperty.GetArrayElementAtIndex(existingIndex);
                return true;
            }

            if (!createIfMissing)
            {
                return false;
            }

            Undo.RecordObject(settings, "Avatar Audio Safety Guard Create Rule");
            element = CreateRuleElement(rulesProperty, resolvedPath);
            serializedObject.ApplyModifiedProperties();
            return element != null;
        }

        private static SerializedProperty CreateRuleElement(SerializedProperty rulesProperty, string resolvedPath)
        {
            if (rulesProperty == null)
            {
                return null;
            }

            int newIndex = rulesProperty.arraySize;
            rulesProperty.InsertArrayElementAtIndex(newIndex);
            SerializedProperty element = rulesProperty.GetArrayElementAtIndex(newIndex);
            InitializeRuleElement(element, resolvedPath);
            return element;
        }

        private static bool SetFloatProperty(SerializedProperty property, float value)
        {
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static void FocusSettings(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }
    }
}
