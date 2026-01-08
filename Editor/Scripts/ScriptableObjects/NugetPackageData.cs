using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public class NugetPackageData : PackageData
    {
        protected override void PopulateManifestData()
        {
            string fullName = SeekText("title");
            string nameAndVersion = fullName.Substring(fullName.IndexOf(".") + 1);
            string version = nameAndVersion.Substring(nameAndVersion.LastIndexOf(" ") + 1);
            Author = fullName.Replace(nameAndVersion, string.Empty).Replace(".", string.Empty);
            string name = nameAndVersion.Replace(version, string.Empty);
            name = name.TrimEnd(' ');
            Name = name;
            LatestVersionName = version.TrimEnd('\"');
            Description = SeekText("description");
            LatestVersion = ParseVersion(LatestVersionName);
        }

        protected override void PopulateInstallData()
        {
            throw new System.NotImplementedException();
        }

        public override bool ValidateLink(string link, out string correctedLink)
        {
            correctedLink = link;
            return (true);
        }

        protected override string GetLatestPackageURL => SeekText("url").Replace("\"", string.Empty);
        protected override string GetLatestReleaseURL => "https://www.nuget.org/api/v2/package/" + Author + "." + Name + "/" + LatestVersionName;
        protected override string GetIconURL => "https://gcdn.thunderstore.io/live/repository/icons/" + Author + "-" + Name + "-" + LatestVersionName + ".png";

        private string SeekText(string keyword)
        {
            string initialResult = downloadHandlerText.SeekText("<meta property=\"og:" + keyword, " />");
            return (initialResult.SeekText("content=\"", " />"));
        }

    }
}
