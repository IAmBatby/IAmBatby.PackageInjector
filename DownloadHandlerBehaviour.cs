using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using System.IO.Compression;

namespace IAmBatby.PackageInjector
{
    public class DownloadHandlerBehaviour : MonoBehaviour
    {
        private static DownloadHandlerBehaviour _instance;
        public static DownloadHandlerBehaviour Instance
        {
            get
            {
                if (_instance == null)
                    _instance = UnityEngine.Object.FindFirstObjectByType<DownloadHandlerBehaviour>();
                return _instance;
            }
        }

        public void TryDownloadPackage(PackageInfo packageInfo)
        {
            TryDeletePackage(packageInfo);
            AssetDatabase.DisallowAutoRefresh();
            StartCoroutine(DownloadPackageInfo(packageInfo));
        }

        public void TryGetManifest(string thunderstoreURL)
        {
            StartCoroutine(GetManifest(thunderstoreURL));
        }

        public void TrySetIcon(PackageData packageData)
        {
            StartCoroutine(GetIcon(packageData));
        }

        public void TryDownloadLatest(PackageData packageData)
        {
            StartCoroutine(DownloadLatestRelease(packageData));
        }

        private void TryDeletePackage(PackageInfo packageInfo)
        {
            DefaultAsset assembly = (DefaultAsset)AssetDatabase.LoadAssetAtPath(packageInfo.assetsPath, typeof(DefaultAsset));
            if (assembly != null)
            {
                Debug.Log("Found Preexisting Install Of Package!");
                if (AssetDatabase.DeleteAsset(packageInfo.assetsPath))
                    Debug.Log("Deleted Preexisting Install!");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private IEnumerator DownloadPackageInfo(PackageInfo packageInfo)
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

            OnDownloadFinished();
        }

        public string GetThunderstoreAPIURL(string projectNamespace, string projectName)
        {
            return ("https://thunderstore.io/api/experimental/package/" + projectNamespace + "/" + projectName + "/");
        }

        private IEnumerator GetManifest(string thunderstoreURL)
        {
            string skippedUrl = thunderstoreURL.Substring(thunderstoreURL.IndexOf("/p/") + 3);

            string projectNamespace = skippedUrl.Replace(skippedUrl.Substring(skippedUrl.IndexOf("/")), string.Empty);
            string projectName = skippedUrl.Substring(skippedUrl.IndexOf("/") + 1);
            projectName = projectName.Replace("/", string.Empty);

            string url = GetThunderstoreAPIURL(projectNamespace, projectName);
            Debug.Log(url);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = url.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        PackageInjectorEditorWindow.CreateNewPackageData(webRequest.downloadHandler.text);
                        break;
                }
            }
        }
        
        private IEnumerator DownloadLatestRelease(PackageData packageData)
        {
            Debug.Log("Attempting To Download: " + packageData.LatestReleaseURL);
            string destinationPath = packageData.FullPackageInjectorFolder + "/" + "LatestRelease.zip";
            UnityWebRequest unityWebRequest = new UnityWebRequest(packageData.LatestReleaseURL, UnityWebRequest.kHttpVerbGET);
            unityWebRequest.downloadHandler = new DownloadHandlerFile(destinationPath);

            yield return unityWebRequest.SendWebRequest();
            yield return new WaitUntil(() => unityWebRequest.result != UnityWebRequest.Result.InProgress);

            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {

                AssetDatabase.CreateFolder(packageData.PackageInjectorFolder, packageData.Version);
                if (AssetDatabase.IsValidFolder(packageData.PackageInjectorFolder + "/" + packageData.Version))
                {
                    ZipFile.ExtractToDirectory(packageData.FullPackageInjectorFolder + "/" + "LatestRelease.zip", packageData.FullPackageInjectorFolder + "/" + packageData.Version);
                    AssetDatabase.Refresh();

                    ReleaseData newReleaseData = ScriptableObject.CreateInstance<ReleaseData>();
                    string path = packageData.PackageInjectorFolder + "/" + packageData.Version + "/" + "ReleaseData-" + packageData.Version + ".asset";
                    AssetDatabase.CreateAsset(newReleaseData, path);
                    newReleaseData = (ReleaseData)AssetDatabase.LoadAssetAtPath(path, typeof(ReleaseData));
                    newReleaseData.Populate(packageData, packageData.VersionNumber);

                    System.IO.File.Delete(destinationPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            Debug.Log("Finished Download With Status: " + unityWebRequest.result);
        }
        
        private IEnumerator GetIcon(PackageData package)
        {
            UnityWebRequest wr = new UnityWebRequest(package.SeekValue("icon"));
            DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
            wr.downloadHandler = texDl;
            yield return wr.SendWebRequest();
            if (wr.result == UnityWebRequest.Result.Success)
            {
                Texture2D t = texDl.texture;
                Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                    Vector2.zero, 1f);
                package.Icon = s;
            }
        }

        private void ParseManifestData(string downloadHandlerText)
        {
            Debug.Log("Author: " + SeekText(downloadHandlerText, "\"owner\":", ","));
            Debug.Log("Version: " + SeekText(downloadHandlerText, "\"version_number\":", ","));
            Debug.Log("Thunderstore URL: " + SeekText(downloadHandlerText, "\"package_url\":", ","));
            Debug.Log("Descripton: " + SeekText(downloadHandlerText, "\"description\":", ","));
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

        private void OnDownloadFinished()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.AllowAutoRefresh();
        }
    }
}
