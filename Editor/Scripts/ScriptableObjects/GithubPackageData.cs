using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public class GithubPackageData : PackageData
    {
        private string savedLink;
        protected override void PopulateManifestData()
        {
            Author = SeekText("login");
            Name = SeekText("url").SeekText("/" + Author + "/", "/");
            LatestVersionName = SeekText("tag_name");
            if (LatestVersionName.Contains("v"))
                LatestVersionName = LatestVersionName.Replace("v", string.Empty);
            LatestVersion = ParseVersion(LatestVersionName);
        }

        protected override void PopulateInstallData()
        {
            throw new System.NotImplementedException();
        }

        public override bool ValidateLink(string link, out string correctedLink)
        {
            string authorAndRepoName = link.Substring(link.IndexOf("github.com/"));
            authorAndRepoName = authorAndRepoName.Replace("github.com/", string.Empty);
            correctedLink = "https://api.github.com/repos/" + authorAndRepoName + "/releases/latest";
            savedLink = link;
            return (true);
        }

        protected override string GetLatestPackageURL => savedLink;
        protected override string GetLatestReleaseURL => GetBiggestLatestRelease();
        protected override string GetIconURL => "https://gcdn.thunderstore.io/live/repository/icons/" + Author + "-" + Name + "-" + LatestVersionName + ".png";

        private string SeekText(string keyword) => downloadHandlerText.SeekText("\"" + keyword + "\": ", ",").Replace("\"", string.Empty);

        private string GetBiggestLatestRelease()
        {
            Dictionary<string, string> releases = new Dictionary<string, string>();
            string inputString = downloadHandlerText;
            string seperator = "\"uploader\": {";
            string size = string.Empty;
            string url = string.Empty;
            while (inputString.Contains(seperator))
            {
                size = inputString.SeekText("\"size\": ", ",");
                url = inputString.SeekText("\"browser_download_url\": ", "}").Replace("\"", string.Empty);
                if (!releases.ContainsKey(size))
                    releases.Add(size, url);
                SkipString(ref inputString, seperator);
            }
            size = inputString.SeekText("\"size\": ", ",");
            url = inputString.SeekText("\"browser_download_url\": ", "}").Replace("\"", string.Empty);
            if (!releases.ContainsKey(size))
                releases.Add(size, url);

            int highestSize = -1;
            string biggestURL = string.Empty;

            foreach (KeyValuePair<string, string> kvp in releases)
                if (highestSize == -1 || int.Parse(kvp.Key) > highestSize)
                {
                    highestSize = int.Parse(kvp.Key);
                    biggestURL = kvp.Value;
                }

            return (biggestURL);


        }

        private void SkipString(ref string input, string seperator)
        {
            if (input.Contains(seperator))
            {
                input = input.Substring(input.IndexOf(seperator));
                input = input.Remove(0, seperator.Length);
            }
        }
    }
}
