using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyScanActions
    {
        public static bool RunDryRun(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return false;
            }

            AvatarAudioSafetySettingsIdUtility.TryEnsureSourceSettingsId(settings);
            AvatarAudioSafetyScanReport report = AvatarAudioSafetyScanner.Scan(settings);
            AvatarAudioSafetySessionState.RememberSettings(settings);

            Undo.RecordObject(settings, "Avatar Audio Safety Guard Dry Run");
            settings.SetScanResults(report.Results, report.Summary);
            EditorUtility.SetDirty(settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
            AvatarAudioSafetyReportWindow.RepaintOpenWindows();

            Debug.Log(
                "[Avatar Audio Safety Guard] Dry Run 走査が完了しました。 " + AvatarAudioSafetyUiText.GetSummaryText(report.Summary),
                settings);

            return true;
        }

        public static bool RefreshDetectedResults(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return false;
            }

            AvatarAudioSafetyScanReport report = AvatarAudioSafetyScanner.Reclassify(settings);

            Undo.RecordObject(settings, "Avatar Audio Safety Guard Reclassify Results");
            settings.SetScanResults(report.Results, report.Summary);
            EditorUtility.SetDirty(settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
            AvatarAudioSafetyReportWindow.RepaintOpenWindows();
            return true;
        }

        public static void RefreshDetectedResultsIfClassificationChanged(
            AvatarAudioSafetySettings settings,
            string previousSignature)
        {
            if (settings == null)
            {
                return;
            }

            string currentSignature = GetClassificationSignature(settings);
            if (string.Equals(previousSignature, currentSignature, System.StringComparison.Ordinal))
            {
                return;
            }

            RefreshDetectedResults(settings);
        }

        public static string GetClassificationSignature(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return string.Empty;
            }

            StringBuilder signature = new StringBuilder();
            AvatarAudioThresholdPreset resolvedThresholds = settings.ResolveDefaultThresholds();

            signature.Append((int)settings.Profile).Append('|');
            signature.AppendFormat(
                "{0:0.###}|{1:0.###}|{2:0.###}|",
                resolvedThresholds != null ? resolvedThresholds.maxGain : 0f,
                resolvedThresholds != null ? resolvedThresholds.maxFarDistance : 0f,
                resolvedThresholds != null ? resolvedThresholds.maxVolume : 0f);
            signature.Append(settings.WarnOnMissingVrcSpatialAudioSource ? '1' : '0');
            signature.Append(settings.WarnOnCustomRolloff ? '1' : '0');
            signature.Append(settings.WarnOnLoopWithLongRange ? '1' : '0');
            signature.Append(settings.WarnOnNon3DAudio ? '1' : '0');
            signature.Append('|');

            IReadOnlyList<AvatarAudioSourceRuleEntry> rules = settings.PerSourceRules;
            if (rules != null)
            {
                for (int i = 0; i < rules.Count; i++)
                {
                    AvatarAudioSourceRuleEntry rule = rules[i];
                    if (rule == null)
                    {
                        signature.Append("<null>|");
                        continue;
                    }

                    signature.Append(AvatarAudioSafetyPathUtility.Normalize(rule.path)).Append('|');
                    signature.Append((int)rule.rule).Append('|');

                    AvatarAudioThresholdPreset customThresholds = rule.customThresholds;
                    signature.AppendFormat(
                        "{0:0.###}|{1:0.###}|{2:0.###}|",
                        customThresholds != null ? customThresholds.maxGain : 0f,
                        customThresholds != null ? customThresholds.maxFarDistance : 0f,
                        customThresholds != null ? customThresholds.maxVolume : 0f);
                }
            }

            return signature.ToString();
        }
    }
}
