using System.Collections.Generic;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal sealed class AvatarAudioSafetyEvaluationRequest
    {
        public AvatarAudioSafetyEvaluationRequest(
            AvatarAudioSafetySettings settings,
            AudioSource audioSource,
            string path,
            AvatarAudioSourceRuleEntry matchedRule,
            AvatarAudioSafetyRule appliedRule,
            AvatarAudioThresholdPreset thresholds,
            AvatarAudioSpatialAudioData spatialAudio)
        {
            Settings = settings;
            AudioSource = audioSource;
            Path = path;
            MatchedRule = matchedRule;
            AppliedRule = appliedRule;
            Thresholds = thresholds;
            SpatialAudio = spatialAudio;
        }

        public AvatarAudioSafetySettings Settings { get; }

        public AudioSource AudioSource { get; }

        public string Path { get; }

        public AvatarAudioSourceRuleEntry MatchedRule { get; }

        public AvatarAudioSafetyRule AppliedRule { get; }

        public AvatarAudioThresholdPreset Thresholds { get; }

        public AvatarAudioSpatialAudioData SpatialAudio { get; }
    }

    internal sealed class AvatarAudioSafetyEvaluation
    {
        public AudioSource AudioSource { get; set; }

        public string Path { get; set; }

        public AudioClip Clip { get; set; }

        public bool Loop { get; set; }

        public float Volume { get; set; }

        public float Gain { get; set; }

        public float FarDistance { get; set; }

        public AvatarAudioSafetyRule AppliedRule { get; set; }

        public AvatarAudioSafetyResultKind Result { get; set; }

        public bool HasDiagnosticReasons { get; set; }

        public string Reason { get; set; }

        public string PlannedChange { get; set; }
    }

    internal static class AvatarAudioSafetyEvaluator
    {
        public static AvatarAudioSafetyEvaluationRequest CreateRequest(
            AvatarAudioSafetySettings settings,
            AudioSource audioSource,
            AvatarAudioThresholdPreset defaultThresholds)
        {
            string path = AvatarAudioSafetyPathUtility.GetRelativePath(settings.transform, audioSource.transform);
            AvatarAudioSourceRuleEntry matchedRule = FindMatchingRule(settings.PerSourceRules, path);
            AvatarAudioSafetyRule appliedRule = matchedRule != null ? matchedRule.rule : AvatarAudioSafetyRule.Default;
            AvatarAudioThresholdPreset thresholds = ResolveThresholds(defaultThresholds, matchedRule);
            AvatarAudioSpatialAudioData spatialAudio = AvatarAudioSafetySpatialAudioUtility.Read(audioSource);

            return new AvatarAudioSafetyEvaluationRequest(
                settings,
                audioSource,
                path,
                matchedRule,
                appliedRule,
                thresholds,
                spatialAudio);
        }

        public static AvatarAudioSafetyEvaluation Evaluate(AvatarAudioSafetyEvaluationRequest request)
        {
            AvatarAudioSafetyEvaluation evaluation = new AvatarAudioSafetyEvaluation
            {
                Path = request.Path,
                AudioSource = request.AudioSource,
                Clip = request.AudioSource.clip,
                Loop = request.AudioSource.loop,
                Volume = request.AudioSource.volume,
                Gain = request.SpatialAudio.Gain,
                FarDistance = request.SpatialAudio.FarDistance,
                AppliedRule = request.AppliedRule,
            };

            if (request.AppliedRule == AvatarAudioSafetyRule.Ignore)
            {
                evaluation.Result = AvatarAudioSafetyResultKind.Ignored;
                evaluation.HasDiagnosticReasons = false;
                evaluation.Reason = "Path rule is set to Ignore.";
                evaluation.PlannedChange = "No change needed";
                return evaluation;
            }

            List<string> reasons = new List<string>();
            List<string> plannedChanges = new List<string>();
            bool requiresManualReview = false;
            bool wouldClamp = false;

            if (request.Settings.WarnOnMissingVrcSpatialAudioSource && !request.SpatialAudio.HasComponent)
            {
                reasons.Add("Missing VRCSpatialAudioSource.");
            }

            if (request.Settings.WarnOnCustomRolloff && request.AudioSource.rolloffMode == AudioRolloffMode.Custom)
            {
                reasons.Add("Custom rolloff requires manual review.");
                requiresManualReview = true;
            }

            if (request.Settings.WarnOnNon3DAudio && request.AudioSource.spatialBlend < 0.99f)
            {
                reasons.Add("Spatial Blend is below 1.0.");
            }

            if (request.Settings.WarnOnLoopWithLongRange && request.AudioSource.loop && evaluation.FarDistance > request.Thresholds.maxFarDistance)
            {
                reasons.Add("Loop source exceeds the far distance threshold.");
            }

            if (evaluation.Gain > request.Thresholds.maxGain)
            {
                wouldClamp = true;
                plannedChanges.Add(string.Format("Gain {0:0.##} -> {1:0.##}", evaluation.Gain, request.Thresholds.maxGain));
            }

            if (evaluation.FarDistance > request.Thresholds.maxFarDistance)
            {
                wouldClamp = true;
                plannedChanges.Add(string.Format("Far {0:0.##} -> {1:0.##}", evaluation.FarDistance, request.Thresholds.maxFarDistance));
            }

            if (evaluation.Volume > request.Thresholds.maxVolume)
            {
                wouldClamp = true;
                plannedChanges.Add(string.Format("Volume {0:0.##} -> {1:0.##}", evaluation.Volume, request.Thresholds.maxVolume));
            }

            if (requiresManualReview)
            {
                evaluation.Result = AvatarAudioSafetyResultKind.ManualReview;
            }
            else if (request.AppliedRule == AvatarAudioSafetyRule.ReportOnly && (wouldClamp || reasons.Count > 0))
            {
                evaluation.Result = AvatarAudioSafetyResultKind.ReportOnly;
            }
            else if (wouldClamp)
            {
                evaluation.Result = AvatarAudioSafetyResultKind.WouldClamp;
            }
            else if (reasons.Count > 0)
            {
                evaluation.Result = AvatarAudioSafetyResultKind.Warning;
            }
            else
            {
                evaluation.Result = AvatarAudioSafetyResultKind.Safe;
            }

            evaluation.HasDiagnosticReasons = reasons.Count > 0;
            evaluation.Reason = reasons.Count > 0 ? string.Join(" | ", reasons.ToArray()) : "Within current thresholds.";
            evaluation.PlannedChange = plannedChanges.Count > 0 ? string.Join("; ", plannedChanges.ToArray()) : "No change needed";
            return evaluation;
        }

        private static AvatarAudioSourceRuleEntry FindMatchingRule(IReadOnlyList<AvatarAudioSourceRuleEntry> rules, string path)
        {
            if (rules == null)
            {
                return null;
            }

            string normalizedPath = AvatarAudioSafetyPathUtility.Normalize(path);

            for (int i = 0; i < rules.Count; i++)
            {
                AvatarAudioSourceRuleEntry rule = rules[i];
                if (rule == null)
                {
                    continue;
                }

                string rulePath = AvatarAudioSafetyPathUtility.Normalize(rule.path);
                if (string.IsNullOrEmpty(rulePath))
                {
                    continue;
                }

                if (rulePath == normalizedPath)
                {
                    return rule;
                }
            }

            return null;
        }

        private static AvatarAudioThresholdPreset ResolveThresholds(
            AvatarAudioThresholdPreset defaultThresholds,
            AvatarAudioSourceRuleEntry matchedRule)
        {
            if (matchedRule != null && matchedRule.rule == AvatarAudioSafetyRule.CustomThreshold && matchedRule.customThresholds != null)
            {
                return matchedRule.customThresholds.Clone();
            }

            if (defaultThresholds != null)
            {
                return defaultThresholds.Clone();
            }

            return AvatarAudioThresholdPresets.CreateStandard();
        }
    }
}
