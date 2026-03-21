using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyBuildLogger
    {
        public static void LogSkipped(AvatarAudioSafetySettings settings, string reason)
        {
            if (settings == null || string.IsNullOrEmpty(reason))
            {
                return;
            }

            Debug.Log(
                string.Format("[Avatar Audio Safety Guard] Build pass skipped for '{0}': {1}", settings.gameObject.name, reason),
                settings);
        }

        public static void LogItem(
            AvatarAudioSafetySettings settings,
            AvatarAudioSafetyBuildApplyPlan plan,
            AvatarAudioSafetyBuildApplyOutcome outcome)
        {
            if (settings == null || plan == null || plan.Evaluation == null)
            {
                return;
            }

            string path = string.IsNullOrEmpty(plan.Path) ? "./" : plan.Path;
            string reason = plan.Evaluation.Reason;

            if (outcome != null && outcome.AppliedAnyChange)
            {
                string suffix = plan.Evaluation.HasDiagnosticReasons && !string.IsNullOrEmpty(reason) ? " | " + reason : string.Empty;
                Debug.Log(
                    string.Format("[Avatar Audio Safety Guard] build clamped: {0} - {1}{2}", path, outcome.AppliedChangeSummary, suffix),
                    plan.AudioSource);
            }

            if (outcome != null && outcome.HasFailures)
            {
                Debug.LogWarning(
                    string.Format("[Avatar Audio Safety Guard] build apply warning: {0} - {1}", path, outcome.FailureSummary),
                    plan.AudioSource);
            }

            switch (plan.Evaluation.Result)
            {
                case AvatarAudioSafetyResultKind.Warning:
                    Debug.LogWarning(
                        string.Format("[Avatar Audio Safety Guard] warned: {0} - {1}", path, reason),
                        plan.AudioSource);
                    break;
                case AvatarAudioSafetyResultKind.ReportOnly:
                    Debug.Log(
                        string.Format("[Avatar Audio Safety Guard] report_only: {0} - {1}", path, reason),
                        plan.AudioSource);
                    break;
                case AvatarAudioSafetyResultKind.ManualReview:
                    Debug.LogWarning(
                        string.Format("[Avatar Audio Safety Guard] manual_review: {0} - {1}", path, reason),
                        plan.AudioSource);
                    break;
            }
        }

        public static void LogSummary(
            AvatarAudioSafetySettings settings,
            AvatarAudioScanSummary summary,
            int correctedSourceCount)
        {
            if (settings == null || summary == null)
            {
                return;
            }

            Debug.Log(
                string.Format(
                    "[Avatar Audio Safety Guard] Build summary for '{0}': scanned={1}, warnings={2}, corrected_sources={3}, report_only={4}, ignored={5}, manual_review={6}",
                    settings.gameObject.name,
                    summary.scanned,
                    summary.warnings,
                    correctedSourceCount,
                    summary.reportOnly,
                    summary.ignored,
                    summary.manualReview),
                settings);
        }
    }
}
