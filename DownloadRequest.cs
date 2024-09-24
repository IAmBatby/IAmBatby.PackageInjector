using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace IAmBatby.PackageInjector
{
    public struct TextDownloadRequest
    {
        public string URL { get; private set; }
        public Action<string> SuccessCallback { get; private set; }

        public TextDownloadRequest(string newURL,  Action<string> newSuccessCallback)
        {
            URL = newURL;
            SuccessCallback = newSuccessCallback;
        }

        public IEnumerator ProcessRequest()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(URL))
            {
                yield return webRequest.SendWebRequest();
                string[] pages = URL.Split('/');
                int page = pages.Length - 1;

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(pages[page] + ": Error (" + webRequest.result + ") : " + webRequest.error);
                    yield return null;
                }

                SuccessCallback.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    public struct ZipDownloadRequest<T>
    {
        public string URL { get; private set; }
        public string DestinationPath { get; private set; }
        public string FileName { get; private set; }
        public T Value { get; private set; }
        public Action<T> SuccessCallback { get; private set; }

        public ZipDownloadRequest(string newURL, string newDestination, string newFileName, T newValue, Action<T> newSuccessCallback)
        {
            URL = newURL;
            DestinationPath = newDestination;
            FileName = newFileName;
            Value = newValue;
            SuccessCallback = newSuccessCallback;
        }

        public IEnumerator ProcessRequest()
        {
            string destinationPath = DestinationPath + "/" + FileName + ".zip";
            UnityWebRequest unityWebRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
            unityWebRequest.downloadHandler = new DownloadHandlerFile(destinationPath);

            yield return unityWebRequest.SendWebRequest();
            yield return new WaitUntil(() => unityWebRequest.result != UnityWebRequest.Result.InProgress);


            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                string localPath = DestinationPath.Substring(DestinationPath.IndexOf("Packages/"));
                ZipFile.ExtractToDirectory(destinationPath, localPath + "/" + FileName);
                System.IO.File.Delete(destinationPath);
                SuccessCallback.Invoke(Value);
            }
            else
                Debug.LogError("Finished Download: " + destinationPath + " With Status: " + unityWebRequest.result);
        }
    }
}
