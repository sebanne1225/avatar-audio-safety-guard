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
        private const float RowSpacing = 2f;
        private const float HorizontalGap = 4f;
        private static readonly Dictionary<int, bool> DetailFoldouts = new Dictionary<int, bool>();

        public static void Draw(
            IReadOnlyList<AvatarAudioScanResult> results,
            ref Vector2 scrollPosition,
            Func<AvatarAudioScanResult, bool> filter = null)
        {
            if (results == null || results.Count == 0)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.ResultsEmptyMessage, MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(180f));

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

                DrawRow(result, hasVisibleRow);
                hasVisibleRow = true;
            }

            if (!hasVisibleRow)
            {
                EditorGUILayout.HelpBox(AvatarAudioSafetyUiText.FilterNoMatchMessage, MessageType.None);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawRow(AvatarAudioScanResult result, bool drawSeparator)
        {
            bool hasDetails = !string.IsNullOrEmpty(result.reason) || !string.IsNullOrEmpty(result.plannedChange);
            int rowKey = GetRowKey(result);
            bool isExpanded = hasDetails && GetFoldoutState(rowKey);

            if (drawSeparator)
            {
                DrawSeparator();
            }

            DrawPrimaryRow(result);
            DrawMainInfoRow(result, ref isExpanded, rowKey, hasDetails);
            DrawPathRow(result);

            if (isExpanded)
            {
                DrawExpandedDetails(result);
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

        private static void DrawMainInfoRow(AvatarAudioScanResult result, ref bool isExpanded, int rowKey, bool hasDetails)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect contentRect = GetIndentedRect(rowRect);
            Rect mainInfoRect = contentRect;

            if (hasDetails)
            {
                Rect detailsRect = new Rect(contentRect.xMax - DetailsButtonWidth, contentRect.y, DetailsButtonWidth, contentRect.height);
                mainInfoRect.xMax = detailsRect.xMin - HorizontalGap;
                string buttonLabel = isExpanded ? AvatarAudioSafetyUiText.HideDetailsButton : AvatarAudioSafetyUiText.DetailsButton;
                if (GUI.Button(detailsRect, buttonLabel, EditorStyles.miniButton))
                {
                    isExpanded = !isExpanded;
                    DetailFoldouts[rowKey] = isExpanded;
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

        private static void DrawExpandedDetails(AvatarAudioScanResult result)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(RowSpacing);
            EditorGUI.indentLevel++;
            DrawDetailRow(AvatarAudioSafetyUiText.GetReasonLabel(), result.reason);
            DrawDetailRow(AvatarAudioSafetyUiText.GetPlanLabel(), result.plannedChange);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
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

        private static bool GetFoldoutState(int rowKey)
        {
            bool expanded;
            return DetailFoldouts.TryGetValue(rowKey, out expanded) && expanded;
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

        private static GUIStyle GetMainInfoStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = new Color(0.3f, 0.3f, 0.3f);
            style.clipping = TextClipping.Clip;
            return style;
        }

        private static GUIStyle GetPathStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = new Color(0.48f, 0.48f, 0.48f);
            style.clipping = TextClipping.Clip;
            return style;
        }

        private static GUIStyle GetDetailStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            style.normal.textColor = new Color(0.35f, 0.35f, 0.35f);
            return style;
        }

        private static void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.12f));
            EditorGUILayout.Space(RowSpacing);
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
    }
}
