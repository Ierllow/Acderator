using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Acderator.ContentBuilder
{
    public sealed class AddressablesExporter : EditorWindow
    {
        private bool buildAndroid = true;
        private bool buildIos;

        [MenuItem("Tools/Build Addressables")]
        private static void OpenWindow() => GetWindow<AddressablesExporter>(false, "Build Addressables");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Song assets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Syncs content addresses and labels. Output: ServerData/<BuildTarget>.", MessageType.Info);
            buildAndroid = EditorGUILayout.Toggle("Android", buildAndroid);
            buildIos = EditorGUILayout.Toggle("iOS", buildIos);

            if (GUILayout.Button("Sync Content Addresses")) AddressablesContentSetup.Sync();

            using (new EditorGUI.DisabledScope(!buildAndroid && !buildIos))
            {
                if (GUILayout.Button("Build Addressables"))
                    EditorApplication.delayCall += BuildSelected;
            }
        }

        private void BuildSelected()
        {
            try
            {
                AddressablesContentSetup.Sync();
                if (buildAndroid) Build(BuildTarget.Android);
                if (buildIos) Build(BuildTarget.iOS);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Addressables build failed", exception.Message, "OK");
            }
        }

        private void Build(BuildTarget target)
        {
            if (target != EditorUserBuildSettings.activeBuildTarget &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("Could not switch build target to " + target);

            AddressableAssetSettings.BuildPlayerContent(out var result);
            if (result == null)
                throw new InvalidOperationException(target + " Addressables build failed: No result");
            if (!string.IsNullOrEmpty(result.Error))
                throw new InvalidOperationException(target + " Addressables build failed: " + result.Error);

            var output = Path.GetFullPath(Path.Combine("ServerData", target.ToString()));
            if (!Directory.Exists(output) || Directory.GetFiles(output, "catalog*.hash").Length == 0)
                throw new InvalidOperationException("Remote catalog was not produced at " + output);
            if (result.AssetBundleBuildResults.Count == 0)
                throw new InvalidOperationException("The build did not report any content bundles.");

            var currentBundles = result.AssetBundleBuildResults.Select(bundle => Path.GetFileName(bundle.FilePath)).ToHashSet();
            foreach (var bundlePath in Directory.GetFiles(output, "*.bundle"))
            {
                if (!currentBundles.Contains(Path.GetFileName(bundlePath)))
                    File.Delete(bundlePath);
            }
            Debug.Log(string.Format("Addressables ready for hosting: {0}", output));
        }
    }
}
