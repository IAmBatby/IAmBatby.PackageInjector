using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public static class Extensions
    {
        public static Color SetAlpha(this Color color, float newAlpha, bool scaledValues = false)
        {
            Color scaledColor;
            Color nonScaledColor;
            if (color.r > 1 || color.g > 1 || color.b > 1)
            {
                scaledColor = new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
                nonScaledColor = color;
            }
            else
            {
                scaledColor = color;
                nonScaledColor = new Color(color.r * 255, color.g * 255, color.b * 255, color.a * 255);
            }

            if (scaledValues == true)
                return (new Color(scaledColor.r, scaledColor.g, scaledColor.b, newAlpha));
            else
                return (new Color(nonScaledColor.r, nonScaledColor.g, nonScaledColor.b, newAlpha));
        }

        public static string ToBold(this string input)
        {
            return new string("<b>" + input + "</b>");
        }

        public static string Colorize(this string input)
        {
            string hexColor = "#" + "FFFFFF";
            return new string("<color=" + hexColor + ">" + input + "</color>");
        }

        public static string Colorize(this string input, Color color)
        {
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
            return new string("<color=" + hexColor + ">" + input + "</color>");
        }

        public static string FirstToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            char[] chars = input.ToCharArray();
            if (char.IsLetter(chars[0]))
                chars[0] = char.ToUpper(chars[0]);
            return (new string(chars));
        }

        public static string ToItalic(this string str) => "<i>" + str + "</i>";

        public static GUIStyle Colorize(this GUIStyle input, Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            input.normal.background = texture;
            return input;
        }

        public static string SeekText(this string text, string searchTerm, string endIdentifier)
        {
            string returnText = text;
            if (returnText.Contains(searchTerm))
            {
                string skip = returnText.Substring(returnText.IndexOf(searchTerm) + searchTerm.Length);

                string result = skip.Replace(skip.Substring(skip.IndexOf(endIdentifier)), string.Empty);
                return (result);
            }
            Debug.LogError("Could Not Find Text With: " + searchTerm);
            return (returnText);
        }

        public static string ToFullPath(this string localUnityPath)
        {
            return (Utilities.GetFullPath(localUnityPath));
        }
    }
}
