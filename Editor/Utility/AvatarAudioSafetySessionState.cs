using UnityEditor;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetySessionState
    {
        private const string LastSettingsKey = "Sebanne.AvatarAudioSafetyGuard.LastSettings";

        public static void RememberSettings(AvatarAudioSafetySettings settings)
        {
            if (settings == null)
            {
                return;
            }

            // Keep the last known edit-mode settings reference intact while playing.
            if (EditorApplication.isPlaying)
            {
                return;
            }

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(settings);
            SessionState.SetString(LastSettingsKey, globalObjectId.ToString());
        }

        public static AvatarAudioSafetySettings RestoreLastSettings()
        {
            string raw = SessionState.GetString(LastSettingsKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }

            GlobalObjectId globalObjectId;
            if (!GlobalObjectId.TryParse(raw, out globalObjectId))
            {
                return null;
            }

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) as AvatarAudioSafetySettings;
        }
    }
}
