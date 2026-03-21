using System.Collections.Generic;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal sealed class AvatarAudioSafetyBuildApplyPlan
    {
        public AudioSource AudioSource { get; set; }

        public string Path { get; set; }

        public AvatarAudioSafetyEvaluation Evaluation { get; set; }

        public AvatarAudioSafetyRule AppliedRule { get; set; }

        public bool HasSpatialAudioComponent { get; set; }

        public bool ShouldClampFar { get; set; }

        public float OriginalFarDistance { get; set; }

        public float TargetFarDistance { get; set; }

        public bool ShouldClampNear { get; set; }

        public float OriginalNearDistance { get; set; }

        public float TargetNearDistance { get; set; }

        public bool ShouldClampVolumetricRadius { get; set; }

        public float OriginalVolumetricRadius { get; set; }

        public float TargetVolumetricRadius { get; set; }

        public bool ShouldClampGain { get; set; }

        public float OriginalGain { get; set; }

        public float TargetGain { get; set; }

        public bool ShouldClampVolume { get; set; }

        public float OriginalVolume { get; set; }

        public float TargetVolume { get; set; }

        public bool ShouldApply
        {
            get
            {
                return ShouldClampFar
                    || ShouldClampNear
                    || ShouldClampVolumetricRadius
                    || ShouldClampGain
                    || ShouldClampVolume;
            }
        }

        public int PlannedChangeCount
        {
            get
            {
                int count = 0;

                if (ShouldClampFar)
                {
                    count++;
                }

                if (ShouldClampGain)
                {
                    count++;
                }

                if (ShouldClampNear)
                {
                    count++;
                }

                if (ShouldClampVolumetricRadius)
                {
                    count++;
                }

                if (ShouldClampVolume)
                {
                    count++;
                }

                return count;
            }
        }

        public string DescribePlannedChanges()
        {
            List<string> changes = new List<string>();

            if (ShouldClampFar)
            {
                changes.Add(string.Format("Far {0:0.##} -> {1:0.##}", OriginalFarDistance, TargetFarDistance));
            }

            if (ShouldClampNear)
            {
                changes.Add(string.Format("Near {0:0.##} -> {1:0.##}", OriginalNearDistance, TargetNearDistance));
            }

            if (ShouldClampVolumetricRadius)
            {
                changes.Add(string.Format("Volumetric Radius {0:0.##} -> {1:0.##}", OriginalVolumetricRadius, TargetVolumetricRadius));
            }

            if (ShouldClampGain)
            {
                changes.Add(string.Format("Gain {0:0.##} -> {1:0.##}", OriginalGain, TargetGain));
            }

            if (ShouldClampVolume)
            {
                changes.Add(string.Format("Volume {0:0.##} -> {1:0.##}", OriginalVolume, TargetVolume));
            }

            return changes.Count > 0 ? string.Join("; ", changes.ToArray()) : "No change needed";
        }
    }
}
