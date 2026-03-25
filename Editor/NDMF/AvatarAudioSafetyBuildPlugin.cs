using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEngine;

[assembly: ExportsPlugin(typeof(Sebanne.AvatarAudioSafetyGuard.Editor.NDMF.AvatarAudioSafetyBuildPlugin))]

namespace Sebanne.AvatarAudioSafetyGuard.Editor.NDMF
{
    internal sealed class AvatarAudioSafetyBuildPlugin : Plugin<AvatarAudioSafetyBuildPlugin>
    {
        public override string QualifiedName
        {
            get { return "com.sebanne.avatar-audio-safety-guard.ndmf"; }
        }

        public override string DisplayName
        {
            get { return "Avatar Audio Safety Guard"; }
        }

        protected override void Configure()
        {
            Sequence sequence = InPhase(BuildPhase.Transforming);
            sequence.AfterPlugin("nadena.dev.modular-avatar");
            sequence.Run("Avatar Audio Safety Guard Build Correction", ApplyBuildCorrection);
        }

        private static void ApplyBuildCorrection(BuildContext context)
        {
            AvatarAudioSafetySettings settings = context.AvatarRootObject != null
                ? context.AvatarRootObject.GetComponent<AvatarAudioSafetySettings>()
                : null;

            IReadOnlyList<AudioSource> audioSources = settings != null
                ? AvatarAudioSafetyAudioSourceCollector.Collect(settings)
                : null;
            IReadOnlyList<ObjectReference> sourceObjectReferences = CollectSourceObjectReferences(audioSources);
            IReadOnlyList<string> sourceAnchorPaths = CollectSourceAnchorPaths(audioSources);

            AvatarAudioSafetyBuildResultSnapshot snapshot =
                AvatarAudioSafetyBuildProcessor.Process(context.AvatarRootObject, audioSources, sourceAnchorPaths);
            AvatarAudioSafetyNdmfConsoleReporter.ReportActionableResults(
                snapshot,
                sourceObjectReferences,
                settings != null ? settings.Profile : AvatarAudioSafetyProfile.Standard);
        }

        private static IReadOnlyList<ObjectReference> CollectSourceObjectReferences(IReadOnlyList<AudioSource> audioSources)
        {
            if (audioSources == null || audioSources.Count == 0)
            {
                return null;
            }

            List<ObjectReference> sourceObjectReferences = new List<ObjectReference>(audioSources.Count);

            for (int i = 0; i < audioSources.Count; i++)
            {
                AudioSource audioSource = audioSources[i];
                sourceObjectReferences.Add(audioSource != null ? ObjectRegistry.GetReference(audioSource.gameObject) : null);
            }

            return sourceObjectReferences;
        }

        private static IReadOnlyList<string> CollectSourceAnchorPaths(IReadOnlyList<AudioSource> audioSources)
        {
            if (audioSources == null || audioSources.Count == 0)
            {
                return null;
            }

            List<string> sourceAnchorPaths = new List<string>(audioSources.Count);

            for (int i = 0; i < audioSources.Count; i++)
            {
                AudioSource audioSource = audioSources[i];
                if (audioSource == null)
                {
                    sourceAnchorPaths.Add(string.Empty);
                    continue;
                }

                ObjectReference reference = ObjectRegistry.GetReference(audioSource.gameObject);
                sourceAnchorPaths.Add(NormalizeSourceAnchorPath(reference));
            }

            return sourceAnchorPaths;
        }

        private static string NormalizeSourceAnchorPath(ObjectReference reference)
        {
            if (reference == null || reference.Path == "<unknown>")
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(reference.Path))
            {
                return "./";
            }

            return AvatarAudioSafetyPathUtility.Normalize(reference.Path);
        }
    }
}
