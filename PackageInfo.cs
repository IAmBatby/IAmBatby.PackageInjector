using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

namespace IAmBatby.PackageInjector
{
    [System.Serializable]
    public class PackageInfo
    {
        public string PackageName;
        public string packageFolder;
        public string dllLinkPath;
        public string fullPath;
        public string assetsPath;
    }
}
