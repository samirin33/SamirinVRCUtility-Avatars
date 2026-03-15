#if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace Samirin.VRCUtility.AvatarEditor.Editor
{
    [InitializeOnLoad]
    public static class InstollerImport
    {
        private const string ZipFileName = "SamirinVRCUtility Avatar Installer.zip";
        private static string EditorPrefsKey => "SamirinVRCUtility.InstallerImported." + Application.dataPath;

        static InstollerImport()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            ExtractInstallerIfNeeded();
        }

        private static void ExtractInstallerIfNeeded()
        {
            if (EditorPrefs.GetBool(EditorPrefsKey, false)) return;
            ExtractInstaller();
        }

        [MenuItem("Tools/SamirinVRCUtility Avatar Installer Reimport")]
        private static void ReimportInstaller()
        {
            ExtractInstaller(force: true);
        }

        private static void ExtractInstaller(bool force = false)
        {
            string projectPath = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectPath)) return;

            string zipPath = Path.Combine(projectPath, "Packages",
                "com.github.samirin33.samirin-vrc-utility.avatar-editor", ZipFileName);
            string assetsPath = Application.dataPath;

            if (!File.Exists(zipPath))
            {
                if (force)
                    Debug.LogWarning($"[SamirinVRCUtility] {ZipFileName} が見つかりません: {zipPath}");
                return;
            }

            try
            {
                ExtractZipToDirectory(zipPath, assetsPath);
                EditorPrefs.SetBool(EditorPrefsKey, true);
                AssetDatabase.Refresh();
                Debug.Log($"[SamirinVRCUtility] {ZipFileName} を Assets に解凍して配置しました。");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SamirinVRCUtility] ZIP の解凍に失敗しました: {ex.Message}");
            }
        }

        private static void ExtractZipToDirectory(string zipPath, string destinationPath)
        {
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    string destPath = Path.Combine(destinationPath, entry.FullName);
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        if (!Directory.Exists(destPath))
                            Directory.CreateDirectory(destPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                        entry.ExtractToFile(destPath, overwrite: true);
                    }
                }
            }
        }
    }
}
#endif
