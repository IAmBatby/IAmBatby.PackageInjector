using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEditor;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public abstract class PackageData : ScriptableObject
    {
        [field: Header("Package Information")]

        [field: SerializeField] public string Name { get; protected set; } = "Unknown";
        [field: SerializeField] public string Author { get; protected set; } = "Unknown";
        [field: SerializeField] public string Description { get; protected set; } = "Unknown";

        [field: SerializeField] public string LatestVersionName { get; protected set; } = "Unknown";
        [field: SerializeField] public Vector3Int LatestVersion { get; protected set; } = Vector3Int.zero;
        [field: SerializeField] public string LatestReleaseDate { get; protected set; } = "Unknown";

        public Texture2D Icon
        {
            get
            {
                if (InstalledReleases.Count > 0)
                    return (InstalledReleases[0]).Icon;
                return (null);
            }
        }

        public string UUID => "com." + Author.ToLowerInvariant() + "." + Name.ToLowerInvariant();

        public string LocalLocation => ManagedPath + "/" + name + ".asset";
        public string FullLocation => ManagedPath.ToFullPath() + "/" + name + ".asset";

        public string ManagedPath => PackageInjectorManager.PackagePath + "/" + UUID;
        public string InstallPath => PackageInjectorManager.LocalPluginsFolder + "/" + UUID;

        [field: Header("Package Install Information")]

        [field: SerializeField] public List<ReleaseData> InstalledReleases { get; protected set; } = new List<ReleaseData>();

        protected string downloadHandlerText { get; private set; }

        internal void SetManifestData(string newDownloadHandlerText)
        {
            downloadHandlerText = newDownloadHandlerText;
            PopulateManifestData();
            name = UUID;

        }

        private void OnEnable()
        {
            if (!PackageInjectorManager.instance.AllPackages.Contains(this))
                PackageInjectorManager.instance.AllPackages.Add(this);
        }

        private void OnDestroy()
        {
            if (PackageInjectorManager.instance.AllPackages.Contains(this))
                PackageInjectorManager.instance.AllPackages.Remove(this);
        }

        protected abstract void PopulateManifestData();

        protected abstract void PopulateInstallData();

        public bool TryGetLatestReleaseURL(out string latestVersionURL)
        {
            latestVersionURL = GetLatestReleaseURL;
            return (!string.IsNullOrEmpty(latestVersionURL));
        }

        public bool TryGetIconURL(out string iconURL)
        {
            iconURL = GetIconURL;
            return (!string.IsNullOrEmpty(iconURL));
        }

        public bool TryGetLatestPackageURL(out string packageURL)
        {
            packageURL = GetLatestPackageURL;
            return (!string.IsNullOrEmpty(packageURL));
        }

        protected virtual string GetLatestPackageURL => string.Empty;

        protected virtual string GetLatestReleaseURL => string.Empty;

        protected virtual string GetIconURL => string.Empty;

        protected Vector3Int ParseVersion(string versionText)
        {
            return (Vector3Int.one);
            string xFloat = versionText.Replace(versionText.Substring(versionText.IndexOf(".")), string.Empty);
            string yFloat = versionText.Substring(versionText.IndexOf(".") + 1).Replace(versionText.Substring(versionText.IndexOf(".")), string.Empty);
            string zFloat = versionText.Substring(versionText.LastIndexOf(".") + 1);
            return (new Vector3Int((int)float.Parse(xFloat), (int)float.Parse(yFloat), (int)float.Parse(zFloat)));
        }
    }
}
