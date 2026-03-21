using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyBuildPlanner
    {
        private const float Epsilon = 0.0001f;

        public static AvatarAudioSafetyBuildApplyPlan CreatePlan(
            AvatarAudioSafetyEvaluationRequest request,
            AvatarAudioSafetyEvaluation evaluation)
        {
            AvatarAudioSafetyBuildApplyPlan plan = new AvatarAudioSafetyBuildApplyPlan
            {
                AudioSource = request != null ? request.AudioSource : null,
                Path = evaluation != null ? evaluation.Path : string.Empty,
                Evaluation = evaluation,
                AppliedRule = request != null ? request.AppliedRule : AvatarAudioSafetyRule.Default,
                HasSpatialAudioComponent = request != null && request.SpatialAudio.HasComponent,
            };

            if (request == null || evaluation == null || request.AudioSource == null || request.Thresholds == null)
            {
                return plan;
            }

            if (request.AppliedRule == AvatarAudioSafetyRule.Ignore || request.AppliedRule == AvatarAudioSafetyRule.ReportOnly)
            {
                return plan;
            }

            if (evaluation.Result != AvatarAudioSafetyResultKind.WouldClamp)
            {
                return plan;
            }

            float targetFarDistance = Mathf.Min(evaluation.FarDistance, request.Thresholds.maxFarDistance);
            if (evaluation.FarDistance - targetFarDistance > Epsilon)
            {
                plan.ShouldClampFar = true;
                plan.OriginalFarDistance = evaluation.FarDistance;
                plan.TargetFarDistance = targetFarDistance;
            }

            float targetGain = Mathf.Min(evaluation.Gain, request.Thresholds.maxGain);
            if (evaluation.Gain - targetGain > Epsilon)
            {
                plan.ShouldClampGain = true;
                plan.OriginalGain = evaluation.Gain;
                plan.TargetGain = targetGain;
            }

            float targetVolume = Mathf.Min(evaluation.Volume, request.Thresholds.maxVolume);
            if (evaluation.Volume - targetVolume > Epsilon)
            {
                plan.ShouldClampVolume = true;
                plan.OriginalVolume = evaluation.Volume;
                plan.TargetVolume = targetVolume;
            }

            return plan;
        }
    }
}
