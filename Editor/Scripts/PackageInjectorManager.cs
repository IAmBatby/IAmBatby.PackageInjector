using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditorInternal;
using System.Reflection.Emit;
using System.Linq;

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

        private List<PackageData> RecentlyInstalledPackages = new List<PackageData>();

        private void OnEnable()
        {
            Validate();
            PostProcessPackages();
        }
        private void OnDisable() => Save(true);

        public static void Validate()
        {
            int count = 0;
            foreach (PackageData package in new List<PackageData>(instance.AllPackages))
            {
                if (package == null)
                    instance.AllPackages.RemoveAt(0);
                count++;
            }
        }

        [MenuItem("PackageInjector/GetAllPackages")]
        public static void GetAllPackages()
        {
            //string url = 
            //DownloadHandlerBehaviour.ProcessDownloadRequest(new TextDownloadRequest(url, DebugPackages));
        }

        public static void DebugPackages(string result)
        {
            Debug.Log(result);
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

        public static void TryDownloadLatestPackageVersion(PackageData packageData)
        {
            if (packageData.TryGetLatestReleaseURL(out string latestReleaseURL))
            {
                AssetDatabase.CreateFolder(packageData.ManagedPath, packageData.LatestVersionName);
                ZipDownloadRequest<PackageData> newZipRequest = new ZipDownloadRequest<PackageData>
                (
                    newURL: latestReleaseURL,
                    newDestination: packageData.ManagedPath.ToFullPath(),
                    newFileName: packageData.LatestVersionName,
                    newValue: packageData,
                    newSuccessCallback: CreateNewReleaseData
                );

                Debug.Log("Trying To Download New Release: " + newZipRequest.URL + " - " + newZipRequest.DestinationPath + " - " + newZipRequest.FileName);
                DownloadHandlerBehaviour.ProcessDownloadRequest(newZipRequest);
            }
        }

        public static void TryInstallRelease(ReleaseData releaseData)
        {
            //AssetDatabase.DisallowAutoRefresh();
            //if (AssetDatabase.IsValidFolder(releaseData.PackageData.InstallPath))
                //AssetDatabase.DeleteAsset(releaseData.PackageData.InstallPath);
            //AssetDatabase.CreateFolder(PackageInjectorManager.LocalPluginsFolder, releaseData.PackageData.UUID);

            foreach (DefaultAsset assembly in releaseData.AssemblyFiles)
            {
                //string path = AssetDatabase.GetAssetPath(assembly);
                //string newPath = releaseData.PackageData.InstallPath + "/" + assembly.name + ".dll";
                //AssetDatabase.CopyAsset(path, newPath);

                //DefaultAsset newCopy = AssetDatabase.LoadAssetAtPath<DefaultAsset>(newPath);

                //releaseData.InstalledAssemblyFiles.Add(newCopy);
                releaseData.InstalledAssemblyFiles.Add(assembly);
            }

            instance.RecentlyInstalledPackages.Add(releaseData.PackageData);
            instance.Save(true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CreateNewReleaseData(PackageData packageData)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ReleaseData newReleaseData = Utilities.CreateAndSave<ReleaseData>(packageData.ManagedPath + "/" + packageData.LatestVersionName);
            newReleaseData.Populate(packageData, packageData.LatestVersion);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            TryInstallRelease(newReleaseData);
        }

        public static void CreateNewPackageData<T>(string downloadHandlerText) where T : PackageData
        {
            AssetDatabase.SaveAssets();
            if (AssetDatabase.IsValidFolder(PackagePath))
            {
                T newPackageData = CreateInstance<T>();
                newPackageData.SetManifestData(downloadHandlerText);

                if (AssetDatabase.IsValidFolder(newPackageData.ManagedPath) == false)
                    AssetDatabase.CreateFolder(PackagePath, newPackageData.UUID);

                string location = newPackageData.LocalLocation;
                AssetDatabase.CreateAsset(newPackageData, location);
                newPackageData = AssetDatabase.LoadAssetAtPath(location, typeof(T)) as T;
                newPackageData.SetManifestData(downloadHandlerText);
                Utilities.SetAssetName(newPackageData, "ReleaseData");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                TryDownloadLatestPackageVersion(newPackageData);
            }
        }

        private void PostProcessPackages()
        {
            if (RecentlyInstalledPackages.Count == 0) return;
            foreach (PackageData packageData in RecentlyInstalledPackages)
            {
                ReleaseData releaseData = packageData.InstalledReleases.First();
                foreach (DefaultAsset assemblyFile in releaseData.InstalledAssemblyFiles)
                {
                    string assemblyPath = AssetDatabase.GetAssetPath(assemblyFile);
                    AssetImporter importer = AssetImporter.GetAtPath(assemblyPath);
                    if (importer != null && importer is PluginImporter pluginImporter)
                    {
                        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assemblyPath);
                        if (assets != null && assets.Length > 0)
                        {
                            foreach (UnityEngine.Object asset in assets)
                                if (asset is MonoScript monoScript)
                                {
                                    if (releaseData.Icon != null)
                                    {
                                        pluginImporter.SetIcon(monoScript.GetClass().FullName, releaseData.Icon);
                                        EditorUtility.SetDirty(monoScript);
                                    }
                                }
                            EditorUtility.SetDirty(pluginImporter);
                            pluginImporter.SaveAndReimport();
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        else
                            Debug.LogError("No MonoScript Assets Found! Path: " + assemblyPath);
                    }
                }
            }
            Debug.Log("Finished Installing #" + RecentlyInstalledPackages.Count + " Packages.");
            RecentlyInstalledPackages.Clear();
        }
    }
}
