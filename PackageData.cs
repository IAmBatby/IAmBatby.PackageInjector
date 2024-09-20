using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

namespace IAmBatby.PackageInjector
{
    public class PackageData : ScriptableObject
    {
        [field: Header("Package Information")]

        [field: SerializeField] public string PackageName { get; private set; }
        [field: SerializeField] public string PackageAuthor { get; private set; }
        [field: SerializeField] public string PackageDescription { get; private set; }
        [field: SerializeField] public string ThunderstoreURL { get; private set; }
        [field: SerializeField] public string LatestReleaseURL { get; private set; }
        public string ID => "com." + PackageAuthor.ToLowerInvariant() + "." + PackageName.ToLowerInvariant();
        [field: SerializeField] public string Version { get; private set; }
        [field: SerializeField] public Sprite Icon { get; set; }



        [field: Header("Project Install Information")]


        [SerializeField] private DefaultAsset _assemblyAsset;

        [field: SerializeField] public List<MonoScript> MonoBehaviours { get; private set; } = new List<MonoScript>();
        [field: SerializeField] public List<MonoScript> ScriptableObjects { get; private set; } = new List<MonoScript>();

        public DefaultAsset AssemblyAsset
        {
            get
            {
                if (_assemblyAsset == null)
                    _assemblyAsset = (DefaultAsset)AssetDatabase.LoadAssetAtPath(AssetsFolder, typeof(DefaultAsset));
                return _assemblyAsset;
            }
        }

        public Vector3Int VersionNumber
        {
            get
            {
                if (string.IsNullOrEmpty(Version) || !Version.Contains("."))
                    return Vector3Int.zero;
                else
                {
                    string version = Version;
                    string xFloat = version.Replace(version.Substring(version.IndexOf(".")), string.Empty);
                    string yFloat = version.Substring(version.IndexOf(".") + 1).Replace(version.Substring(version.IndexOf(".")), string.Empty);
                    string zFloat = version.Substring(version.LastIndexOf(".") + 1);
                    return (new Vector3Int((int)float.Parse(xFloat), (int)float.Parse(yFloat), (int)float.Parse(zFloat)));
                }
            }
        }

        [field: SerializeField] public string PackageFileName { get; private set; }
        [field: SerializeField] public string PackageFolder { get; private set; }
        [field: SerializeField] public string AssetsFolder { get; private set; }
        [field: SerializeField] public string FullFolder { get; private set; }

        [field: Space(15)]
        [field: SerializeField] public string URL { get; private set; }

        [field: SerializeField] public string PackageInjectorFolder { get; private set; }
        [field: SerializeField] public string FullPackageInjectorFolder { get; private set; }
        [field: SerializeField] public List<ReleaseData> InstalledReleases { get; private set; } = new List<ReleaseData>();
        public string PackageDataLocation => PackageInjectorFolder + "/" + name + ".asset";

        private string cachedDownloadHandlerText;

        private void Awake()
        {
            //if (Icon == null && cachedDownloadHandlerText != null)
                //if (DownloadHandlerBehaviour.Instance != null)
                    //DownloadHandlerBehaviour.Instance.TrySetIcon(this);
        }

        private void OnEnable()
        {
            if (Icon == null && !string.IsNullOrEmpty(cachedDownloadHandlerText))
                if (DownloadHandlerBehaviour.Instance != null)
                    DownloadHandlerBehaviour.Instance.TrySetIcon(this);
        }

        internal void Populate(string downloadHandlerText)
        {
            cachedDownloadHandlerText = downloadHandlerText;

            PackageName = SeekTSValue(downloadHandlerText, "name");
            PackageAuthor = SeekTSValue(downloadHandlerText, "owner");
            URL = SeekTSValue(downloadHandlerText, "package_url");
            PackageDescription = SeekTSValue(downloadHandlerText, "description");
            Version = SeekTSValue(downloadHandlerText, "version_number");
            LatestReleaseURL = SeekTSValue(downloadHandlerText, "download_url");

            string dllName = PackageName + ".dll";
            PackageFileName = dllName.Replace(dllName.Substring(dllName.IndexOf(".dll")), string.Empty);
            PackageFolder = PackageInjectorManager.Instance.targetPluginsPath + "/" + PackageFileName;
            FullFolder = GetTargetPath(PackageFileName, PackageFolder);
            AssetsFolder = FullFolder.Substring(FullFolder.IndexOf("Assets/"));

            string applicationRoot = FullFolder.Replace(AssetsFolder, string.Empty);
            FullPackageInjectorFolder = applicationRoot + PackageInjectorFolder;

            FullPackageInjectorFolder = FullPackageInjectorFolder.Replace("com.iambatby.packageinjector", "IAmBatby.PackageInjector");


            PackageInjectorFolder = PackageInjectorManager.Instance.packageDataPath + "/" + ID;

            name = PackageFileName;

            if (Icon == null && !string.IsNullOrEmpty(cachedDownloadHandlerText))
                if (DownloadHandlerBehaviour.Instance != null)
                    DownloadHandlerBehaviour.Instance.TrySetIcon(this);
        }

        internal void PopulateTypes()
        {
            MonoBehaviours.Clear();
            ScriptableObjects.Clear();
            if (AssemblyAsset != null)
            {
                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetsFolder))
                {
                    if (asset is MonoScript monoScript)
                    {
                        Type monoType = monoScript.GetClass();
                        CheckTypeIterative(monoType, monoScript);
                    }
                }

            }

            Debug.Log("MonoBehaviours Count: " + MonoBehaviours.Count);
            Debug.Log("ScriptableObjects Count: " + ScriptableObjects.Count);
        }

        internal void CheckTypeIterative(Type type, MonoScript monoScript)
        {
            Debug.Log("Nested Type: " + type);
            if (type.FullName.Contains("MonoBehaviour"))
                MonoBehaviours.Add(monoScript);
            else if (type.FullName.Contains("ScriptableObject"))
                ScriptableObjects.Add(monoScript);
            else
            {
                Type baseType = type.BaseType;
                if (baseType != null)
                    CheckTypeIterative(baseType, monoScript);
            }
        }

        private static string GetTargetPath(string packageName, string packageFolder)
        {
            string targetPathFolder = packageFolder;
            if (targetPathFolder.Contains("Assets"))
                targetPathFolder = targetPathFolder.Replace("Assets", string.Empty);

            targetPathFolder = Application.dataPath + targetPathFolder;

            targetPathFolder += "/" + packageName + ".dll";

            return (targetPathFolder);
        }

        public string SeekValue(string searchKeyword)
        {
            return (SeekTSValue(cachedDownloadHandlerText, searchKeyword));
        }

        private string SeekTSValue(string text, string searchKeyword)
        {
            return (SeekText(text, TSFormat(searchKeyword), ",")).Replace("\"", string.Empty);
        }

        private string TSFormat(string keyword)
        {
            return ("\"" + keyword + "\":");
        }

        private string SeekText(string text, string searchTerm, string endIdentifier)
        {
            if (text.Contains(searchTerm))
            {
                string skip = text.Substring(text.IndexOf(searchTerm) + searchTerm.Length);

                string result = skip.Replace(skip.Substring(skip.IndexOf(endIdentifier)), string.Empty);
                return (result);
            }
            Debug.LogError("Could Not Find Text With: " + searchTerm);
            return (text);
        }
    }
}
