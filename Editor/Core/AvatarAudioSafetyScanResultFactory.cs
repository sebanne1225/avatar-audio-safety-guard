using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyScanResultFactory
    {
        public static AvatarAudioScanResult Create(AvatarAudioSafetyEvaluation evaluation)
        {
            if (evaluation == null)
            {
                return new AvatarAudioScanResult();
            }

            return new AvatarAudioScanResult
            {
                path = evaluation.Path,
                audioSource = evaluation.AudioSource,
                clip = evaluation.Clip,
                loop = evaluation.Loop,
                volume = evaluation.Volume,
                gain = evaluation.Gain,
                farDistance = evaluation.FarDistance,
                nearDistance = evaluation.NearDistance,
                volumetricRadius = evaluation.VolumetricRadius,
                appliedRule = evaluation.AppliedRule,
                result = evaluation.Result,
                reason = evaluation.Reason,
                plannedChange = evaluation.PlannedChange,
            };
        }
    }
}
