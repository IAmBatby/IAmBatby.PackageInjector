using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace IAmBatby.PackageInjector
{
    public class DownloadHandler
    {
        public static DownloadHandler Instance { get; private set; }

        [InitializeOnLoadMethod]
        public static void RefreshInstace()
        {
            Instance = new DownloadHandler();
        }

        public static bool ValidatePackageInfo(PackageInfo packageInfo)
        {
            if (packageInfo == null) return (false);
            if (string.IsNullOrEmpty(packageInfo.dllLinkPath)) return (false);
            if (string.IsNullOrEmpty(packageInfo.PackageName)) return (false);
            if (string.IsNullOrEmpty(packageInfo.packageFolder)) return (false);
            if (string.IsNullOrEmpty(packageInfo.fullPath)) return (false);

            if (packageInfo.dllLinkPath.Contains(".dll"))
                return (true);

            return (false);
        }

        public static string GetTargetPath(PackageInfo packageInfo)
        {
            string targetPathFolder = packageInfo.packageFolder;
            if (targetPathFolder.Contains("Assets"))
                targetPathFolder = targetPathFolder.Replace("Assets", string.Empty);

            targetPathFolder = Application.dataPath + targetPathFolder;

            targetPathFolder += "/" + packageInfo.PackageName + ".dll";

            return (targetPathFolder);
        }

        public static void PopulatePackageInfo(PackageInfo packageInfo)
        {
            if (packageInfo == null) return;
            if (string.IsNullOrEmpty(packageInfo.dllLinkPath)) return;
            if (!packageInfo.dllLinkPath.Contains(".dll")) return;

            string dllName = packageInfo.dllLinkPath.Substring(packageInfo.dllLinkPath.LastIndexOf("/") + 1);
            packageInfo.PackageName = dllName.Replace(dllName.Substring(dllName.IndexOf(".dll")), string.Empty);
            packageInfo.packageFolder = PackageInjectorManager.Instance.targetPluginsPath + "/" + packageInfo.PackageName;
            packageInfo.fullPath = GetTargetPath(packageInfo);
            packageInfo.assetsPath = packageInfo.fullPath.Substring(packageInfo.fullPath.IndexOf("Assets/"));
        }

        public static void TryDownloadPackage(PackageInfo packageInfo)
        {
            if (DownloadHandlerBehaviour.Instance != null)
                DownloadHandlerBehaviour.Instance.TryDownloadPackage(packageInfo);
        }
        
        private IEnumerator DownloadPackage(PackageInfo packageInfo)
        {
            string debugString = string.Empty;
            UnityWebRequest unityWebRequest = new UnityWebRequest(packageInfo.dllLinkPath, UnityWebRequest.kHttpVerbGET);
            unityWebRequest.downloadHandler = new DownloadHandlerFile(packageInfo.fullPath);

            yield return unityWebRequest.SendWebRequest();

            yield return new WaitUntil(() => unityWebRequest.result != UnityWebRequest.Result.InProgress);

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                debugString = "Failed To Download: " + packageInfo.PackageName + "\n";
                debugString += "Link: " + packageInfo.dllLinkPath + "\n";
                debugString += "Destination: " + packageInfo.fullPath + "\n";
                debugString += "Result: " + unityWebRequest.result + "\n";
                debugString += "Code: " + unityWebRequest.responseCode + "\n";
                Debug.LogError(debugString);
            }
            else
            {
                debugString = "Succesfully Downloaded: " + packageInfo.PackageName;
                Debug.Log(debugString);
            }
            AssetDatabase.Refresh();
        }
        
        public static void StartCoroutine(IEnumerator routine, Action end = null)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (routine.MoveNext() == false)
                    {
                        if (end != null)
                            end();
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    if (end != null)
                        end();
                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }
    }
}
