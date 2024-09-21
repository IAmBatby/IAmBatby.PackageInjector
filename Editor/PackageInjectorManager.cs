using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    [CreateAssetMenu(fileName = "PackageInjectorManager", menuName = "ScriptableObjects/PackageInjectorManager", order = 1)]
    public class PackageInjectorManager : ScriptableObject
    {
        private static PackageInjectorManager _instance;
        public static PackageInjectorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    PackageInjectorManager[] packageManagers = (PackageInjectorManager[])Resources.FindObjectsOfTypeAll(typeof(PackageInjectorManager));
                    _instance = packageManagers[0];
                }
                return (_instance);
            }
        }

        public string packageDataPath;
        public string targetPluginsPath;

        public Color testColor;

        public List<PackageInfo> packageInfos = new List<PackageInfo>();

        [MenuItem("PackageInjector/Populate Packages")]
        public static void PopulatePackages()
        {
            foreach (PackageInfo packageInfo in Instance.packageInfos)
            {
                DownloadHandler.PopulatePackageInfo(packageInfo);
            }
        }

        [MenuItem("PackageInjector/Get Packages")]
        public static void GetPackages()
        {
            foreach (PackageInfo packageInfo in Instance.packageInfos)
            {
                DownloadHandler.PopulatePackageInfo(packageInfo);
                if (DownloadHandler.ValidatePackageInfo(packageInfo) == true)
                {
                    Debug.Log("Trying To Download: " + packageInfo.PackageName);
                    DownloadHandler.TryDownloadPackage(packageInfo);
                }
                else
                    Debug.LogError("Package: " +  packageInfo.PackageName + " Was Invalid!");
            }
        }


        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            _instance = this;
        }

        private void OnValidate()
        {
            _instance = this;
        }
    }
}
