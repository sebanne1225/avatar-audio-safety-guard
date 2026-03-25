using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyUiText
    {
        public static readonly GUIContent EnabledLabel = new GUIContent("有効化");
        public static readonly GUIContent ModeLabel = new GUIContent("動作モード");
        public static readonly GUIContent ProfileLabel = new GUIContent("判定プロファイル");
        public static readonly GUIContent MaxGainLabel = new GUIContent("最大 Gain");
        public static readonly GUIContent MaxFarDistanceLabel = new GUIContent("最大距離（Far Distance）");
        public static readonly GUIContent MaxVolumeLabel = new GUIContent("最大音量（Max Volume）");
        public static readonly GUIContent WarnMissingSpatialAudioLabel = new GUIContent("VRC Spatial Audio Source 未設定を警告");
        public static readonly GUIContent WarnCustomRolloffLabel = new GUIContent("Custom Rolloff 使用を警告");
        public static readonly GUIContent WarnLoopLongRangeLabel = new GUIContent("Loop + 長距離設定を警告");
        public static readonly GUIContent WarnNon3DAudioLabel = new GUIContent("2D Audio を警告");
        public static readonly GUIContent RuleTargetLabel = new GUIContent("対象 GameObject");
        public static readonly GUIContent PathLabel = new GUIContent("パス");
        public static readonly GUIContent RuleLabel = new GUIContent("ルール");
        public static readonly GUIContent MemoLabel = new GUIContent("メモ");
        public static readonly GUIContent SettingsLabel = new GUIContent("設定");
        public static readonly GUIContent AvatarRootLabel = new GUIContent("アバタールート");

        public static readonly string[] ModeOptions =
        {
            "診断のみ",
            "Build時に補正",
        };

        public static readonly string[] ProfileOptions =
        {
            "厳しめ",
            "標準",
            "カスタム",
        };

        public static readonly string[] RuleOptions =
        {
            "通常",
            "対象外",
            "報告のみ",
            "個別しきい値",
        };

        public static readonly string[] ReportFilterOptions =
        {
            "すべて",
            "問題なし以外",
            "Build時に補正予定",
            "報告のみ",
            "対象外",
        };

        public const string BasicSectionTitle = "基本設定";
        public const string ThresholdSectionTitle = "しきい値";
        public const string DiagnosticsSectionTitle = "診断オプション";
        public const string PerSourceRulesSectionTitle = "音源ごとの設定";
        public const string ToolsSectionTitle = "実行";
        public const string DetectedAudioSourcesSectionTitle = "検出された AudioSource";
        public const string LastBuildResultSectionTitle = "前回 Build 補正結果";
        public const string SummaryLabel = "要約";
        public const string LastScanLabel = "最終走査";
        public const string ReportWindowTitle = "診断レポート";
        public const string ReportTargetSectionTitle = "表示中の設定";
        public const string LastBuildExecutedLabel = "前回 Build 実行";
        public const string LastBuildAvatarLabel = "対象アバター";
        public const string LastBuildModeLabel = "実行モード";
        public const string LastBuildMessageLabel = "メモ";
        public const string ReportTargetDescription = "このウィンドウは、選択した AvatarAudioSafetySettings に保存されている検出結果へ現在の設定を反映して表示します。";
        public const string ReportWindowDescription = "Inspector の一覧と同じ見え方で、保存済みの検出結果に現在の設定を反映した診断を見返せます。";
        public const string FilterDescription = "表示したい結果だけに絞り込めます。「問題なし以外」は対象外や報告のみも含みます。";
        public const string NoSummaryYet = "まだ AudioSource は走査されていません。";
        public const string NoLastBuildResultYet = "まだ前回 Build 補正結果はありません。";
        public const string MissingSettingsMessage = "AvatarAudioSafetySettings を指定してください。アバターのルートに付けた設定コンポーネントを選ぶと結果を確認できます。";
        public const string MissingSettingsComponentMessage = "設定コンポーネントが見つかりません。";
        public const string ResultsEmptyMessage = "まだ走査結果がありません。「AudioSource を走査」を実行してください。";
        public const string BuildResultEntriesEmptyMessage = "保存済みの Build 明細はありません。";
        public const string FilterNoMatchMessage = "現在の絞り込み条件に一致する結果はありません。";
        public const string ReportUsesStoredResultsDescription = "最新の検出結果は AvatarAudioSafetySettings に保持されます。設定変更時は分類だけ即時更新され、Hierarchy の追加・削除・構造変化を拾うには再走査が必要です。";
        public const string LastBuildResultDescription = "前回の Build 履歴です。ここでは履歴を補助的に見返し、必要なら「結果を表示」で要約を確認できます。";
        public const string CachedBuildResultNotice = "settings の参照が一時的に外れているため、最後に取得できた Build 結果を保持表示しています。参照が戻ると通常表示に戻ります。";
        public const string OperationDialogTitle = "Avatar Audio Safety Guard";
        public const string OperationDialogOkButton = "OK";
        public const string ClearDialogConfirmButton = "クリアする";
        public const string CancelButtonLabel = "キャンセル";
        public const string AddRuleButton = "ルールを追加";
        public const string AddRuleMiniButton = "追加";
        public const string RemoveRuleButton = "削除";
        public const string OpenReportButton = "診断レポートを開く";
        public const string ScanAudioSourcesButton = "AudioSource を走査";
        public const string ClearResultsButton = "結果をクリア";
        public const string ClearLastBuildResultButton = "前回 Build 結果をクリア";
        public const string RelogLastBuildResultButton = "結果を表示";
        public const string LastBuildDetailsFoldout = "明細を見る";
        public const string DetailsButton = "詳細";
        public const string HideDetailsButton = "閉じる";
        public const string BuildBeforeLabel = "Build前";
        public const string BuildAfterLabel = "Build後";
        public const string BuildComparisonArrow = "→";
        public const string BuildComparisonFlowLabel = "補正";
        public const string BuildEntryResultLabel = "結果";
        public const string ComparisonEmptyValue = "変化なし / 情報なし";
        public const string PathRuleHelpText = "対象 GameObject を選ぶと、アバタールート基準のパスが自動入力されます。必要ならパスを手入力で微調整することもできます。";
        public const string PerSourceRulesReportAddHint = "診断レポートからも、この設定の個別ルールを切り替えられます。必要に応じて、ここで内容を確認・調整してください。";
        public const string NoRulesMessage = "音源ごとの設定はまだありません。必要な AudioSource だけ追加してください。";
        public const string CustomThresholdDescription = "カスタムでは、下の値を直接編集できます。";
        public const string InlineRuleLabel = "個別ルール";
        public const string UnresolvedRuleTargetMessage = "現在のパスから GameObject を見つけられません。手入力したパスを確認してください。";
        public const string InvalidRuleTargetMessage = "この GameObject は AvatarAudioSafetySettings の子ではありません。アバター内の GameObject を選んでください。";
        public const string AddRuleUnavailableMissingSettings = "source settings を安全に解決できる時だけ追加できます。";
        public const string AddRuleUnavailableMissingTarget = "source 側の対象 GameObject を解決できる行だけ追加できます。";
        public const string ResultsPersistenceHint = "最新の検出結果は、この AvatarAudioSafetySettings に保持されます。設定変更時は分類だけ即時更新され、Hierarchy の変化は再走査で取り込みます。";
        public const string AddRuleCompletedDialogMessage = "個別ルールを追加しました。Settings 側で確認してください";
        public const string ClearLastBuildResultDialogTitle = "前回 Build 結果をクリア";
        public const string ClearLastBuildResultDialogMessage = "保存されている前回 Build 結果をクリアします。続行しますか？";
        public const string ClearResultsDialogTitle = "結果をクリア";
        public const string ClearResultsDialogMessage = "保存されている診断結果をクリアします。続行しますか？";
        public const string LastBuildResultConsoleHint = "ボタンから、前回 Build 結果の要約をダイアログで確認できます。";

        public const string BasicSectionDescription = "まずはこのツールを使うかどうかと、Dry Run / Build 時の基本方針を決めます。";
        public const string ThresholdSectionDescription = "音量や距離の判定基準です。Dry Run でもこの値を使って「Build時に補正予定」や警告を判定します。";
        public const string DiagnosticsSectionDescription = "音量の上限だけでは拾いにくい設定ミスを、追加で警告したいときに使います。";
        public const string PerSourceRulesSectionDescription = "特定の AudioSource だけ別ルールにしたいときに使います。BGM やギミック用の音源を個別に扱いたい場面向けです。";
        public const string ToolsSectionDescription = "Inspector からの走査は常に非破壊です。再走査では Hierarchy から AudioSource を取り直し、設定変更時は保存済みの検出結果を自動で再判定します。";
        public const string DetectedAudioSourcesSectionDescription = "保存済みの検出結果に、現在の設定を反映した診断一覧です。ObjectField をクリックすると対象 GameObject を追えます。";
        public const string LastBuildResultSectionDescription = "前回の Build で実際に何を変更したかを、あとから確認できます。";
        public const string ReportFilterSectionTitle = "表示フィルタ";

        public static MessageType GetDryRunBehaviorMessageType(bool toolEnabled, AvatarAudioSafetyMode mode)
        {
            if (!toolEnabled)
            {
                return MessageType.Warning;
            }

            if (mode == AvatarAudioSafetyMode.ApplyOnBuild)
            {
                return MessageType.Info;
            }

            return MessageType.None;
        }

        public static string GetDryRunBehaviorDescription(bool toolEnabled, AvatarAudioSafetyMode mode)
        {
            if (!toolEnabled)
            {
                return "有効化がオフでも Dry Run の走査は実行できます。Build 時は build clone への補正をスキップします。";
            }

            if (mode == AvatarAudioSafetyMode.ApplyOnBuild)
            {
                return "Dry Run は非破壊です。Build時に補正 では、Build 時だけ build clone に補正を適用し、scene / prefab 本体は変更しません。";
            }

            return "診断のみ では、Dry Run の一覧更新だけを行い、Build 時も値を変更しません。";
        }

        public static string GetThresholdReadonlyDescription(AvatarAudioSafetyProfile profile)
        {
            return string.Format(
                "{0} を選択中です。細かく調整したい場合は カスタム に切り替えてください。",
                GetProfileLabel(profile));
        }

        public static string GetRuleEntryTitle(int index)
        {
            return "音源ごとの設定 " + (index + 1);
        }

        public static string GetSummaryText(AvatarAudioScanSummary summary)
        {
            if (summary == null || summary.scanned <= 0)
            {
                return NoSummaryYet;
            }

            return string.Format(
                "走査数: {0} | 問題なし: {1} | 警告: {2} | Build時に補正予定: {3} | 報告のみ: {4} | 対象外: {5}",
                summary.scanned,
                summary.safe,
                summary.warnings,
                summary.wouldClamp,
                summary.reportOnly,
                summary.ignored);
        }

        public static string GetBuildSummaryText(AvatarAudioSafetyBuildResultSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.HasData)
            {
                return NoLastBuildResultYet;
            }

            return string.Format(
                "走査数: {0} | 変更: {1} | 変更なし: {2} | スキップ: {3} | エラー: {4}",
                snapshot.scanned,
                snapshot.changed,
                snapshot.unchanged,
                snapshot.skipped,
                snapshot.errors);
        }

        public static string GetScanCompletedDialogMessage(IReadOnlyList<AvatarAudioScanResult> results)
        {
            int detectedCount = results != null ? results.Count : 0;
            StringBuilder builder = new StringBuilder();
            builder.Append(detectedCount).Append(" 件の AudioSource を検出しました");

            if (detectedCount <= 0)
            {
                return builder.ToString();
            }

            builder.AppendLine();
            builder.AppendLine();

            const int MaxVisibleEntries = 5;
            int listedCount = 0;

            for (int i = 0; i < detectedCount && listedCount < MaxVisibleEntries; i++)
            {
                AvatarAudioScanResult result = results[i];
                builder.Append("- ").AppendLine(GetScanResultDisplayName(result));
                listedCount++;
            }

            int remainingCount = detectedCount - listedCount;
            if (remainingCount > 0)
            {
                builder.Append("ほか ").Append(remainingCount).Append(" 件");
            }

            return builder.ToString().TrimEnd();
        }

        public static string GetBuildResultDialogMessage(
            AvatarAudioSafetyBuildResultSnapshot snapshot,
            AvatarAudioSafetyProfile? profile = null)
        {
            if (snapshot == null || !snapshot.HasData)
            {
                return NoLastBuildResultYet;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(SummaryLabel + ": " + GetBuildSummaryText(snapshot));

            if (!string.IsNullOrEmpty(snapshot.executedLocalTime))
            {
                builder.AppendLine(LastBuildExecutedLabel + ": " + snapshot.executedLocalTime);
            }

            builder.AppendLine(LastBuildModeLabel + ": " + GetModeLabel(snapshot.mode));

            if (profile.HasValue)
            {
                builder.AppendLine(ProfileLabel.text + ": " + GetProfileLabel(profile.Value));
            }

            if (!string.IsNullOrEmpty(snapshot.summaryMessage))
            {
                builder.AppendLine(LastBuildMessageLabel + ": " + snapshot.summaryMessage);
            }

            return builder.ToString().TrimEnd();
        }

        public static string GetResultLabel(AvatarAudioSafetyResultKind resultKind)
        {
            switch (resultKind)
            {
                case AvatarAudioSafetyResultKind.Warning:
                    return "警告";
                case AvatarAudioSafetyResultKind.WouldClamp:
                    return "Build時に補正予定";
                case AvatarAudioSafetyResultKind.ReportOnly:
                    return "報告のみ";
                case AvatarAudioSafetyResultKind.Ignored:
                    return "対象外";
                case AvatarAudioSafetyResultKind.ManualReview:
                    return "手動確認";
                case AvatarAudioSafetyResultKind.Safe:
                default:
                    return "問題なし";
            }
        }

        public static string GetRuleLabel(AvatarAudioSafetyRule rule)
        {
            switch (rule)
            {
                case AvatarAudioSafetyRule.Ignore:
                    return "対象外";
                case AvatarAudioSafetyRule.ReportOnly:
                    return "報告のみ";
                case AvatarAudioSafetyRule.CustomThreshold:
                    return "個別しきい値";
                case AvatarAudioSafetyRule.Default:
                default:
                    return "通常";
            }
        }

        public static string GetModeLabel(AvatarAudioSafetyMode mode)
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

        public static string GetBuildEntryStatusLabel(AvatarAudioSafetyBuildResultEntryStatus status)
        {
            switch (status)
            {
                case AvatarAudioSafetyBuildResultEntryStatus.Changed:
                    return "変更あり";
                case AvatarAudioSafetyBuildResultEntryStatus.Skipped:
                    return "スキップ";
                case AvatarAudioSafetyBuildResultEntryStatus.Error:
                    return "エラー";
                case AvatarAudioSafetyBuildResultEntryStatus.Unchanged:
                default:
                    return "変更なし";
            }
        }

        public static string GetRuleShortLabel(AvatarAudioSafetyRule rule)
        {
            switch (rule)
            {
                case AvatarAudioSafetyRule.Ignore:
                    return "対象外";
                case AvatarAudioSafetyRule.ReportOnly:
                    return "報告のみ";
                case AvatarAudioSafetyRule.CustomThreshold:
                    return "個別しきい値";
                case AvatarAudioSafetyRule.Default:
                default:
                    return "通常";
            }
        }

        public static string GetProfileLabel(AvatarAudioSafetyProfile profile)
        {
            switch (profile)
            {
                case AvatarAudioSafetyProfile.Conservative:
                    return "厳しめ";
                case AvatarAudioSafetyProfile.Custom:
                    return "カスタム";
                case AvatarAudioSafetyProfile.Standard:
                default:
                    return "標準";
            }
        }

        public static string GetPathText(string path)
        {
            return "パス: " + (string.IsNullOrEmpty(path) ? "./" : path);
        }

        public static string GetMainInfoText(AvatarAudioScanResult result)
        {
            return string.Format(
                "AudioClip: {0} | ルール: {1}",
                result != null && result.clip != null ? result.clip.name : "-",
                result != null ? GetRuleShortLabel(result.appliedRule) : GetRuleShortLabel(AvatarAudioSafetyRule.Default));
        }

        public static string GetMainInfoTooltip(AvatarAudioScanResult result)
        {
            return string.Format(
                "パス（Path）: {0}\nAudioClip: {1}\nLoop: {2}\n音量（Volume）: {3:0.##}\nGain: {4:0.##}\n最大距離（Far Distance）: {5:0.##}\nNear: {6:0.##}\nVolumetric Radius: {7:0.##}\nルール（Rule）: {8}",
                result != null && !string.IsNullOrEmpty(result.path) ? result.path : "./",
                result != null && result.clip != null ? result.clip.name : "-",
                result != null && result.loop ? "On" : "Off",
                result != null ? result.volume : 0f,
                result != null ? result.gain : 0f,
                result != null ? result.farDistance : 0f,
                result != null ? result.nearDistance : 0f,
                result != null ? result.volumetricRadius : 0f,
                result != null ? GetRuleLabel(result.appliedRule) : GetRuleLabel(AvatarAudioSafetyRule.Default));
        }

        public static string GetRuleDescription(AvatarAudioSafetyRule rule)
        {
            switch (rule)
            {
                case AvatarAudioSafetyRule.Ignore:
                    return "この AudioSource はチェック対象から外します。";
                case AvatarAudioSafetyRule.ReportOnly:
                    return "問題があっても補正せず、一覧に表示だけします。";
                case AvatarAudioSafetyRule.CustomThreshold:
                    return "この AudioSource だけ別の上限値で判定します。";
                case AvatarAudioSafetyRule.Default:
                default:
                    return "通常の判定ルールを使います。";
            }
        }

        public static string GetReasonLabel()
        {
            return "理由";
        }

        public static string GetPlanLabel()
        {
            return "予定";
        }

        private static string GetScanResultDisplayName(AvatarAudioScanResult result)
        {
            if (result != null && result.audioSource != null && result.audioSource.gameObject != null)
            {
                return result.audioSource.gameObject.name;
            }

            if (result != null && !string.IsNullOrEmpty(result.path) && result.path != "./")
            {
                int separatorIndex = result.path.LastIndexOf('/');
                if (separatorIndex >= 0 && separatorIndex + 1 < result.path.Length)
                {
                    return result.path.Substring(separatorIndex + 1);
                }

                return result.path;
            }

            return "(unnamed AudioSource)";
        }
    }
}
