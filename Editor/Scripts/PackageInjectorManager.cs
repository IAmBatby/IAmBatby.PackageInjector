using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditorInternal;
using System.Reflection.Emit;
using System.Linq;
using System.IO;
using UnityEditor.VersionControl;

namespace IAmBatby.PackageInjector
{
    [FilePath("com.iambatby.packageinjector/packageinjectormanager.data", FilePathAttribute.Location.PreferencesFolder)]
    public class PackageInjectorManager : ScriptableSingleton<PackageInjectorManager>
    {
        [SerializeField] private string _packageFolder;
        internal static string LocalPackagePath
        {
            get
            {
                if (string.IsNullOrEmpty(instance._packageFolder))
                {
                    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                    foreach (AssemblyDefinitionAsset assemblyDef in Utilities.FindAssets<AssemblyDefinitionAsset>())
                        if (assemblyDef.name == assemblyName)
                        {
                            instance._packageFolder = AssetDatabase.GetAssetPath(assemblyDef);
                            instance._packageFolder = instance._packageFolder.Remove(instance._packageFolder.LastIndexOf("/"));
                            return (instance._packageFolder);
                        }
                    Debug.LogError("Failed To Find Package Folder!");
                    return (string.Empty);
                }
                else
                    return (instance._packageFolder);
            }
        }

        public static string PackagePath => LocalPackagePath + "/packages/plugins";

        [SerializeField] private string _localPluginsFolder = "Assets/LethalCompany/Tools/Plugins";
        public static string LocalPluginsFolder { get => instance._localPluginsFolder; set => instance._localPluginsFolder = value; }

        [SerializeField] private string _downloadedReleaseFileName = "LatestRelease";
        public static string DownloadedReleaseFileName { get => instance._downloadedReleaseFileName; set => instance._downloadedReleaseFileName = value; }

        [field: SerializeField] public List<PackageData> AllPackages { get; private set; } = new List<PackageData>();

        [field: SerializeField] private List<PackageData> RecentlyInstalledPackages = new List<PackageData>();

        private void OnEnable()
        {
            Validate();
            PostProcessPackages();
        }
        private void OnDisable() => Save(true);

        public static void Validate()
        {
            if (instance.AllPackages.Count == 0) return;
            int count = 0;
            foreach (PackageData package in new List<PackageData>(instance.AllPackages))
            {
                if (package == null)
                    instance.AllPackages.RemoveAt(count);
                count++;
            }
        }

        public static void TryDownloadNewPackageData(string userURL)
        {
            //Awful, Thunderstore Hardcoded And No Validation
            string skippedUrl = userURL.Substring(userURL.IndexOf("/p/") + 3);
            string projectNamespace = skippedUrl.Replace(skippedUrl.Substring(skippedUrl.IndexOf("/")), string.Empty);
            string projectName = skippedUrl.Substring(skippedUrl.IndexOf("/") + 1);
            projectName = projectName.Replace("/", string.Empty);
            string url = "https://thunderstore.io/api/experimental/package/" + projectNamespace + "/" + projectName + "/";

            DownloadHandlerBehaviour.ProcessDownloadRequest(new TextDownloadRequest(url, CreateNewPackageData<ThunderstorePackageData>));
        }

        public static void CreateNewPackageData<T>(string downloadHandlerText) where T : PackageData
        {
            if (AssetDatabase.IsValidFolder(PackagePath))
            {
                AssetDatabase.DisallowAutoRefresh();

                T newPackageData = CreateInstance<T>();
                newPackageData.SetManifestData(downloadHandlerText);

                if (AssetDatabase.IsValidFolder(newPackageData.ManagedPath) == false)
                    AssetDatabase.CreateFolder(PackagePath, newPackageData.UUID);

                AssetDatabase.CreateAsset(newPackageData, newPackageData.LocalLocation);
                newPackageData = AssetDatabase.LoadAssetAtPath(newPackageData.LocalLocation, typeof(T)) as T;
                newPackageData.SetManifestData(downloadHandlerText);
                TryDownloadLatestPackageVersion(newPackageData);
            }
        }

        public static void TryDownloadLatestPackageVersion(PackageData packageData)
        {
            if (packageData.TryGetLatestReleaseURL(out string latestReleaseURL))
            {
                AssetDatabase.CreateFolder(packageData.ManagedPath, packageData.LatestVersionName);
                ZipDownloadRequest<PackageData> newZipRequest = new ( latestReleaseURL, packageData.ManagedPath.ToFullPath(), packageData.LatestVersionName, packageData, CreateNewReleaseData );
                DownloadHandlerBehaviour.ProcessDownloadRequest(newZipRequest);
            }
        }

        public static void CreateNewReleaseData(PackageData packageData)
        {
            string releasePath = packageData.ManagedPath + "/" + packageData.LatestVersionName;
            ReleaseData newReleaseData = CreateInstance<ReleaseData>();
            newReleaseData.Populate(packageData, packageData.LatestVersion);
            AssetDatabase.CreateAsset(newReleaseData, releasePath + "/ReleaseData.asset");
            newReleaseData = AssetDatabase.LoadAssetAtPath<ReleaseData>(releasePath + "/ReleaseData.asset");
            newReleaseData.Populate(packageData, packageData.LatestVersion);

            EditorUtility.SetDirty(newReleaseData);
            EditorUtility.SetDirty(packageData);

            instance.RecentlyInstalledPackages.Add(newReleaseData.PackageData);
            instance.Save(true);

            AssetDatabase.AllowAutoRefresh();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private void PostProcessPackages()
        {
            if (RecentlyInstalledPackages.Count == 0) return;
            foreach (PackageData packageData in RecentlyInstalledPackages)
            {
                if (packageData.Icon == null) continue;

                ReleaseData releaseData = packageData.InstalledReleases.First();
                foreach (DefaultAsset assemblyFile in releaseData.AssemblyFiles)
                {
                    string assemblyPath = AssetDatabase.GetAssetPath(assemblyFile);
                    AssetImporter importer = AssetImporter.GetAtPath(assemblyPath);
                    if (importer != null && importer is PluginImporter pluginImporter)
                    {
                        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(assemblyPath))
                            if (asset is MonoScript monoScript)
                            {
                                pluginImporter.SetIcon(monoScript.GetClass().FullName, releaseData.Icon);
                                EditorUtility.SetDirty(monoScript);
                            }
                        EditorUtility.SetDirty(pluginImporter);
                        pluginImporter.SaveAndReimport();
                    }
                }
                string releasePath = packageData.ManagedPath + "/" + packageData.LatestVersionName;
                foreach (DefaultAsset assemblyAsset in releaseData.AssemblyFiles)
                {
                    string currentPath = AssetDatabase.GetAssetPath(assemblyAsset);
                    string idealPath = releasePath + "/" + assemblyAsset.name + ".dll";
                    if (currentPath != idealPath)
                        AssetDatabase.MoveAsset(currentPath, idealPath);
                }

                Debug.Log(releasePath);
                foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { releasePath }))
                {
                    UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid));
                    string assetPath = AssetDatabase.GetAssetPath(asset);
                    Debug.Log(assetPath);
                    if (AssetDatabase.IsValidFolder(assetPath))
                        AssetDatabase.DeleteAsset(assetPath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }
            Debug.Log("Finished Installing #" + RecentlyInstalledPackages.Count + " Packages.");
            RecentlyInstalledPackages.Clear();
        }

        public static void UninstallPackage(PackageData packageData)
        {
            AssetDatabase.DisallowAutoRefresh();

            AssetDatabase.DeleteAsset(packageData.ManagedPath);
            AssetDatabase.AllowAutoRefresh();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
