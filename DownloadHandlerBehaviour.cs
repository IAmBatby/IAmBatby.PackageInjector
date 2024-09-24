using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor;
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

        public static void ProcessDownloadRequest(TextDownloadRequest request) => TryStartCoroutine(request.ProcessRequest());

        public static void ProcessDownloadRequest<T>(ZipDownloadRequest<T> request) => TryStartCoroutine(request.ProcessRequest());

        public static void TryStartCoroutine(IEnumerator coroutine) => Instance?.StartCoroutine(coroutine);
    }
}
