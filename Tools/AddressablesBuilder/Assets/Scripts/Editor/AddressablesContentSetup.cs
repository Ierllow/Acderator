using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Acderator.ContentBuilder
{
    public static class AddressablesContentSetup
    {
        private const string ContentRoot = "Assets/Content";

        private static readonly HashSet<string> AddressableExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".bytes", ".spriteatlas", ".prefab"
        };

        [MenuItem("Tools/Sync Content Addresses")]
        public static void Sync()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = settings.groups.Single(candidate => candidate?.GetSchema<BundledAssetGroupSchema>()?.LoadPath.GetName(settings) == "Remote.LoadPath");

            var contentEntries = FindContentEntries().ToArray();
            var duplicateAddress = contentEntries.GroupBy(entry => entry.Address, StringComparer.Ordinal).FirstOrDefault(entries => entries.Skip(1).Any())?.Key;

            if (duplicateAddress != null)
                throw new InvalidOperationException("Duplicate content address: " + duplicateAddress);

            foreach (var (guid, address) in contentEntries)
            {
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.SetAddress(address);
                entry.SetLabel(address, true, true);
            }

            AssetDatabase.SaveAssets();
            Debug.Log(string.Format("Content addresses synchronized: {0} assets in {1}.", contentEntries.Length, group.Name));
        }

        private static IEnumerable<(string Guid, string Address)> FindContentEntries()
            => AssetDatabase.FindAssets(string.Empty, new[] { ContentRoot })
                            .Select(guid => (Guid: guid, Path: AssetDatabase.GUIDToAssetPath(guid)))
                            .Where(asset => AddressableExtensions.Contains(Path.GetExtension(asset.Path)) && !Path.GetFileName(asset.Path).StartsWith("_", StringComparison.Ordinal))
                            .Select(asset => (asset.Guid, Address: AddressForPath(asset.Path)));

        private static string AddressForPath(string path)
        {
            var relative = path[(ContentRoot.Length + 1)..];
            var withoutExtension = relative[..^Path.GetExtension(relative).Length];
            var parts = withoutExtension.Split('/');
            if (Path.GetExtension(relative).Equals(".spriteatlas", StringComparison.OrdinalIgnoreCase) &&
                parts.Length > 1 &&
                string.Equals(parts[^1], parts[^2], StringComparison.OrdinalIgnoreCase))
                withoutExtension = string.Join("/", parts.Take(parts.Length - 1));

            var address = withoutExtension.ToLowerInvariant();
            return address == "sounds/song/song_se" ? "sounds/song/songse" : address;
        }
    }
}
