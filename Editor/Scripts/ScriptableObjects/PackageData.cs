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

        private const string versionSeperator = ".";
        protected static Vector3Int ParseVersion(string versionText)
        {
            List<string> stringList = new List<string>();
            Vector3Int returnInt = Vector3Int.zero;

            string inputString = versionText;

            while (inputString.Contains(versionSeperator))
            {
                string inputStringWithoutTextBeforeFirstComma = inputString.Substring(inputString.IndexOf(versionSeperator));
                stringList.Add(inputString.Replace(inputStringWithoutTextBeforeFirstComma, ""));
                if (inputStringWithoutTextBeforeFirstComma.Contains(versionSeperator))
                    inputString = inputStringWithoutTextBeforeFirstComma.Substring(inputStringWithoutTextBeforeFirstComma.IndexOf(versionSeperator) + 1);

            }
            stringList.Add(inputString);

            if (stringList.Count > 0 && int.TryParse(stringList[0], out int xResult))
                returnInt.x = xResult;
            if (stringList.Count > 1 && int.TryParse(stringList[1], out int yResult))
                returnInt.y = yResult;
            if (stringList.Count > 2 && int.TryParse(stringList[2], out int zResult))
                returnInt.z = zResult;
            return (returnInt);
        }
    }
}
