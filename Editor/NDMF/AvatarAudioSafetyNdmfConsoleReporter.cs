using System;
using System.Collections.Generic;
using System.Text;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor.NDMF
{
    internal static class AvatarAudioSafetyNdmfConsoleReporter
    {
        public static void ReportActionableResults(
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            IReadOnlyList<ObjectReference> sourceObjectReferences,
            AvatarAudioSafetyProfile profile)
        {
            if (snapshot == null || !snapshot.HasData)
            {
                return;
            }

            int warningCount = CountEntries(snapshot, AvatarAudioSafetyResultKind.Warning);
            int manualReviewCount = CountEntries(snapshot, AvatarAudioSafetyResultKind.ManualReview);
            int errorCount = snapshot.errors;

            if (snapshot.changed <= 0 && manualReviewCount <= 0 && warningCount <= 0 && errorCount <= 0)
            {
                return;
            }

            ErrorReport.ReportError(CreateSummaryEntry(snapshot, profile, warningCount, manualReviewCount, errorCount));

            if (snapshot.entries == null)
            {
                return;
            }

            for (int i = 0; i < snapshot.entries.Count; i++)
            {
                AvatarAudioSafetyBuildResultEntry entry = snapshot.entries[i];
                if (!ShouldReportRow(entry))
                {
                    continue;
                }

                AvatarAudioSafetyNdmfConsoleError rowError = CreateRowEntry(entry);
                ObjectReference reference = ResolveReference(sourceObjectReferences, i);
                if (reference != null)
                {
                    rowError.AddReference(reference);
                }

                ErrorReport.ReportError(rowError);
            }
        }

        private static AvatarAudioSafetyNdmfConsoleError CreateSummaryEntry(
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            AvatarAudioSafetyProfile profile,
            int warningCount,
            int manualReviewCount,
            int errorCount)
        {
            string title = errorCount > 0 || manualReviewCount > 0 || warningCount > 0
                ? "Avatar Audio Safety Guard: Build 時の注目点があります"
                : "Avatar Audio Safety Guard: Build 時に補正を適用しました";

            StringBuilder details = new StringBuilder();
            details.AppendLine("対象アバター: " + GetAvatarName(snapshot));
            details.AppendLine("実行モード: " + AvatarAudioSafetyUiText.GetModeLabel(snapshot.mode));
            details.AppendLine("判定プロファイル: " + AvatarAudioSafetyUiText.GetProfileLabel(profile));
            details.AppendLine("走査数: " + snapshot.scanned);
            details.AppendLine("変更: " + snapshot.changed);
            details.AppendLine("警告: " + warningCount);
            details.AppendLine("手動確認: " + manualReviewCount);
            details.AppendLine("エラー: " + errorCount);

            if (!string.IsNullOrEmpty(snapshot.summaryMessage))
            {
                details.AppendLine("概要: " + snapshot.summaryMessage);
            }

            AppendChangedSummary(details, snapshot);

            ErrorSeverity severity = errorCount > 0 || manualReviewCount > 0 || warningCount > 0
                ? ErrorSeverity.NonFatal
                : ErrorSeverity.Information;

            return new AvatarAudioSafetyNdmfConsoleError(
                severity,
                title,
                details.ToString().TrimEnd());
        }

        private static AvatarAudioSafetyNdmfConsoleError CreateRowEntry(AvatarAudioSafetyBuildResultEntry entry)
        {
            string objectName = GetEntryObjectName(entry);
            string title;
            ErrorSeverity severity;

            if (entry.status == AvatarAudioSafetyBuildResultEntryStatus.Error)
            {
                title = "Build エラー: " + objectName;
                severity = ErrorSeverity.NonFatal;
            }
            else if (entry.evaluationResult == AvatarAudioSafetyResultKind.ManualReview)
            {
                title = "手動確認: " + objectName;
                severity = ErrorSeverity.NonFatal;
            }
            else
            {
                title = "補正を適用: " + objectName;
                severity = ErrorSeverity.Information;
            }

            StringBuilder details = new StringBuilder();

            if (!string.IsNullOrEmpty(entry.reason))
            {
                details.AppendLine("理由: " + entry.reason);
            }

            if (!string.IsNullOrEmpty(entry.detail))
            {
                details.AppendLine("詳細: " + entry.detail);
            }

            if (!string.IsNullOrEmpty(entry.beforeSummary))
            {
                details.AppendLine("Before: " + entry.beforeSummary);
            }

            if (!string.IsNullOrEmpty(entry.afterSummary))
            {
                details.AppendLine("After: " + entry.afterSummary);
            }

            details.AppendLine("Path: " + (string.IsNullOrEmpty(entry.path) ? "./" : entry.path));

            return new AvatarAudioSafetyNdmfConsoleError(
                severity,
                title,
                details.ToString().TrimEnd());
        }

        private static int CountEntries(
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            AvatarAudioSafetyResultKind resultKind)
        {
            if (snapshot == null || snapshot.entries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < snapshot.entries.Count; i++)
            {
                AvatarAudioSafetyBuildResultEntry entry = snapshot.entries[i];
                if (entry != null && entry.evaluationResult == resultKind)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ShouldReportRow(AvatarAudioSafetyBuildResultEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            return entry.status == AvatarAudioSafetyBuildResultEntryStatus.Error
                || entry.evaluationResult == AvatarAudioSafetyResultKind.ManualReview;
        }

        private static void AppendChangedSummary(StringBuilder details, AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (details == null || snapshot == null || snapshot.entries == null || snapshot.changed <= 0)
            {
                return;
            }

            const int MaxListedChangedEntries = 3;
            List<string> changedNames = new List<string>();

            for (int i = 0; i < snapshot.entries.Count; i++)
            {
                AvatarAudioSafetyBuildResultEntry entry = snapshot.entries[i];
                if (entry == null || entry.status != AvatarAudioSafetyBuildResultEntryStatus.Changed)
                {
                    continue;
                }

                if (changedNames.Count < MaxListedChangedEntries)
                {
                    changedNames.Add(GetEntryObjectName(entry));
                }
            }

            if (changedNames.Count == 0)
            {
                return;
            }

            details.AppendLine("補正対象(抜粋): " + string.Join(", ", changedNames));

            int remainingCount = snapshot.changed - changedNames.Count;
            if (remainingCount > 0)
            {
                details.AppendLine("補正対象: ほか " + remainingCount + " 件");
            }
        }

        private static ObjectReference ResolveReference(IReadOnlyList<ObjectReference> sourceObjectReferences, int index)
        {
            if (sourceObjectReferences == null || index < 0 || index >= sourceObjectReferences.Count)
            {
                return null;
            }

            return sourceObjectReferences[index];
        }

        private static string GetAvatarName(AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.avatarName))
            {
                return "(unknown)";
            }

            return snapshot.avatarName;
        }

        private static string GetEntryObjectName(AvatarAudioSafetyBuildResultEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.path) || entry.path == "./")
            {
                return "(root)";
            }

            int separatorIndex = entry.path.LastIndexOf('/');
            if (separatorIndex >= 0 && separatorIndex + 1 < entry.path.Length)
            {
                return entry.path.Substring(separatorIndex + 1);
            }

            return entry.path;
        }
    }

    internal sealed class AvatarAudioSafetyNdmfConsoleError : SimpleError
    {
        private static readonly Localizer EmptyLocalizer =
            new Localizer("en-US", () => new List<(string, Func<string, string>)>());

        private readonly ErrorSeverity severity;
        private readonly string title;
        private readonly string details;

        public AvatarAudioSafetyNdmfConsoleError(
            ErrorSeverity severity,
            string title,
            string details)
        {
            this.severity = severity;
            this.title = title ?? string.Empty;
            this.details = details;
        }

        public override Localizer Localizer
        {
            get { return EmptyLocalizer; }
        }

        public override string TitleKey
        {
            get { return "aasg.ndmf.console"; }
        }

        public override ErrorSeverity Severity
        {
            get { return severity; }
        }

        public override string FormatTitle()
        {
            return title;
        }

        public override string FormatDetails()
        {
            return string.IsNullOrEmpty(details) ? null : details;
        }

        public override string FormatHint()
        {
            return null;
        }
    }
}
