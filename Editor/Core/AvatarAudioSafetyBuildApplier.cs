using System.Collections.Generic;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal sealed class AvatarAudioSafetyBuildApplyOutcome
    {
        private readonly List<string> appliedChanges = new List<string>();
        private readonly List<string> failedChanges = new List<string>();

        public bool AppliedAnyChange
        {
            get { return appliedChanges.Count > 0; }
        }

        public int AppliedChangeCount
        {
            get { return appliedChanges.Count; }
        }

        public bool HasFailures
        {
            get { return failedChanges.Count > 0; }
        }

        public string AppliedChangeSummary
        {
            get { return appliedChanges.Count > 0 ? string.Join("; ", appliedChanges.ToArray()) : "No change needed"; }
        }

        public string FailureSummary
        {
            get { return failedChanges.Count > 0 ? string.Join("; ", failedChanges.ToArray()) : string.Empty; }
        }

        public void RecordApplied(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                appliedChanges.Add(text);
            }
        }

        public void RecordFailure(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                failedChanges.Add(text);
            }
        }
    }

    internal static class AvatarAudioSafetyBuildApplier
    {
        private const float Epsilon = 0.0001f;

        public static AvatarAudioSafetyBuildApplyOutcome Apply(AvatarAudioSafetyBuildApplyPlan plan)
        {
            AvatarAudioSafetyBuildApplyOutcome outcome = new AvatarAudioSafetyBuildApplyOutcome();

            if (plan == null || !plan.ShouldApply || plan.AudioSource == null)
            {
                return outcome;
            }

            AudioSource audioSource = plan.AudioSource;

            if (plan.ShouldClampFar)
            {
                ApplyFar(audioSource, plan, outcome);
            }

            if (plan.ShouldClampNear)
            {
                ApplyNear(audioSource, plan, outcome);
            }

            if (plan.ShouldClampVolumetricRadius)
            {
                ApplyVolumetricRadius(audioSource, plan, outcome);
            }

            if (plan.ShouldClampGain)
            {
                ApplyGain(audioSource, plan, outcome);
            }

            if (plan.ShouldClampVolume)
            {
                ApplyVolume(audioSource, plan, outcome);
            }

            return outcome;
        }

        private static void ApplyFar(AudioSource audioSource, AvatarAudioSafetyBuildApplyPlan plan, AvatarAudioSafetyBuildApplyOutcome outcome)
        {
            if (plan.HasSpatialAudioComponent)
            {
                if (!AvatarAudioSafetySpatialAudioUtility.TryWriteFarDistance(audioSource, plan.TargetFarDistance))
                {
                    outcome.RecordFailure("Far could not be written to VRC Spatial Audio Source");
                    return;
                }
            }

            if (Mathf.Abs(audioSource.maxDistance - plan.TargetFarDistance) > Epsilon)
            {
                audioSource.maxDistance = plan.TargetFarDistance;
            }

            outcome.RecordApplied(string.Format("Far {0:0.##} -> {1:0.##}", plan.OriginalFarDistance, plan.TargetFarDistance));
        }

        private static void ApplyNear(AudioSource audioSource, AvatarAudioSafetyBuildApplyPlan plan, AvatarAudioSafetyBuildApplyOutcome outcome)
        {
            if (!plan.HasSpatialAudioComponent)
            {
                outcome.RecordFailure("Near could not be written because VRC Spatial Audio Source is missing");
                return;
            }

            if (!AvatarAudioSafetySpatialAudioUtility.TryWriteNearDistance(audioSource, plan.TargetNearDistance))
            {
                outcome.RecordFailure("Near could not be written to VRC Spatial Audio Source");
                return;
            }

            outcome.RecordApplied(string.Format("Near {0:0.##} -> {1:0.##}", plan.OriginalNearDistance, plan.TargetNearDistance));
        }

        private static void ApplyVolumetricRadius(AudioSource audioSource, AvatarAudioSafetyBuildApplyPlan plan, AvatarAudioSafetyBuildApplyOutcome outcome)
        {
            if (!plan.HasSpatialAudioComponent)
            {
                outcome.RecordFailure("Volumetric Radius could not be written because VRC Spatial Audio Source is missing");
                return;
            }

            if (!AvatarAudioSafetySpatialAudioUtility.TryWriteVolumetricRadius(audioSource, plan.TargetVolumetricRadius))
            {
                outcome.RecordFailure("Volumetric Radius could not be written to VRC Spatial Audio Source");
                return;
            }

            outcome.RecordApplied(string.Format("Volumetric Radius {0:0.##} -> {1:0.##}", plan.OriginalVolumetricRadius, plan.TargetVolumetricRadius));
        }

        private static void ApplyGain(AudioSource audioSource, AvatarAudioSafetyBuildApplyPlan plan, AvatarAudioSafetyBuildApplyOutcome outcome)
        {
            if (!AvatarAudioSafetySpatialAudioUtility.TryWriteGain(audioSource, plan.TargetGain))
            {
                outcome.RecordFailure("Gain could not be written to VRC Spatial Audio Source");
                return;
            }

            outcome.RecordApplied(string.Format("Gain {0:0.##} -> {1:0.##}", plan.OriginalGain, plan.TargetGain));
        }

        private static void ApplyVolume(AudioSource audioSource, AvatarAudioSafetyBuildApplyPlan plan, AvatarAudioSafetyBuildApplyOutcome outcome)
        {
            if (Mathf.Abs(audioSource.volume - plan.TargetVolume) > Epsilon)
            {
                audioSource.volume = plan.TargetVolume;
            }

            outcome.RecordApplied(string.Format("Volume {0:0.##} -> {1:0.##}", plan.OriginalVolume, plan.TargetVolume));
        }
    }
}
