using PlasticGui.WorkspaceWindow.Sync;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAmBatby.PackageInjector
{
    public abstract class Source
    {
        public string DisplayName => GetDisplayName();

        protected abstract string GetDisplayName();

        protected abstract bool ValidateLink(string newLink);

        protected abstract IEnumerator RequestPackageManifest();

        protected abstract IEnumerator RequestPackage();
    }

    public class ThunderstoreSource : Source
    {
        protected override string GetDisplayName() => "Thunderstore";

        protected override IEnumerator RequestPackage()
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerator RequestPackageManifest()
        {
            throw new System.NotImplementedException();
        }

        protected override bool ValidateLink(string newLink)
        {
            throw new System.NotImplementedException();
        }
    }
}
