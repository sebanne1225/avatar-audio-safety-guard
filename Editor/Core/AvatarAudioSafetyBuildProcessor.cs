using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;
using Object = UnityEngine.Object;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyBuildProcessor
    {
        public static AvatarAudioSafetyBuildResultSnapshot Process(
            GameObject avatarRootObject,
            IReadOnlyList<AudioSource> audioSources = null,
            IReadOnlyList<string> sourceAnchorPaths = null)
        {
            if (avatarRootObject == null)
            {
                return null;
            }

            AvatarAudioSafetySettings settings = avatarRootObject.GetComponent<AvatarAudioSafetySettings>();
            if (settings == null)
            {
                return null;
            }

            AvatarAudioSafetySettings sourceSettings = ResolveSourceSettings(settings);
            AvatarAudioSafetyBuildResultSnapshot snapshot = CreateSnapshot(settings, sourceSettings);
            IReadOnlyList<AudioSource> collectedAudioSources = audioSources ?? AvatarAudioSafetyAudioSourceCollector.Collect(settings);
            AvatarAudioThresholdPreset defaultThresholds = settings.ResolveDefaultThresholds();

            try
            {
                if (!settings.ToolEnabled)
                {
                    snapshot.summaryMessage = "有効化がオフのため、この Build では補正を実行しませんでした。";
                    PersistBuildResult(sourceSettings, settings, snapshot, sourceAnchorPaths);
                    AvatarAudioSafetyBuildLogger.LogSkipped(settings, "有効化がオフのため、build clone は変更しません。");
                    AvatarAudioSafetyBuildLogger.LogSummary(sourceSettings ?? settings, snapshot);
                    return snapshot;
                }

                if (settings.Mode != AvatarAudioSafetyMode.ApplyOnBuild)
                {
                    PopulateSnapshotEntries(settings, collectedAudioSources, defaultThresholds, snapshot, false);
                    FinalizeSnapshot(snapshot);
                    snapshot.summaryMessage = "動作モードが「診断のみ」のため、この Build では補正を実行しませんでした。";
                    PersistBuildResult(sourceSettings, settings, snapshot, sourceAnchorPaths);
                    AvatarAudioSafetyBuildLogger.LogSkipped(settings, "診断のみ のため、build clone は変更しません。");
                    AvatarAudioSafetyBuildLogger.LogSummary(sourceSettings ?? settings, snapshot);
                    return snapshot;
                }

                PopulateSnapshotEntries(settings, collectedAudioSources, defaultThresholds, snapshot, true);
                FinalizeSnapshot(snapshot);
                PersistBuildResult(sourceSettings, settings, snapshot, sourceAnchorPaths);
                AvatarAudioSafetyBuildLogger.LogSummary(sourceSettings ?? settings, snapshot);
                return snapshot;
            }
            finally
            {
                CleanupSettingsComponent(settings);
            }
        }

        private static AvatarAudioSafetyBuildResultSnapshot CreateSnapshot(
            AvatarAudioSafetySettings buildSettings,
            AvatarAudioSafetySettings sourceSettings)
        {
            AvatarAudioSafetyBuildResultSnapshot snapshot = new AvatarAudioSafetyBuildResultSnapshot();
            snapshot.executedLocalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            snapshot.avatarName = ResolveAvatarName(buildSettings, sourceSettings);
            snapshot.mode = buildSettings != null ? buildSettings.Mode : AvatarAudioSafetyMode.PreviewOnly;
            return snapshot;
        }

        private static AvatarAudioSafetySettings ResolveSourceSettings(AvatarAudioSafetySettings buildSettings)
        {
            if (buildSettings == null || string.IsNullOrEmpty(buildSettings.SourceSettingsGlobalId))
            {
                return null;
            }

            GlobalObjectId globalObjectId;
            if (!GlobalObjectId.TryParse(buildSettings.SourceSettingsGlobalId, out globalObjectId))
            {
                return null;
            }

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) as AvatarAudioSafetySettings;
        }

        private static bool PersistBuildResult(
            AvatarAudioSafetySettings sourceSettings,
            AvatarAudioSafetySettings buildSettings,
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            IReadOnlyList<string> sourceAnchorPaths)
        {
            if (buildSettings != null && snapshot != null)
            {
                AvatarAudioSafetySessionState.RememberBuildResult(
                    sourceSettings,
                    buildSettings,
                    snapshot,
                    sourceAnchorPaths);
            }

            if (sourceSettings == null || snapshot == null)
            {
                AvatarAudioSafetyBuildLogger.LogResultPersistenceWarning(buildSettings);
                return false;
            }

            sourceSettings.SetLastBuildResult(snapshot);
            AvatarAudioSafetySessionState.RememberSettings(sourceSettings);
            EditorUtility.SetDirty(sourceSettings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(sourceSettings);
            return true;
        }

        private static void PopulateSnapshotEntries(
            AvatarAudioSafetySettings settings,
            IReadOnlyList<AudioSource> audioSources,
            AvatarAudioThresholdPreset defaultThresholds,
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            bool applyChanges)
        {
            if (settings == null || audioSources == null || snapshot == null)
            {
                return;
            }

            for (int i = 0; i < audioSources.Count; i++)
            {
                AudioSource audioSource = audioSources[i];
                AvatarAudioSafetyEvaluationRequest request = AvatarAudioSafetyEvaluator.CreateRequest(settings, audioSource, defaultThresholds);
                AvatarAudioSafetyEvaluation evaluation = AvatarAudioSafetyEvaluator.Evaluate(request);
                AvatarAudioSafetyBuildApplyPlan plan = AvatarAudioSafetyBuildPlanner.CreatePlan(request, evaluation);
                AvatarAudioSafetyBuildApplyOutcome outcome = applyChanges
                    ? AvatarAudioSafetyBuildApplier.Apply(plan)
                    : null;

                snapshot.entries.Add(CreateBuildResultEntry(evaluation, plan, outcome));
                AvatarAudioSafetyBuildLogger.LogItem(settings, plan, outcome);
            }
        }

        private static void FinalizeSnapshot(AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.scanned = snapshot.entries != null ? snapshot.entries.Count : 0;
            snapshot.changed = 0;
            snapshot.skipped = 0;
            snapshot.unchanged = 0;
            snapshot.errors = 0;

            if (snapshot.entries != null)
            {
                for (int i = 0; i < snapshot.entries.Count; i++)
                {
                    AvatarAudioSafetyBuildResultEntry entry = snapshot.entries[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    switch (entry.status)
                    {
                        case AvatarAudioSafetyBuildResultEntryStatus.Changed:
                            snapshot.changed++;
                            break;
                        case AvatarAudioSafetyBuildResultEntryStatus.Error:
                            snapshot.errors++;
                            break;
                        case AvatarAudioSafetyBuildResultEntryStatus.Skipped:
                            snapshot.skipped++;
                            break;
                        case AvatarAudioSafetyBuildResultEntryStatus.Unchanged:
                        default:
                            snapshot.unchanged++;
                            break;
                    }
                }
            }

            snapshot.summaryMessage = BuildSummaryMessage(snapshot);
        }

        private static string BuildSummaryMessage(AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return string.Empty;
            }

            if (snapshot.errors > 0 && snapshot.changed > 0)
            {
                return string.Format(
                    "{0}件を補正しましたが、{1}件で書き込みエラーがありました。Console の詳細を確認してください。",
                    snapshot.changed,
                    snapshot.errors);
            }

            if (snapshot.errors > 0)
            {
                return string.Format(
                    "補正中に {0}件の書き込みエラーがありました。Console の詳細を確認してください。",
                    snapshot.errors);
            }

            if (snapshot.changed > 0 && snapshot.skipped > 0)
            {
                return string.Format(
                    "{0}件を補正し、{1}件はルールや診断結果により変更しませんでした。",
                    snapshot.changed,
                    snapshot.skipped);
            }

            if (snapshot.changed > 0)
            {
                return string.Format("{0}件の AudioSource を Build 時に補正しました。", snapshot.changed);
            }

            if (snapshot.skipped > 0)
            {
                return string.Format(
                    "自動補正はありませんでした。{0}件はルールや診断結果により変更していません。",
                    snapshot.skipped);
            }

            return "補正が必要な AudioSource はありませんでした。";
        }

        private static AvatarAudioSafetyBuildResultEntry CreateBuildResultEntry(
            AvatarAudioSafetyEvaluation evaluation,
            AvatarAudioSafetyBuildApplyPlan plan,
            AvatarAudioSafetyBuildApplyOutcome outcome)
        {
            AvatarAudioSafetyBuildResultEntry entry = new AvatarAudioSafetyBuildResultEntry();
            entry.path = evaluation != null && !string.IsNullOrEmpty(evaluation.Path) ? evaluation.Path : "./";
            entry.clipName = evaluation != null && evaluation.Clip != null ? evaluation.Clip.name : string.Empty;
            entry.appliedRule = evaluation != null ? evaluation.AppliedRule : AvatarAudioSafetyRule.Default;
            entry.evaluationResult = evaluation != null ? evaluation.Result : AvatarAudioSafetyResultKind.Safe;
            entry.reason = evaluation != null ? evaluation.Reason : string.Empty;
            entry.beforeSummary = DescribeBeforeState(evaluation);

            if (outcome != null && outcome.HasFailures)
            {
                entry.status = AvatarAudioSafetyBuildResultEntryStatus.Error;
                entry.detail = outcome.FailureSummary;
                entry.afterSummary = outcome.AppliedAnyChange
                    ? "一部変更あり: " + outcome.AppliedChangeSummary
                    : entry.beforeSummary;
                return entry;
            }

            if (outcome != null && outcome.AppliedAnyChange)
            {
                entry.status = AvatarAudioSafetyBuildResultEntryStatus.Changed;
                entry.detail = outcome.AppliedChangeSummary;
                entry.afterSummary = DescribeAfterState(evaluation, plan);
                return entry;
            }

            if (evaluation != null && evaluation.Result == AvatarAudioSafetyResultKind.Safe)
            {
                entry.status = AvatarAudioSafetyBuildResultEntryStatus.Unchanged;
                entry.detail = "基準内だったため、変更はありませんでした。";
                entry.afterSummary = entry.beforeSummary;
                return entry;
            }

            if (evaluation != null && evaluation.Result == AvatarAudioSafetyResultKind.WouldClamp && plan != null && plan.ShouldApply)
            {
                entry.status = AvatarAudioSafetyBuildResultEntryStatus.Unchanged;
                entry.detail = "補正対象でしたが、Build 時の変更は発生しませんでした。";
                entry.afterSummary = DescribeAfterState(evaluation, plan);
                return entry;
            }

            entry.status = AvatarAudioSafetyBuildResultEntryStatus.Skipped;
            entry.detail = DescribeSkippedDetail(evaluation);
            entry.afterSummary = entry.beforeSummary;
            return entry;
        }

        private static string DescribeBeforeState(AvatarAudioSafetyEvaluation evaluation)
        {
            if (evaluation == null)
            {
                return string.Empty;
            }

            return FormatAudioState(
                evaluation.Volume,
                evaluation.Gain,
                evaluation.FarDistance,
                evaluation.NearDistance,
                evaluation.VolumetricRadius);
        }

        private static string DescribeAfterState(
            AvatarAudioSafetyEvaluation evaluation,
            AvatarAudioSafetyBuildApplyPlan plan)
        {
            if (evaluation == null)
            {
                return string.Empty;
            }

            return FormatAudioState(
                plan != null && plan.ShouldClampVolume ? plan.TargetVolume : evaluation.Volume,
                plan != null && plan.ShouldClampGain ? plan.TargetGain : evaluation.Gain,
                plan != null && plan.ShouldClampFar ? plan.TargetFarDistance : evaluation.FarDistance,
                plan != null && plan.ShouldClampNear ? plan.TargetNearDistance : evaluation.NearDistance,
                plan != null && plan.ShouldClampVolumetricRadius ? plan.TargetVolumetricRadius : evaluation.VolumetricRadius);
        }

        private static string DescribeSkippedDetail(AvatarAudioSafetyEvaluation evaluation)
        {
            if (evaluation == null)
            {
                return "Build 結果を記録できませんでした。";
            }

            switch (evaluation.Result)
            {
                case AvatarAudioSafetyResultKind.Warning:
                    return "警告のみで、Build 時の自動補正対象ではありません。";
                case AvatarAudioSafetyResultKind.ReportOnly:
                    return "ルールが「報告のみ」のため、Build では変更しませんでした。";
                case AvatarAudioSafetyResultKind.Ignored:
                    return "ルールが「対象外」のため、Build では処理しませんでした。";
                case AvatarAudioSafetyResultKind.ManualReview:
                    return "手動確認が必要なため、自動補正しませんでした。";
                case AvatarAudioSafetyResultKind.WouldClamp:
                    return "補正候補でしたが、今回は変更が発生しませんでした。";
                case AvatarAudioSafetyResultKind.Safe:
                default:
                    return "基準内だったため、変更はありませんでした。";
            }
        }

        private static string FormatAudioState(
            float volume,
            float gain,
            float farDistance,
            float nearDistance,
            float volumetricRadius)
        {
            return string.Format(
                "Volume {0:0.##} / Gain {1:0.##} / Far {2:0.##} / Near {3:0.##} / Volumetric Radius {4:0.##}",
                volume,
                gain,
                farDistance,
                nearDistance,
                volumetricRadius);
        }

        private static string ResolveAvatarName(
            AvatarAudioSafetySettings buildSettings,
            AvatarAudioSafetySettings sourceSettings)
        {
            string avatarName = sourceSettings != null
                ? sourceSettings.gameObject.name
                : buildSettings != null
                    ? buildSettings.gameObject.name
                    : string.Empty;

            const string CloneSuffix = "(Clone)";
            if (!string.IsNullOrEmpty(avatarName) && avatarName.EndsWith(CloneSuffix, StringComparison.Ordinal))
            {
                return avatarName.Substring(0, avatarName.Length - CloneSuffix.Length);
            }

            return avatarName;
        }

        private static void CleanupSettingsComponent(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return;
            }

            GameObject avatarRootObject = settings.gameObject;
            Object.DestroyImmediate(settings);
            AvatarAudioSafetyBuildLogger.LogSettingsRemoved(avatarRootObject);
        }
    }
}
