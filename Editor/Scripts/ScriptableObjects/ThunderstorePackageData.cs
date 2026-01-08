using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public class ThunderstorePackageData : PackageData
    {
        protected override void PopulateManifestData()
        {
            Name = SeekText("name");
            Author = SeekText("namespace");
            Description = SeekText("description");
            LatestVersionName = SeekText("version_number");
            LatestVersion = ParseVersion(LatestVersionName);
        }

        protected override void PopulateInstallData()
        {
            throw new System.NotImplementedException();
        }

        public override bool ValidateLink(string link, out string correctedLink)
        {
            string skippedUrl = link.Substring(link.IndexOf("/p/") + 3);
            string projectNamespace = skippedUrl.Replace(skippedUrl.Substring(skippedUrl.IndexOf("/")), string.Empty);
            string projectName = skippedUrl.Substring(skippedUrl.IndexOf("/") + 1);
            projectName = projectName.Replace("/", string.Empty);
            correctedLink = "https://thunderstore.io/api/experimental/package/" + projectNamespace + "/" + projectName + "/";

            return (true);
        }

        protected override string GetLatestPackageURL => "https://thunderstore.io/package/" + Author + "/" + Name + "/";
        protected override string GetLatestReleaseURL => "https://thunderstore.io/package/download/" + Author + "/" + Name + "/" + LatestVersionName + "/";
        protected override string GetIconURL => "https://gcdn.thunderstore.io/live/repository/icons/" + Author + "-" + Name + "-" + LatestVersionName + ".png";

        private string SeekText(string keyword) => downloadHandlerText.SeekText("\"" + keyword + "\":", ",").Replace("\"", string.Empty);
    }
}
