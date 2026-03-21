using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;

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
            AvatarAudioSafetyBuildProcessor.Process(context.AvatarRootObject);
        }
    }
}
