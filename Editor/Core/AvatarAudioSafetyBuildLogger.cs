using System.Text;
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
            AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (settings == null || snapshot == null || !snapshot.HasData)
            {
                return;
            }

            string avatarName = string.IsNullOrEmpty(snapshot.avatarName) ? settings.gameObject.name : snapshot.avatarName;
            string executedLocalTime = string.IsNullOrEmpty(snapshot.executedLocalTime) ? "不明" : snapshot.executedLocalTime;
            string modeLabel = GetModeLabel(snapshot.mode);
            int warningCount = CountWarningEntries(snapshot);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[Avatar Audio Safety Guard] Build 結果");
            builder.AppendLine("対象アバター: " + avatarName);
            builder.AppendLine("実行時刻: " + executedLocalTime);
            builder.AppendLine("実行モード: " + modeLabel);
            builder.AppendLine(string.Format(
                "要約: 走査 {0}件 / 変更 {1}件 / スキップ {2}件 / 警告 {3}件 / エラー {4}件 / 変更なし {5}件",
                snapshot.scanned,
                snapshot.changed,
                snapshot.skipped,
                warningCount,
                snapshot.errors,
                snapshot.unchanged));

            if (!string.IsNullOrEmpty(snapshot.summaryMessage))
            {
                builder.AppendLine("概要: " + snapshot.summaryMessage);
            }

            AppendDetailLines(builder, snapshot);
            Debug.Log(builder.ToString().TrimEnd(), settings);
        }

        public static void LogSettingsRemoved(GameObject avatarRootObject)
        {
            if (avatarRootObject == null)
            {
                return;
            }

            Debug.Log(
                string.Format(
                    "[Avatar Audio Safety Guard] Removed editor-only AvatarAudioSafetySettings from build clone '{0}'.",
                    avatarRootObject.name),
                avatarRootObject);
        }

        public static void LogResultPersistenceWarning(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Debug.LogWarning(
                "[Avatar Audio Safety Guard] Build 結果の保存先となる AvatarAudioSafetySettings を source 側で解決できず、前回 Build 補正結果を保持できませんでした。",
                settings);
        }

        private static string GetModeLabel(AvatarAudioSafetyMode mode)
        {
            switch (mode)
            {
                case AvatarAudioSafetyMode.ApplyOnBuild:
                    return "Build時に補正";
                case AvatarAudioSafetyMode.PreviewOnly:
                default:
                    return "診断のみ";
            }
        }

        private static int CountWarningEntries(AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (snapshot == null || snapshot.entries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < snapshot.entries.Count; i++)
            {
                AvatarAudioSafetyBuildResultEntry entry = snapshot.entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.evaluationResult == AvatarAudioSafetyResultKind.Warning
                    || entry.evaluationResult == AvatarAudioSafetyResultKind.ManualReview)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AppendDetailLines(StringBuilder builder, AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (builder == null || snapshot == null)
            {
                return;
            }

            if (snapshot.entries == null || snapshot.entries.Count == 0)
            {
                return;
            }

            int detailCount = 0;
            for (int i = 0; i < snapshot.entries.Count; i++)
            {
                AvatarAudioSafetyBuildResultEntry entry = snapshot.entries[i];
                if (!ShouldIncludeInSummaryDetails(entry))
                {
                    continue;
                }

                if (detailCount == 0)
                {
                    builder.AppendLine("明細:");
                }

                detailCount++;
                builder.AppendLine("- AudioSource: " + GetEntryDisplayName(entry));

                string resultLabel = AvatarAudioSafetyUiText.GetResultLabel(entry.evaluationResult);
                string statusLabel = AvatarAudioSafetyUiText.GetBuildEntryStatusLabel(entry.status);
                builder.AppendLine("  判定: " + statusLabel + " / " + resultLabel);

                if (!string.IsNullOrEmpty(entry.reason))
                {
                    builder.AppendLine("  理由: " + entry.reason);
                }

                if (!string.IsNullOrEmpty(entry.beforeSummary))
                {
                    builder.AppendLine("  Before: " + entry.beforeSummary);
                }

                if (!string.IsNullOrEmpty(entry.afterSummary))
                {
                    builder.AppendLine("  After : " + entry.afterSummary);
                }

                builder.AppendLine("  Path  : " + (string.IsNullOrEmpty(entry.path) ? "./" : entry.path));
            }

            if (detailCount == 0)
            {
                builder.AppendLine("明細: 補足が必要な項目はありません。");
            }
        }

        private static bool ShouldIncludeInSummaryDetails(AvatarAudioSafetyBuildResultEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            if (entry.status != AvatarAudioSafetyBuildResultEntryStatus.Unchanged)
            {
                return true;
            }

            return entry.evaluationResult == AvatarAudioSafetyResultKind.Warning
                || entry.evaluationResult == AvatarAudioSafetyResultKind.WouldClamp
                || entry.evaluationResult == AvatarAudioSafetyResultKind.ReportOnly
                || entry.evaluationResult == AvatarAudioSafetyResultKind.ManualReview;
        }

        private static string GetEntryDisplayName(AvatarAudioSafetyBuildResultEntry entry)
        {
            if (entry == null)
            {
                return "(unknown)";
            }

            string path = entry.path ?? string.Empty;
            if (string.IsNullOrEmpty(path) || path == "./")
            {
                return string.IsNullOrEmpty(entry.clipName) ? "(root)" : entry.clipName;
            }

            int separatorIndex = path.LastIndexOf('/');
            if (separatorIndex >= 0 && separatorIndex + 1 < path.Length)
            {
                return path.Substring(separatorIndex + 1);
            }

            return path;
        }
    }
}
