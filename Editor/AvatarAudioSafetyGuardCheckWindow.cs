using UnityEditor;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    public sealed class AvatarAudioSafetyGuardCheckWindow : EditorWindow
    {
        private const string WindowTitle = "Avatar Audio Safety Guard";
        private const string PackageName = "com.sebanne.avatar-audio-safety-guard";
        private const string DisplayName = "Avatar Audio Safety Guard";
        private const string StatusLabel = "現在対応: package 読み込み確認 / Dry Run 診断ログ";

        [MenuItem("Tools/Avatar Audio Safety Guard/Check Window")]
        private static void Open()
        {
            var window = GetWindow<AvatarAudioSafetyGuardCheckWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(420f, 220f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Avatar Audio Safety Guard の読み込み確認と、今後の診断 UI の土台を兼ねた最小ウィンドウです。現在の処理は非破壊で、Dry Run 相当の確認ログのみを出します。", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("package名", PackageName);
            EditorGUILayout.LabelField("displayName", DisplayName);
            EditorGUILayout.LabelField(StatusLabel, EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("今後ここに AudioSource 診断と安全側補正の UI を追加していく想定です。", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();
            if (GUILayout.Button("診断ログを出す (Dry Run)"))
            {
                Debug.Log("[Avatar Audio Safety Guard] Dry Run diagnostic window is working.");
            }
        }
    }
}
