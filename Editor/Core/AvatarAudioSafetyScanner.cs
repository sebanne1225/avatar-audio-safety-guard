using System.Collections.Generic;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal sealed class AvatarAudioSafetyScanReport
    {
        public AvatarAudioSafetyScanReport(List<AvatarAudioScanResult> results, AvatarAudioScanSummary summary)
        {
            Results = results;
            Summary = summary;
        }

        public IReadOnlyList<AvatarAudioScanResult> Results { get; }

        public AvatarAudioScanSummary Summary { get; }
    }

    internal static class AvatarAudioSafetyScanner
    {
        public static AvatarAudioSafetyScanReport Scan(AvatarAudioSafetySettings settings)
        {
            List<AudioSource> audioSources = AvatarAudioSafetyAudioSourceCollector.Collect(settings);
            AvatarAudioThresholdPreset defaultThresholds = settings != null
                ? settings.ResolveDefaultThresholds()
                : null;
            List<AvatarAudioScanResult> results = EvaluateAudioSources(settings, audioSources, defaultThresholds);
            AvatarAudioScanSummary summary = BuildSummary(results);
            return new AvatarAudioSafetyScanReport(results, summary);
        }

        public static AvatarAudioSafetyScanReport Reclassify(AvatarAudioSafetySettings settings)
        {
            List<AvatarAudioScanResult> results = new List<AvatarAudioScanResult>();
            AvatarAudioScanSummary summary = BuildSummaryPreservingScanTime(results, settings != null ? settings.LastScanSummary : null);

            if (settings == null)
            {
                return new AvatarAudioSafetyScanReport(results, summary);
            }

            AvatarAudioThresholdPreset defaultThresholds = settings.ResolveDefaultThresholds();
            IReadOnlyList<AvatarAudioScanResult> detectedAudioSources = settings.DetectedAudioSources;

            if (detectedAudioSources != null)
            {
                for (int i = 0; i < detectedAudioSources.Count; i++)
                {
                    AvatarAudioScanResult existingResult = detectedAudioSources[i];
                    AvatarAudioScanResult refreshedResult = ReclassifyExistingResult(settings, existingResult, defaultThresholds);
                    if (refreshedResult != null)
                    {
                        results.Add(refreshedResult);
                    }
                }
            }

            summary = BuildSummaryPreservingScanTime(results, settings.LastScanSummary);
            return new AvatarAudioSafetyScanReport(results, summary);
        }

        private static List<AvatarAudioScanResult> EvaluateAudioSources(
            AvatarAudioSafetySettings settings,
            IReadOnlyList<AudioSource> audioSources,
            AvatarAudioThresholdPreset defaultThresholds)
        {
            List<AvatarAudioScanResult> results = new List<AvatarAudioScanResult>();

            if (settings == null || audioSources == null)
            {
                return results;
            }

            for (int i = 0; i < audioSources.Count; i++)
            {
                AudioSource audioSource = audioSources[i];
                AvatarAudioSafetyEvaluationRequest request = AvatarAudioSafetyEvaluator.CreateRequest(settings, audioSource, defaultThresholds);
                AvatarAudioSafetyEvaluation evaluation = AvatarAudioSafetyEvaluator.Evaluate(request);
                results.Add(AvatarAudioSafetyScanResultFactory.Create(evaluation));
            }

            return results;
        }

        private static AvatarAudioScanResult ReclassifyExistingResult(
            AvatarAudioSafetySettings settings,
            AvatarAudioScanResult existingResult,
            AvatarAudioThresholdPreset defaultThresholds)
        {
            if (existingResult == null)
            {
                return null;
            }

            if (settings == null || existingResult.audioSource == null)
            {
                return existingResult.Clone();
            }

            AvatarAudioSafetyEvaluationRequest request = AvatarAudioSafetyEvaluator.CreateRequest(
                settings,
                existingResult.audioSource,
                defaultThresholds,
                existingResult.path);
            AvatarAudioSafetyEvaluation evaluation = AvatarAudioSafetyEvaluator.Evaluate(request);
            AvatarAudioScanResult refreshedResult = AvatarAudioSafetyScanResultFactory.Create(evaluation);
            refreshedResult.path = string.IsNullOrEmpty(existingResult.path) ? refreshedResult.path : existingResult.path;
            return refreshedResult;
        }

        private static AvatarAudioScanSummary BuildSummary(IReadOnlyList<AvatarAudioScanResult> results)
        {
            AvatarAudioScanSummary summary = new AvatarAudioScanSummary();
            summary.UpdateFromResults(results);
            return summary;
        }

        private static AvatarAudioScanSummary BuildSummaryPreservingScanTime(
            IReadOnlyList<AvatarAudioScanResult> results,
            AvatarAudioScanSummary previousSummary)
        {
            AvatarAudioScanSummary summary = previousSummary != null ? previousSummary.Clone() : new AvatarAudioScanSummary();
            string preservedLastScanLocalTime = summary.lastScanLocalTime;
            summary.UpdateFromResults(results);
            summary.lastScanLocalTime = preservedLastScanLocalTime;
            return summary;
        }
    }
}
