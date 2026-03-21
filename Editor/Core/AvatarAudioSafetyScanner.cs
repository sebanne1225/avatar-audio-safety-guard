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
            List<AvatarAudioScanResult> results = new List<AvatarAudioScanResult>();
            AvatarAudioScanSummary summary = new AvatarAudioScanSummary();

            if (settings == null)
            {
                return new AvatarAudioSafetyScanReport(results, summary);
            }

            AvatarAudioThresholdPreset defaultThresholds = settings.ResolveDefaultThresholds();
            List<AudioSource> audioSources = AvatarAudioSafetyAudioSourceCollector.Collect(settings);

            for (int i = 0; i < audioSources.Count; i++)
            {
                AudioSource audioSource = audioSources[i];
                AvatarAudioSafetyEvaluationRequest request = AvatarAudioSafetyEvaluator.CreateRequest(settings, audioSource, defaultThresholds);
                AvatarAudioSafetyEvaluation evaluation = AvatarAudioSafetyEvaluator.Evaluate(request);
                results.Add(AvatarAudioSafetyScanResultFactory.Create(evaluation));
            }

            summary.UpdateFromResults(results);
            return new AvatarAudioSafetyScanReport(results, summary);
        }
    }
}
