using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public static class Utilities
    {
        private static Color _DefaultBackgroundColor;
        public static Color DefaultBackgroundColor
        {
            get
            {
                if (_DefaultBackgroundColor.a == 0)
                {
                    var method = typeof(EditorGUIUtility)
                        .GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
                    _DefaultBackgroundColor = (Color)method.Invoke(null, null);
                }
                return _DefaultBackgroundColor;
            }
        }

        public static Color HeaderColor = new Color(81f / 255f, 81f / 255f, 81f / 255f, 255);
        public static Color PrimaryAlternatingColor = new Color(62f / 255f, 62f / 255f, 62f / 255f, 255);
        public static Color SecondaryAlternatingColor = new Color(43f / 255f, 41f / 255f, 43f / 255f, 255);

        public static int HeaderFontSize = 14;
        public static int TextFontSize = 13;

        [SerializeField] private static string applicationProjectPath;

        public static string GetFullPath(string unityPath)
        {
            if (string.IsNullOrEmpty(applicationProjectPath))
                applicationProjectPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/"));

            return (applicationProjectPath + "/" + unityPath);
        }

        public static T CreateAndSave<T>(string path, string name = "temp") where T: ScriptableObject
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path + "/" + name + ".asset");
            return (AssetDatabase.LoadAssetAtPath(path + "/" + name + ".asset", typeof(T)) as T);
        }

        public static void SetAssetName<T>(T asset, string newName) where T : UnityEngine.Object
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string newPath = path.Replace("/" + asset.name + ".", "/" + newName + ".");
            AssetDatabase.RenameAsset(path, newPath);
            Debug.LogError("Failed To Rename Asset: \n Original Path: " + path + "\n New Path: " +  newPath);
        }

        public static string GetAssetPath(UnityEngine.Object asset, bool getLocalPath = true, bool includeFile = false)
        {
            string localPath = AssetDatabase.GetAssetPath(asset);

            if (includeFile == false)
                localPath = localPath.Remove(localPath.LastIndexOf("/"));

            if (getLocalPath == true)
                return (localPath);
            else
                return (GetFullPath(localPath));
        }

        public static Color GetColor(float newR, float newG, float newB)
        {
            return (new Color(newR / 255f, newG / 255f, newB / 255f ));
        }

        public static void DrawValue<T>(string title, T value, GUIStyle titleStyle = null, bool readOnly = true, params GUILayoutOption[] options)
        {
            GUIStyle newTitleStyle;
            GUIStyle contentStyle = new GUIStyle(EditorStyles.label);
            if (titleStyle != null)
                newTitleStyle = new GUIStyle(titleStyle);
            else
                newTitleStyle = new GUIStyle(EditorStyles.boldLabel);

            SetStyleTextColor(newTitleStyle, Color.white);
            SetStyleTextColor(contentStyle, Color.white);


            EditorGUILayout.BeginHorizontal(options);

            if (!string.IsNullOrEmpty(title))
                EditorGUILayout.PrefixLabel(title, newTitleStyle);

            using (new EditorGUI.DisabledScope(readOnly == true))
            {
                if (value is string stringValue)
                    EditorGUILayout.TextField(stringValue, contentStyle);
                else if (value is Object objectValue)
                    EditorGUILayout.ObjectField(objectValue, typeof(T), allowSceneObjects: false);
                else if (value is Vector3Int vector3Value)
                {
                    EditorGUILayout.IntField(vector3Value.x, GUILayout.MaxWidth(20));
                    EditorGUILayout.IntField(vector3Value.y, GUILayout.MaxWidth(20));
                    EditorGUILayout.IntField(vector3Value.z, GUILayout.MaxWidth(20));
                }
                else if (value is SerializedProperty propertyValue)
                    EditorGUILayout.PropertyField(propertyValue);

            }

            EditorGUILayout.EndHorizontal();

        }

        public static void SetStyleTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
        }

        public static GUIStyle CreateStyle(bool enableRichText, int fontSize = -1)
        {
            GUIStyle style = new GUIStyle();
            style.richText = enableRichText;

            if (fontSize != -1)
                style.fontSize = fontSize;
            return (style);
        }

        public static GUIStyle CreateStyle(bool enableRichText, Color backgroundColor, int fontSize = -1)
        {
            GUIStyle style = CreateStyle(enableRichText, fontSize);
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, backgroundColor);
            texture.Apply();
            style.normal.background = texture;
            return (style);
        }

        public static Color GetAlternatingColor(Color firstColor, Color secondColor, int arrayIndex)
        {
            if (arrayIndex % 2 == 0)
                return (firstColor);
            else
                return (secondColor);
        }

        public static GUIStyle GetAlternatingStyle(GUIStyle firstStyle, GUIStyle secondStyle, int collectionIndex)
        {
            if (collectionIndex % 2 == 0)
                return (firstStyle);
            return (secondStyle);
        }

        public static GUIStyle GetNewStyle(bool enableRichText = true, int fontSize = -1)
        {
            GUIStyle newStyle = new GUIStyle();
            newStyle.richText = enableRichText;
            newStyle.alignment = TextAnchor.MiddleLeft;

            if (fontSize != -1)
                newStyle.fontSize = fontSize;

            return newStyle;
        }

        public static GUIStyle GetNewStyle(Color backgroundColor, bool enableRichText = true, int fontSize = -1)
        {
            GUIStyle newStyle = GetNewStyle(enableRichText, fontSize);
            return newStyle.Colorize(backgroundColor);
        }

        public static List<T> FindAssets<T>(params string[] directories) where T : UnityEngine.Object
        {
            List<T> returnList = new List<T>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, directories))
                returnList.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            return (returnList);
        }

        public static List<SerializedProperty> FindSerializedProperties(Object nonSerializedObject)
        {
            return (FindSerializedProperties(new SerializedObject(nonSerializedObject)));
        }

        public static List<SerializedProperty> FindSerializedProperties(SerializedObject serializedObject)
        {
            return (FindSerializedProperties(serializedObject.GetIterator()));
        }

        public static List<SerializedProperty> FindSerializedProperties(SerializedProperty serializedProperty)
        {
            List<SerializedProperty> returnList = new List<SerializedProperty> { };
            if (serializedProperty.NextVisible(true))
            {
                do
                    if (!returnList.Contains(serializedProperty))
                        returnList.Add(serializedProperty.Copy());
                while (serializedProperty.NextVisible(false));
            }
            return (returnList);
        }

    }
}
