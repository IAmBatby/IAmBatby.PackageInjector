using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public class PackageInjectorEditorWindow : EditorWindow
    {
        public static string newPackageURL;
        public static string newThunderstoreURL;
        public static List<PackageData> allPackages;
        public static PackageData SelectedPackage { get; private set; }

        public static bool typesBool;

        public static Vector2 monoScroll;
        public static Vector2 scriptableScroll;

        public static Vector2 allPackagesScroll;
        [MenuItem("PackageInjector/Manage Packages")]
        public static void OpenWindow()
        {
            TryRefresh();
            PackageInjectorEditorWindow window = GetWindow<PackageInjectorEditorWindow>();
            window.Show();
        }

        public static void TryRefresh()
        {
            if (allPackages == null)
                allPackages = FindAssets<PackageData>();
            else
                foreach (PackageData package in allPackages)
                    if (package == null)
                    {
                        allPackages = null;
                        break;
                    }
        }

        public static List<T> FindAssets<T>() where T : UnityEngine.Object
        {
            List<T> returnList = new List<T>();
            T t = null;
            Debug.Log(typeof(T).Name);
            string[] prefabGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (asset != null)
                        returnList.Add(asset);
                }
            }
            return (returnList);
        }

        public void OnGUI()
        {
            TryRefresh();

            GUILayout.ExpandWidth(false);
            GUI.skin.label.richText = true;
            GUI.skin.textField.richText = true;

            EditorGUILayout.BeginHorizontal();

            newThunderstoreURL = EditorGUILayout.TextField(newThunderstoreURL);

            if (GUILayout.Button("Add New Thunderstore Package"))
            {
                if (DownloadHandlerBehaviour.Instance != null)
                    DownloadHandlerBehaviour.Instance.TryGetManifest(newThunderstoreURL);
            }

            EditorGUILayout.EndHorizontal();


            if (SelectedPackage == null && allPackages != null && allPackages.Count > 0)
                SelectedPackage = allPackages[0];

            //Main Window

            
            GUILayout.BeginHorizontal();


            GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
            DrawAllPackages();
            GUILayout.EndVertical();

            //GUILayout.FlexibleSpace();

            GUILayout.Space(10);

            Rect titleRect = EditorGUILayout.BeginVertical();
            if (SelectedPackage != null)
                DrawSelectedPackageData(SelectedPackage, titleRect);
            GUILayout.EndVertical();


            GUILayout.EndHorizontal();
            

            //if (SelectedPackage != null)
                //DrawSelectedPackageData(SelectedPackage);

        }

        public void DrawAllPackages()
        {
            EditorGUILayout.LabelField("All Managed Packages", EditorStyles.boldLabel);

            allPackagesScroll = EditorGUILayout.BeginScrollView(allPackagesScroll, false, true, GUILayout.ExpandWidth(false));

            if (allPackages != null)
                foreach (PackageData data in allPackages)
                    DrawPackageData(data);

            EditorGUILayout.EndScrollView();
        }

        public void DrawPackageData(PackageData packageData)
        {
            GUIStyle newStyle = new GUIStyle("ButtonMid");
            newStyle.alignment = TextAnchor.MiddleLeft;
            if (packageData == SelectedPackage)
            {
                newStyle.fontStyle = FontStyle.Bold;
                newStyle.normal.textColor = Color.yellow;
                Color blue = new Color(0.6f, 0.1f, 0, 1);
                SetColor(newStyle.focused.background, blue);
                SetColor(newStyle.normal.background, blue);
                SetColor(newStyle.hover.background, blue);
                SetColor(newStyle.active.background, blue);
            }
            if (GUILayout.Button(packageData.PackageFileName, newStyle))
                SelectedPackage = packageData;
        }

        public void SetColor(Texture2D texture, Color color)
        {
            if (texture == null) return;
            var colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
                colors[i] = color;
            texture.SetPixels(colors);
            texture.Apply();
        }

        public void DrawSelectedPackageData(PackageData packageData, Rect titleRect)
        {
            float size = 100;
            GUILayout.Space(15);
            Rect newTitleRect = EditorGUILayout.BeginHorizontal();
            GUIStyle headerStyle = new GUIStyle("SettingsHeader");
            EditorGUILayout.SelectableLabel(packageData.PackageFileName, headerStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.SelectableLabel(packageData.Version, new GUIStyle("ProfilerSelectedLabel"), GUILayout.MaxHeight(15));
            EditorGUILayout.SelectableLabel("By " + packageData.PackageAuthor, GUILayout.MaxHeight(15));
            GUIStyle newStyle = new GUIStyle("WordWrappedMiniLabel");
            newStyle.fontStyle = FontStyle.Italic;
            EditorGUILayout.SelectableLabel(packageData.ID, newStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Download Latest Release", GUILayout.MaxWidth(200)))
                if (DownloadHandlerBehaviour.Instance != null)
                    DownloadHandlerBehaviour.Instance.TryDownloadLatest(packageData);

            if (GUILayout.Button("Refresh Installs", GUILayout.MaxWidth(200)))
                foreach (ReleaseData releaseData in packageData.InstalledReleases)
                    releaseData.Populate(packageData, packageData.VersionNumber);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            Rect rect = DrawValue("Package Name:", packageData.PackageName);
            DrawValue("Package Author:", packageData.PackageAuthor);
            DrawValue("Package Description:", packageData.PackageDescription);
            //DrawValue("Package Version:", packageData.Version);
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PrefixLabel("Version:", EditorStyles.boldLabel);
            Vector3Int version = packageData.VersionNumber;
            EditorGUILayout.IntField(version.x, GUILayout.MaxWidth(20));
            EditorGUILayout.IntField(version.y, GUILayout.MaxWidth(20));
            EditorGUILayout.IntField(version.z, GUILayout.MaxWidth(20));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            DrawValue("Package URL:", packageData.URL);

            GUILayout.Space(15);

            DrawValue("Package Location:", packageData.PackageFolder);

            if (packageData.AssemblyAsset != null)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PrefixLabel("Assembly Asset:", EditorStyles.boldLabel);
                EditorGUILayout.ObjectField(packageData.AssemblyAsset, typeof(DefaultAsset));
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.BeginVertical();
            if (packageData.AssemblyAsset != null)
            {
                if (packageData.ScriptableObjects.Count == 0 && packageData.MonoBehaviours.Count == 0)
                    packageData.PopulateTypes();

                typesBool = EditorGUILayout.BeginFoldoutHeaderGroup(typesBool, "View Assembly Contents");

                if (typesBool)
                {
                    EditorGUILayout.PrefixLabel("MonoBehaviours", EditorStyles.boldLabel);
                    monoScroll = EditorGUILayout.BeginScrollView(monoScroll);
                    foreach (MonoScript monoBehaviourType in packageData.MonoBehaviours)
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(monoBehaviourType, typeof(MonoScript));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.PrefixLabel("ScriptableObjects", EditorStyles.boldLabel);
                    scriptableScroll = EditorGUILayout.BeginScrollView(scriptableScroll);
                    foreach (MonoScript scriptableObjectType in packageData.ScriptableObjects)
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(scriptableObjectType, typeof(MonoScript));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndVertical();

            Rect newRect = new Rect(new Vector2(newTitleRect.max.x - (size + 10), newTitleRect.position.y), new Vector2(size,size));
            if (packageData.Icon != null)
                EditorGUI.DrawPreviewTexture(newRect, packageData.Icon.texture);
          
        }

        public Rect DrawValue(string title, string value)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PrefixLabel(title, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(value))
                EditorGUILayout.TextField(value, EditorStyles.textField);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            return (rect);
        }

        public static void CreateNewPackageData(string downloadHandlerText)
        {
            if (AssetDatabase.IsValidFolder(PackageInjectorManager.Instance.packageDataPath))
            {
                PackageData newPackageData = ScriptableObject.CreateInstance<PackageData>();
                newPackageData.Populate(downloadHandlerText);
                string location = newPackageData.PackageDataLocation;
                string newFolder = newPackageData.PackageInjectorFolder;
                if (AssetDatabase.IsValidFolder(newPackageData.PackageInjectorFolder) == false)
                {
                    Debug.Log("Creating New Package Folder At: " + newFolder.Replace(newFolder.Substring(newFolder.IndexOf("/")), string.Empty) + " : " + newFolder.Substring(newFolder.IndexOf("/") + 1));
                    AssetDatabase.CreateFolder(PackageInjectorManager.Instance.packageDataPath, newPackageData.ID);
                }
                if (AssetDatabase.IsValidFolder(newFolder) == false)
                {
                    Debug.LogError("Failed To Make Package Folder!");
                    return;
                }
                AssetDatabase.CreateAsset(newPackageData, location);
                newPackageData = (PackageData)AssetDatabase.LoadAssetAtPath(location, typeof(PackageData));
                newPackageData.Populate(downloadHandlerText);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                allPackages = null;
            }
            else
                Debug.LogError("Folder Path: " + PackageInjectorManager.Instance.packageDataPath + " Is Invalid!");
        }

        public void TryParseManifest(string newThunderstoreURL)
        {
            if (DownloadHandlerBehaviour.Instance != null)
                DownloadHandlerBehaviour.Instance.TryGetManifest(newThunderstoreURL);
        }
    }
}
