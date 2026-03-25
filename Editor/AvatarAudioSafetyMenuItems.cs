using nadena.dev.ndmf.runtime;
using UnityEditor;
using UnityEngine;
using Sebanne.AvatarAudioSafetyGuard;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyMenuItems
    {
        [MenuItem("Tools/Sebanne/Avatar Audio Safety Guard/Report Window", false, 100)]
        private static void OpenReportWindow()
        {
            AvatarAudioSafetyReportWindow.Open();
        }

        [MenuItem("GameObject/Avatar Audio Safety Guard/Add Settings", false, 49)]
        private static void AddSettings(MenuCommand command)
        {
            GameObject contextObject = command.context as GameObject ?? Selection.activeGameObject;
            GameObject avatarRoot = ResolveAvatarRoot(contextObject);
            if (avatarRoot == null)
            {
                Debug.LogWarning("[Avatar Audio Safety Guard] Avatar root を解決できませんでした。アバターの root か、その子 GameObject を選択してから実行してください。");
                return;
            }

            AvatarAudioSafetySettings existing = avatarRoot.GetComponent<AvatarAudioSafetySettings>();
            if (existing != null)
            {
                AvatarAudioSafetySessionState.RememberSettings(existing);
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            Undo.AddComponent<AvatarAudioSafetySettings>(avatarRoot);

            AvatarAudioSafetySettings created = avatarRoot.GetComponent<AvatarAudioSafetySettings>();
            if (created != null)
            {
                AvatarAudioSafetySessionState.RememberSettings(created);
                Selection.activeObject = created;
                EditorGUIUtility.PingObject(created);
            }
        }

        [MenuItem("GameObject/Avatar Audio Safety Guard/Add Settings", true, 49)]
        private static bool ValidateAddSettings()
        {
            return Selection.activeGameObject != null;
        }

        private static GameObject ResolveAvatarRoot(GameObject contextObject)
        {
            if (contextObject == null)
            {
                return null;
            }

            if (RuntimeUtil.IsAvatarRoot(contextObject.transform))
            {
                return contextObject;
            }

            Transform avatarRoot = RuntimeUtil.FindAvatarInParents(contextObject.transform);
            return avatarRoot != null && RuntimeUtil.IsAvatarRoot(avatarRoot)
                ? avatarRoot.gameObject
                : null;
        }
    }
}
