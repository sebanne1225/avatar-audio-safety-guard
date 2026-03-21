using System.Collections.Generic;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyBuildProcessor
    {
        public static void Process(GameObject avatarRootObject)
        {
            if (avatarRootObject == null)
            {
                return;
            }

            AvatarAudioSafetySettings settings = avatarRootObject.GetComponent<AvatarAudioSafetySettings>();
            if (settings == null)
            {
                return;
            }

            if (!settings.ToolEnabled)
            {
                AvatarAudioSafetyBuildLogger.LogSkipped(settings, "有効化がオフのため、build clone は変更しません。");
                return;
            }

            if (settings.Mode != AvatarAudioSafetyMode.ApplyOnBuild)
            {
                AvatarAudioSafetyBuildLogger.LogSkipped(settings, "診断のみ のため、build clone は変更しません。");
                return;
            }

            AvatarAudioThresholdPreset defaultThresholds = settings.ResolveDefaultThresholds();
            List<AudioSource> audioSources = AvatarAudioSafetyAudioSourceCollector.Collect(settings);
            List<AvatarAudioScanResult> results = new List<AvatarAudioScanResult>(audioSources.Count);
            AvatarAudioScanSummary summary = new AvatarAudioScanSummary();
            int correctedSourceCount = 0;

            for (int i = 0; i < audioSources.Count; i++)
            {
                AudioSource audioSource = audioSources[i];
                AvatarAudioSafetyEvaluationRequest request = AvatarAudioSafetyEvaluator.CreateRequest(settings, audioSource, defaultThresholds);
                AvatarAudioSafetyEvaluation evaluation = AvatarAudioSafetyEvaluator.Evaluate(request);
                AvatarAudioSafetyBuildApplyPlan plan = AvatarAudioSafetyBuildPlanner.CreatePlan(request, evaluation);
                AvatarAudioSafetyBuildApplyOutcome outcome = AvatarAudioSafetyBuildApplier.Apply(plan);

                if (outcome.AppliedAnyChange)
                {
                    correctedSourceCount++;
                }

                results.Add(AvatarAudioSafetyScanResultFactory.Create(evaluation));
                AvatarAudioSafetyBuildLogger.LogItem(settings, plan, outcome);
            }

            summary.UpdateFromResults(results);
            AvatarAudioSafetyBuildLogger.LogSummary(settings, summary, correctedSourceCount);
        }
    }
}
