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

        public enum PackageInfoType { Overview, Downloads, Installs }
        public PackageInfoType CurrentPackageInfoTypeSetting;

        public RectOffset previousPadding;

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
                else
                    Debug.LogError("DownloadHandler Not Found!");
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
            float size = 125;

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            GUIStyle headerStyle = new GUIStyle("SettingsHeader");
            EditorGUILayout.SelectableLabel(packageData.PackageFileName, headerStyle);


            EditorGUILayout.SelectableLabel(packageData.Version, new GUIStyle("ProfilerSelectedLabel"), GUILayout.MaxHeight(15));
            EditorGUILayout.SelectableLabel("By " + packageData.PackageAuthor, GUILayout.MaxHeight(15));
            GUIStyle newStyle = new GUIStyle("WordWrappedMiniLabel");
            newStyle.fontStyle = FontStyle.Italic;
            EditorGUILayout.SelectableLabel(packageData.ID, newStyle);

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            GUILayout.Space(2.5f);

            GUIStyle imageStyle = new GUIStyle("AC ComponentButton");
            imageStyle.fixedWidth = size;
            imageStyle.fixedHeight = size;
            imageStyle.stretchWidth = true;
            imageStyle.stretchHeight = true;
            imageStyle.alignment = TextAnchor.MiddleRight;
            imageStyle.imagePosition = ImagePosition.ImageOnly;
            imageStyle.padding = new RectOffset(0, 0, 0, 0);

            if (packageData.Icon != null)
                GUILayout.Button(packageData.Icon, imageStyle, GUILayout.Width(size));

            GUILayout.Space(10);


            if (GUILayout.Button("Download Latest", GUILayout.MaxWidth(size)))
                if (DownloadHandlerBehaviour.Instance != null)
                    DownloadHandlerBehaviour.Instance.TryDownloadLatest(packageData);

            if (GUILayout.Button("Refresh Installs", GUILayout.MaxWidth(size)))
                foreach (ReleaseData releaseData in packageData.InstalledReleases)
                    releaseData.Populate(packageData, packageData.VersionNumber);


            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Overview", GUILayout.MaxWidth(100)))
                    CurrentPackageInfoTypeSetting = PackageInfoType.Overview;
                if (GUILayout.Button("Downloads", GUILayout.MaxWidth(100)))
                    CurrentPackageInfoTypeSetting = PackageInfoType.Downloads;
                if (GUILayout.Button("Installs", GUILayout.MaxWidth(100)))
                    CurrentPackageInfoTypeSetting = PackageInfoType.Installs;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical(Utilities.CreateStyle(true, Utilities.GetColor(43f, 41f, 43f)));

            if (CurrentPackageInfoTypeSetting == PackageInfoType.Overview)
                DrawPackageOverview(packageData);
            else if (CurrentPackageInfoTypeSetting == PackageInfoType.Downloads)
                DrawPackageDownloads(packageData);
            else if (CurrentPackageInfoTypeSetting == PackageInfoType.Installs)
                DrawPackageInstall(packageData);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        public void DrawPackageOverview(PackageData packageData)
        {
            Utilities.DrawValue("Package Name:", packageData.PackageName);
            Utilities.DrawValue("Package Author:", packageData.PackageAuthor);
            Utilities.DrawValue("Package Description:", packageData.PackageDescription);
            Utilities.DrawValue("Version:", packageData.VersionNumber);
            Utilities.DrawValue("Package URL:", packageData.URL);

        }

        public void DrawPackageDownloads(PackageData packageData)
        {
            foreach (ReleaseData releaseData in packageData.InstalledReleases)
            {
                EditorGUILayout.BeginHorizontal();
                Utilities.DrawValue("Version:", releaseData.ReleaseVersion);
                Utilities.DrawValue(string.Empty, releaseData.Path);
                EditorGUILayout.EndHorizontal();
            }
        }

        public void DrawPackageInstall(PackageData packageData)
        {
            Utilities.DrawValue("Package Location:", packageData.PackageFolder);

            if (packageData.AssemblyAsset != null)
            {

                if (packageData.ScriptableObjects.Count == 0 && packageData.MonoBehaviours.Count == 0)
                    packageData.PopulateTypes();

                Utilities.DrawValue("Assembly:", packageData.AssemblyAsset, EditorStyles.boldLabel, true, GUILayout.ExpandWidth(false));

                AssetImporter assetImporter = AssetImporter.GetAtPath(packageData.AssetsFolder);
                if (assetImporter != null && assetImporter is PluginImporter plugin)
                {
                    foreach (SerializedProperty property in Utilities.FindSerializedProperties(plugin))
                        if (property.propertyType == SerializedPropertyType.Boolean)
                            Utilities.DrawValue(string.Empty, property, readOnly: false);

                    if (GUILayout.Button("Apply MonoScript Icons"))
                    {
                        foreach (MonoScript monoBehaviour in packageData.MonoBehaviours)
                        {
                            plugin.SetIcon(monoBehaviour.GetClass().FullName, packageData.Icon);
                            EditorUtility.SetDirty(monoBehaviour);
                        }
                        foreach (MonoScript scriptableObject in packageData.ScriptableObjects)
                        {
                            plugin.SetIcon(scriptableObject.GetClass().FullName, packageData.Icon);
                            EditorUtility.SetDirty(scriptableObject);
                        }
                        EditorUtility.SetDirty(plugin);
                        AssetDatabase.SaveAssetIfDirty(packageData);
                        plugin.SaveAndReimport();
                    }
                }

                EditorGUILayout.BeginVertical();
                typesBool = EditorGUILayout.BeginFoldoutHeaderGroup(typesBool, "View Assembly Contents");

                if (typesBool)
                {
                    EditorGUILayout.PrefixLabel("MonoBehaviours", EditorStyles.boldLabel);
                    monoScroll = EditorGUILayout.BeginScrollView(monoScroll);
                    foreach (MonoScript monoBehaviourType in packageData.MonoBehaviours)
                        Utilities.DrawValue(string.Empty, monoBehaviourType, options: GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.PrefixLabel("ScriptableObjects", EditorStyles.boldLabel);
                    scriptableScroll = EditorGUILayout.BeginScrollView(scriptableScroll);
                    foreach (MonoScript scriptableObjectType in packageData.ScriptableObjects)
                        Utilities.DrawValue(string.Empty, scriptableObjectType, options: GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.EndVertical();
            }

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
