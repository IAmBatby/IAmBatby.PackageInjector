using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public class ReleaseData : ScriptableObject
    {
        [field: SerializeField] public PackageData PackageData;
        [field: SerializeField] public Vector3Int ReleaseVersion;
        [field: SerializeField] public string Path;

        [field: SerializeField] public TextAsset ReadMe;
        [field: SerializeField] public TextAsset Changelog;
        [field: SerializeField] public TextAsset Manifest;
        [field: SerializeField] public TextAsset License;

        [field: SerializeField] public Texture2D Icon;

        [field: SerializeField] public List<DefaultAsset> AssemblyFiles = new List<DefaultAsset>();

        [field: SerializeField] public List<DefaultAsset> InstalledAssemblyFiles = new List<DefaultAsset>();


        public void Populate(PackageData packageData, Vector3Int version)
        {
            
            AssemblyFiles.Clear();
            Path = packageData.ManagedPath + "/" + packageData.LatestVersionName;

            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[]{Path}))
            {
                UnityEngine.Object releaseAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid));
                if (releaseAsset != null)
                {
                    if (releaseAsset is TextAsset textAsset)
                    {
                        if (textAsset.name == "CHANGELOG")
                            Changelog = textAsset;
                        else if (textAsset.name == "README")
                            ReadMe = textAsset;
                        else if (textAsset.name == "manifest")
                            Manifest = textAsset;
                        else if (textAsset.name == "LICENSE")
                            License = textAsset;
                    }
                    else if (releaseAsset is Texture2D iconAsset)
                        Icon = iconAsset;
                    else if (releaseAsset is DefaultAsset assemblyAsset)
                    {
                        string path = AssetDatabase.GetAssetPath(assemblyAsset);
                        AssetImporter plugin = AssetImporter.GetAtPath(path);
                        if (plugin != null && plugin is PluginImporter)
                            AssemblyFiles.Add(assemblyAsset);
                    }
                }
            }

            PackageData = packageData;
            if (!packageData.InstalledReleases.Contains(this))
                PackageData.InstalledReleases.Add(this);
            ReleaseVersion = version;
            Utilities.SetAssetName(this, "ReleaseData-" + packageData.LatestVersionName);         
        }

    }
}
