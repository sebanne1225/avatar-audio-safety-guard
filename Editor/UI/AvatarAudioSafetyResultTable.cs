using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyResultTable
    {
        private const float ResultLabelWidth = 118f;
        private const float DetailsButtonWidth = 52f;
        private const float RulePopupWidth = 108f;
        private const float RowSpacing = 2f;
        private const float HorizontalGap = 4f;
        private const float ComparisonPanelGap = 6f;
        private static readonly Dictionary<int, bool> DetailFoldouts = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> BuildDetailFoldouts = new Dictionary<int, bool>();

        public static void Draw(
            IReadOnlyList<AvatarAudioScanResult> results,
            ref Vector2 scrollPosition,
            Func<AvatarAudioScanResult, bool> filter = null,
            bool useScrollView = true,
            AvatarAudioSafetySettings ruleDraftSettings = null)
        {
            if (results == null || results.Count == 0)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.ResultsEmptyMessage, MessageType.Info);
                return;
            }

            if (useScrollView)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(180f));
            }

            bool hasVisibleRow = false;
            for (int i = 0; i < results.Count; i++)
            {
                AvatarAudioScanResult result = results[i];
                if (result == null)
                {
                    continue;
                }

                if (filter != null && !filter(result))
                {
                    continue;
                }

                DrawRow(result, hasVisibleRow, ruleDraftSettings);
                hasVisibleRow = true;
            }

            if (!hasVisibleRow)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.FilterNoMatchMessage, MessageType.None);
            }

            if (useScrollView)
            {
                EditorGUILayout.EndScrollView();
            }
        }

        public static void DrawBuildResults(
            IReadOnlyList<AvatarAudioSafetyBuildResultEntry> entries,
            ref Vector2 scrollPosition,
            Transform targetRoot = null,
            IReadOnlyList<GameObject> cachedTargetObjects = null,
            bool useScrollView = true,
            AvatarAudioSafetySettings ruleDraftSettings = null)
        {
            if (entries == null || entries.Count == 0)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.BuildResultEntriesEmptyMessage, MessageType.Info);
                return;
            }

            if (useScrollView)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(150f));
            }

            for (int i = 0; i < entries.Count; i++)
            {
                AvatarAudioSafetyBuildResultEntry entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                DrawBuildRow(entry, targetRoot, cachedTargetObjects, ruleDraftSettings, i, i > 0);
            }

            if (useScrollView)
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private static void DrawRow(
            AvatarAudioScanResult result,
            bool drawSeparator,
            AvatarAudioSafetySettings ruleDraftSettings)
        {
            bool hasDetails = !string.IsNullOrEmpty(result.reason) || !string.IsNullOrEmpty(result.plannedChange);
            GameObject ruleTargetObject = result != null && result.audioSource != null ? result.audioSource.gameObject : null;
            AvatarAudioSafetyRule currentRule = AvatarAudioSafetyRule.Default;
            bool hasRuleEditor = ruleDraftSettings != null
                && AvatarAudioSafetyRuleActions.CanAddRuleDraft(ruleDraftSettings, ruleTargetObject, out _, out _);
            if (hasRuleEditor)
            {
                currentRule = AvatarAudioSafetyRuleActions.GetEffectiveRule(ruleDraftSettings, ruleTargetObject);
            }

            bool showsCustomThresholdEditor = hasRuleEditor && currentRule == AvatarAudioSafetyRule.CustomThreshold;
            bool hasExpandableContent = hasDetails || showsCustomThresholdEditor;
            int rowKey = GetRowKey(result);
            bool isExpanded = hasExpandableContent && GetFoldoutState(rowKey);

            if (drawSeparator)
            {
                DrawSeparator();
            }

            DrawPrimaryRow(result);
            DrawMainInfoRow(result, ref isExpanded, rowKey, hasExpandableContent, ruleDraftSettings, ruleTargetObject, currentRule, hasRuleEditor);
            DrawPathRow(result);

            if (isExpanded)
            {
                DrawExpandedDetails(result, ruleDraftSettings, ruleTargetObject, currentRule, hasRuleEditor);
            }

            EditorGUILayout.Space(1f);
        }

        private static void DrawPrimaryRow(AvatarAudioScanResult result)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect resultRect = new Rect(rowRect.x, rowRect.y, ResultLabelWidth, rowRect.height);
            Rect objectRect = new Rect(
                resultRect.xMax + HorizontalGap,
                rowRect.y,
                Mathf.Max(0f, rowRect.xMax - (resultRect.xMax + HorizontalGap)),
                rowRect.height);

            EditorGUI.LabelField(resultRect, GetResultLabel(result.result), GetResultStyle(result.result));
            DrawObjectReference(objectRect, result.audioSource);
        }

        private static void DrawMainInfoRow(
            AvatarAudioScanResult result,
            ref bool isExpanded,
            int rowKey,
            bool hasExpandableContent,
            AvatarAudioSafetySettings ruleDraftSettings,
            GameObject ruleTargetObject,
            AvatarAudioSafetyRule currentRule,
            bool hasRuleEditor)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect contentRect = GetIndentedRect(rowRect);
            Rect mainInfoRect = contentRect;

            if (hasExpandableContent || ruleDraftSettings != null)
            {
                float buttonsWidth = 0f;
                if (ruleDraftSettings != null)
                {
                    buttonsWidth += RulePopupWidth + HorizontalGap;
                }

                if (hasExpandableContent)
                {
                    buttonsWidth += DetailsButtonWidth;
                }

                Rect buttonRect = new Rect(contentRect.xMax - buttonsWidth, contentRect.y, buttonsWidth, contentRect.height);
                mainInfoRect.xMax = buttonRect.xMin - HorizontalGap;

                if (ruleDraftSettings != null)
                {
                    Rect ruleRect = new Rect(buttonRect.x, contentRect.y, RulePopupWidth, contentRect.height);
                    DrawRulePopup(ruleRect, ruleDraftSettings, ruleTargetObject, currentRule, hasRuleEditor, ref isExpanded, rowKey);
                    buttonRect.xMin = ruleRect.xMax + HorizontalGap;
                }

                if (hasExpandableContent)
                {
                    Rect detailsRect = new Rect(buttonRect.xMax - DetailsButtonWidth, contentRect.y, DetailsButtonWidth, contentRect.height);
                    string buttonLabel = isExpanded ? AvatarAudioSafetyUiText.HideDetailsButton : AvatarAudioSafetyUiText.DetailsButton;
                    if (GUI.Button(detailsRect, buttonLabel, EditorStyles.miniButton))
                    {
                        isExpanded = !isExpanded;
                        DetailFoldouts[rowKey] = isExpanded;
                    }
                }
            }

            GUIContent content = new GUIContent(AvatarAudioSafetyUiText.GetMainInfoText(result), AvatarAudioSafetyUiText.GetMainInfoTooltip(result));
            EditorGUI.LabelField(mainInfoRect, content, GetMainInfoStyle());
        }

        private static void DrawPathRow(AvatarAudioScanResult result)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect pathRect = GetIndentedRect(rowRect);
            GUIContent content = new GUIContent(AvatarAudioSafetyUiText.GetPathText(result.path), result.path);
            EditorGUI.LabelField(pathRect, content, GetPathStyle());
        }

        private static void DrawExpandedDetails(
            AvatarAudioScanResult result,
            AvatarAudioSafetySettings ruleDraftSettings,
            GameObject ruleTargetObject,
            AvatarAudioSafetyRule currentRule,
            bool hasRuleEditor)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(RowSpacing);
            EditorGUI.indentLevel++;
            DrawDetailRow(AvatarAudioSafetyUiText.GetReasonLabel(), result.reason);
            DrawDetailRow(AvatarAudioSafetyUiText.GetPlanLabel(), result.plannedChange);

            if (hasRuleEditor && currentRule == AvatarAudioSafetyRule.CustomThreshold)
            {
                DrawCustomThresholdEditor(ruleDraftSettings, ruleTargetObject);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private static void DrawBuildRow(
            AvatarAudioSafetyBuildResultEntry entry,
            Transform targetRoot,
            IReadOnlyList<GameObject> cachedTargetObjects,
            AvatarAudioSafetySettings ruleDraftSettings,
            int entryIndex,
            bool drawSeparator)
        {
            bool hasDetails = !string.IsNullOrEmpty(entry.reason)
                || !string.IsNullOrEmpty(entry.detail)
                || !string.IsNullOrEmpty(entry.beforeSummary)
                || !string.IsNullOrEmpty(entry.afterSummary);
            GameObject ruleTargetObject = ResolveCachedBuildTargetObject(cachedTargetObjects, entryIndex);
            AvatarAudioSafetyRule currentRule = AvatarAudioSafetyRule.Default;
            bool hasRuleEditor = ruleDraftSettings != null
                && AvatarAudioSafetyRuleActions.CanAddRuleDraft(ruleDraftSettings, ruleTargetObject, out _, out _);
            if (hasRuleEditor)
            {
                currentRule = AvatarAudioSafetyRuleActions.GetEffectiveRule(ruleDraftSettings, ruleTargetObject);
            }

            bool showsCustomThresholdEditor = hasRuleEditor && currentRule == AvatarAudioSafetyRule.CustomThreshold;
            bool hasExpandableContent = hasDetails || showsCustomThresholdEditor;
            int rowKey = GetBuildRowKey(entry);
            bool isExpanded = hasExpandableContent && GetBuildFoldoutState(rowKey);

            if (drawSeparator)
            {
                DrawSeparator();
            }

            DrawBuildPrimaryRow(entry, targetRoot, cachedTargetObjects, entryIndex);
            DrawBuildMainInfoRow(entry, ruleDraftSettings, ruleTargetObject, currentRule, hasRuleEditor, ref isExpanded, rowKey, hasExpandableContent);
            DrawBuildPathRow(entry);

            if (isExpanded)
            {
                DrawBuildExpandedDetails(entry, ruleDraftSettings, ruleTargetObject, currentRule, hasRuleEditor);
            }

            EditorGUILayout.Space(1f);
        }

        private static void DrawBuildPrimaryRow(
            AvatarAudioSafetyBuildResultEntry entry,
            Transform targetRoot,
            IReadOnlyList<GameObject> cachedTargetObjects,
            int entryIndex)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect resultRect = new Rect(rowRect.x, rowRect.y, ResultLabelWidth, rowRect.height);
            Rect contentRect = new Rect(
                resultRect.xMax + HorizontalGap,
                rowRect.y,
                Mathf.Max(0f, rowRect.xMax - (resultRect.xMax + HorizontalGap)),
                rowRect.height);

            EditorGUI.LabelField(resultRect, AvatarAudioSafetyUiText.GetBuildEntryStatusLabel(entry.status), GetBuildResultStyle(entry.status));

            GameObject targetObject = ResolveCachedBuildTargetObject(cachedTargetObjects, entryIndex);
            if (targetObject == null)
            {
                targetObject = ResolveBuildTargetObject(targetRoot, entry.path);
            }

            if (targetObject != null)
            {
                EditorGUI.ObjectField(contentRect, GUIContent.none, targetObject, typeof(GameObject), true);
                return;
            }

            EditorGUI.LabelField(contentRect, GetBuildMainInfoText(entry), GetMainInfoStyle());
        }

        private static void DrawBuildMainInfoRow(
            AvatarAudioSafetyBuildResultEntry entry,
            AvatarAudioSafetySettings ruleDraftSettings,
            GameObject ruleTargetObject,
            AvatarAudioSafetyRule currentRule,
            bool hasRuleEditor,
            ref bool isExpanded,
            int rowKey,
            bool hasExpandableContent)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect contentRect = GetIndentedRect(rowRect);
            Rect infoRect = contentRect;

            if (hasExpandableContent || ruleDraftSettings != null)
            {
                float buttonsWidth = 0f;
                if (ruleDraftSettings != null)
                {
                    buttonsWidth += RulePopupWidth + HorizontalGap;
                }

                if (hasExpandableContent)
                {
                    buttonsWidth += DetailsButtonWidth;
                }

                Rect buttonRect = new Rect(contentRect.xMax - buttonsWidth, contentRect.y, buttonsWidth, contentRect.height);
                infoRect.xMax = buttonRect.xMin - HorizontalGap;

                if (ruleDraftSettings != null)
                {
                    Rect ruleRect = new Rect(buttonRect.x, contentRect.y, RulePopupWidth, contentRect.height);
                    DrawRulePopup(ruleRect, ruleDraftSettings, ruleTargetObject, currentRule, hasRuleEditor, ref isExpanded, rowKey);
                    buttonRect.xMin = ruleRect.xMax + HorizontalGap;
                }

                if (hasExpandableContent)
                {
                    Rect detailsRect = new Rect(buttonRect.xMax - DetailsButtonWidth, contentRect.y, DetailsButtonWidth, contentRect.height);
                    string buttonLabel = isExpanded ? AvatarAudioSafetyUiText.HideDetailsButton : AvatarAudioSafetyUiText.DetailsButton;
                    if (GUI.Button(detailsRect, buttonLabel, EditorStyles.miniButton))
                    {
                        isExpanded = !isExpanded;
                        BuildDetailFoldouts[rowKey] = isExpanded;
                    }
                }
            }

            EditorGUI.LabelField(infoRect, GetBuildSecondaryInfoText(entry), GetPathStyle());
        }

        private static void DrawBuildPathRow(AvatarAudioSafetyBuildResultEntry entry)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect pathRect = GetIndentedRect(rowRect);
            GUIContent content = new GUIContent(AvatarAudioSafetyUiText.GetPathText(entry.path), entry.path);
            EditorGUI.LabelField(pathRect, content, GetPathStyle());
        }

        private static void DrawBuildExpandedDetails(
            AvatarAudioSafetyBuildResultEntry entry,
            AvatarAudioSafetySettings ruleDraftSettings,
            GameObject ruleTargetObject,
            AvatarAudioSafetyRule currentRule,
            bool hasRuleEditor)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(RowSpacing);
            EditorGUI.indentLevel++;
            DrawDetailRow(AvatarAudioSafetyUiText.GetReasonLabel(), entry.reason);
            DrawDetailRow(AvatarAudioSafetyUiText.BuildEntryResultLabel, entry.detail);
            DrawBeforeAfterComparison(entry.beforeSummary, entry.afterSummary);

            if (hasRuleEditor && currentRule == AvatarAudioSafetyRule.CustomThreshold)
            {
                DrawCustomThresholdEditor(ruleDraftSettings, ruleTargetObject);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private static void DrawBeforeAfterComparison(string beforeSummary, string afterSummary)
        {
            if (string.IsNullOrEmpty(beforeSummary) && string.IsNullOrEmpty(afterSummary))
            {
                return;
            }

            bool useVerticalLayout = EditorGUIUtility.currentViewWidth < 720f;

            if (useVerticalLayout)
            {
                DrawComparisonPanel(AvatarAudioSafetyUiText.BuildBeforeLabel, beforeSummary);
                DrawComparisonFlowIndicator(true);
                EditorGUILayout.Space(RowSpacing);
                DrawComparisonPanel(AvatarAudioSafetyUiText.BuildAfterLabel, afterSummary);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawComparisonPanel(AvatarAudioSafetyUiText.BuildBeforeLabel, beforeSummary);
            DrawComparisonFlowIndicator(false);
            GUILayout.Space(ComparisonPanelGap);
            DrawComparisonPanel(AvatarAudioSafetyUiText.BuildAfterLabel, afterSummary);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawComparisonPanel(string title, string value)
        {
            GUILayoutOption expandWidth = GUILayout.ExpandWidth(true);
            GUILayoutOption minHeight = GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 2f + 10f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, expandWidth, minHeight))
            {
                EditorGUILayout.LabelField(title, GetComparisonHeaderStyle());
                EditorGUILayout.Space(1f);
                EditorGUILayout.LabelField(
                    string.IsNullOrEmpty(value) ? AvatarAudioSafetyUiText.ComparisonEmptyValue : value,
                    GetComparisonValueStyle());
            }
        }

        private static void DrawComparisonFlowIndicator(bool vertical)
        {
            GUILayoutOption fixedWidth = GUILayout.Width(40f);
            GUILayoutOption fixedHeight = GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f + 8f);

            using (new EditorGUILayout.VerticalScope(vertical ? EditorStyles.helpBox : GUIStyle.none, fixedWidth, fixedHeight))
            {
                GUILayout.FlexibleSpace();

                GUIStyle arrowStyle = GetComparisonArrowStyle();
                string arrow = vertical ? "↓" : AvatarAudioSafetyUiText.BuildComparisonArrow;
                GUILayout.Label(arrow, arrowStyle);

                GUIStyle labelStyle = GetComparisonHeaderStyle();
                labelStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(AvatarAudioSafetyUiText.BuildComparisonFlowLabel, labelStyle);

                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawRulePopup(
            Rect rect,
            AvatarAudioSafetySettings ruleDraftSettings,
            GameObject ruleTargetObject,
            AvatarAudioSafetyRule currentRule,
            bool canEditRule,
            ref bool isExpanded,
            int rowKey)
        {
            int selectedRule = Mathf.Clamp((int)currentRule, 0, AvatarAudioSafetyUiText.RuleOptions.Length - 1);
            using (new EditorGUI.DisabledScope(!canEditRule))
            {
                int nextRule = EditorGUI.Popup(rect, selectedRule, AvatarAudioSafetyUiText.RuleOptions, EditorStyles.miniPullDown);
                if (nextRule != selectedRule
                    && AvatarAudioSafetyRuleActions.TrySetRule(ruleDraftSettings, ruleTargetObject, (AvatarAudioSafetyRule)nextRule))
                {
                    if ((AvatarAudioSafetyRule)nextRule == AvatarAudioSafetyRule.CustomThreshold)
                    {
                        isExpanded = true;
                        DetailFoldouts[rowKey] = true;
                    }

                    GUIUtility.ExitGUI();
                }
            }
        }

        private static void DrawCustomThresholdEditor(
            AvatarAudioSafetySettings ruleDraftSettings,
            GameObject ruleTargetObject)
        {
            if (ruleDraftSettings == null || ruleTargetObject == null)
            {
                return;
            }

            AvatarAudioThresholdPreset thresholds = AvatarAudioSafetyRuleActions.GetCustomThresholdsOrDefault(ruleDraftSettings, ruleTargetObject);
            if (thresholds == null)
            {
                return;
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.InlineRuleLabel + ": " + AvatarAudioSafetyUiText.GetRuleShortLabel(AvatarAudioSafetyRule.CustomThreshold), GetDetailStyle());
            EditorGUILayout.LabelField(AvatarAudioSafetyUiText.CustomThresholdDescription, GetDetailStyle());

            EditorGUI.BeginChangeCheck();
            float maxGain = EditorGUILayout.DelayedFloatField(AvatarAudioSafetyUiText.MaxGainLabel, thresholds.maxGain);
            float maxFarDistance = EditorGUILayout.DelayedFloatField(AvatarAudioSafetyUiText.MaxFarDistanceLabel, thresholds.maxFarDistance);
            float maxVolume = EditorGUILayout.Slider(AvatarAudioSafetyUiText.MaxVolumeLabel, thresholds.maxVolume, 0f, 1f);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            AvatarAudioThresholdPreset updatedThresholds = new AvatarAudioThresholdPreset
            {
                maxGain = Mathf.Max(0f, maxGain),
                maxFarDistance = Mathf.Max(0f, maxFarDistance),
                maxVolume = Mathf.Clamp01(maxVolume),
            };

            if (AvatarAudioSafetyRuleActions.TrySetCustomThresholds(ruleDraftSettings, ruleTargetObject, updatedThresholds))
            {
                GUIUtility.ExitGUI();
            }
        }

        private static void DrawObjectReference(Rect rect, AudioSource audioSource)
        {
            GameObject targetObject = audioSource != null ? audioSource.gameObject : null;
            EditorGUI.ObjectField(rect, GUIContent.none, targetObject, typeof(GameObject), true);

            if (audioSource != null && GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                AvatarAudioSafetyObjectActions.Select(audioSource);
            }
        }

        private static GameObject ResolveBuildTargetObject(Transform targetRoot, string path)
        {
            if (targetRoot == null)
            {
                return null;
            }

            Transform target = AvatarAudioSafetyPathUtility.ResolveRelativePath(targetRoot, path);
            return target != null ? target.gameObject : null;
        }

        private static GameObject ResolveCachedBuildTargetObject(
            IReadOnlyList<GameObject> cachedTargetObjects,
            int entryIndex)
        {
            if (cachedTargetObjects == null || entryIndex < 0 || entryIndex >= cachedTargetObjects.Count)
            {
                return null;
            }

            return cachedTargetObjects[entryIndex];
        }

        private static Rect GetIndentedRect(Rect rect)
        {
            float x = rect.x + ResultLabelWidth + HorizontalGap;
            float width = Mathf.Max(0f, rect.width - ResultLabelWidth - HorizontalGap);
            return new Rect(x, rect.y, width, rect.height);
        }

        private static void DrawDetailRow(string label, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            EditorGUILayout.LabelField(label + ": " + value, GetDetailStyle());
        }

        private static int GetRowKey(AvatarAudioScanResult result)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + (result.audioSource != null ? result.audioSource.GetInstanceID() : 0);
                hash = (hash * 31) + (result.path != null ? result.path.GetHashCode() : 0);
                hash = (hash * 31) + result.result.GetHashCode();
                return hash;
            }
        }

        private static int GetBuildRowKey(AvatarAudioSafetyBuildResultEntry entry)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + (entry.path != null ? entry.path.GetHashCode() : 0);
                hash = (hash * 31) + entry.status.GetHashCode();
                hash = (hash * 31) + entry.evaluationResult.GetHashCode();
                hash = (hash * 31) + (entry.clipName != null ? entry.clipName.GetHashCode() : 0);
                return hash;
            }
        }

        private static bool GetFoldoutState(int rowKey)
        {
            bool expanded;
            return DetailFoldouts.TryGetValue(rowKey, out expanded) && expanded;
        }

        private static bool GetBuildFoldoutState(int rowKey)
        {
            bool expanded;
            return BuildDetailFoldouts.TryGetValue(rowKey, out expanded) && expanded;
        }

        private static string GetResultLabel(AvatarAudioSafetyResultKind resultKind)
        {
            return AvatarAudioSafetyUiText.GetResultLabel(resultKind);
        }

        private static GUIStyle GetResultStyle(AvatarAudioSafetyResultKind resultKind)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = GetResultTextColor(resultKind);
            return style;
        }

        private static GUIStyle GetBuildResultStyle(AvatarAudioSafetyBuildResultEntryStatus status)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = GetBuildResultTextColor(status);
            return style;
        }

        private static GUIStyle GetMainInfoStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = GetBodyTextColor();
            style.clipping = TextClipping.Clip;
            return style;
        }

        private static GUIStyle GetPathStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = GetSecondaryTextColor();
            style.clipping = TextClipping.Clip;
            return style;
        }

        private static GUIStyle GetDetailStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            style.normal.textColor = GetDetailTextColor();
            return style;
        }

        private static GUIStyle GetComparisonHeaderStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
            style.normal.textColor = GetSecondaryTextColor();
            return style;
        }

        private static GUIStyle GetComparisonValueStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            style.normal.textColor = GetBodyTextColor();
            return style;
        }

        private static GUIStyle GetComparisonArrowStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = GetBuildResultTextColor(AvatarAudioSafetyBuildResultEntryStatus.Changed);
            return style;
        }

        private static void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, GetSeparatorColor());
            EditorGUILayout.Space(RowSpacing);
        }

        private static Color GetBodyTextColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.84f, 0.84f, 0.84f)
                : new Color(0.18f, 0.18f, 0.18f);
        }

        private static Color GetSecondaryTextColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.72f, 0.72f, 0.72f)
                : new Color(0.32f, 0.32f, 0.32f);
        }

        private static Color GetDetailTextColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.78f, 0.78f, 0.78f)
                : new Color(0.24f, 0.24f, 0.24f);
        }

        private static Color GetSeparatorColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(0f, 0f, 0f, 0.12f);
        }

        private static Color GetResultTextColor(AvatarAudioSafetyResultKind resultKind)
        {
            switch (resultKind)
            {
                case AvatarAudioSafetyResultKind.Warning:
                    return new Color(0.78f, 0.56f, 0.16f);
                case AvatarAudioSafetyResultKind.WouldClamp:
                    return new Color(0.78f, 0.22f, 0.17f);
                case AvatarAudioSafetyResultKind.ReportOnly:
                    return new Color(0.16f, 0.46f, 0.78f);
                case AvatarAudioSafetyResultKind.Ignored:
                    return new Color(0.45f, 0.45f, 0.45f);
                case AvatarAudioSafetyResultKind.ManualReview:
                    return new Color(0.55f, 0.22f, 0.66f);
                case AvatarAudioSafetyResultKind.Safe:
                default:
                    return new Color(0.16f, 0.56f, 0.26f);
            }
        }

        private static Color GetBuildResultTextColor(AvatarAudioSafetyBuildResultEntryStatus status)
        {
            switch (status)
            {
                case AvatarAudioSafetyBuildResultEntryStatus.Changed:
                    return new Color(0.78f, 0.22f, 0.17f);
                case AvatarAudioSafetyBuildResultEntryStatus.Skipped:
                    return new Color(0.16f, 0.46f, 0.78f);
                case AvatarAudioSafetyBuildResultEntryStatus.Error:
                    return new Color(0.72f, 0.2f, 0.2f);
                case AvatarAudioSafetyBuildResultEntryStatus.Unchanged:
                default:
                    return new Color(0.16f, 0.56f, 0.26f);
            }
        }

        private static string GetBuildMainInfoText(AvatarAudioSafetyBuildResultEntry entry)
        {
            return string.Format(
                "AudioClip: {0} | ルール: {1}",
                string.IsNullOrEmpty(entry.clipName) ? "-" : entry.clipName,
                AvatarAudioSafetyUiText.GetRuleShortLabel(entry.appliedRule));
        }

        private static string GetBuildSecondaryInfoText(AvatarAudioSafetyBuildResultEntry entry)
        {
            return string.Format(
                "AudioClip: {0} | ルール: {1} | 判定: {2}",
                string.IsNullOrEmpty(entry.clipName) ? "-" : entry.clipName,
                AvatarAudioSafetyUiText.GetRuleShortLabel(entry.appliedRule),
                AvatarAudioSafetyUiText.GetResultLabel(entry.evaluationResult));
        }
    }
}
