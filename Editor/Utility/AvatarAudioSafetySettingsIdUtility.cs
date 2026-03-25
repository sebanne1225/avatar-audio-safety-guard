using UnityEditor;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetySettingsIdUtility
    {
        public static bool TryEnsureSourceSettingsId(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return false;
            }

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(settings);
            if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) != settings)
            {
                return !string.IsNullOrEmpty(settings.SourceSettingsGlobalId);
            }

            string resolvedId = globalObjectId.ToString();
            if (string.IsNullOrEmpty(resolvedId))
            {
                return !string.IsNullOrEmpty(settings.SourceSettingsGlobalId);
            }

            if (resolvedId == settings.SourceSettingsGlobalId)
            {
                return true;
            }

            settings.SetSourceSettingsGlobalId(resolvedId);
            EditorUtility.SetDirty(settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
            return true;
        }
    }
}
