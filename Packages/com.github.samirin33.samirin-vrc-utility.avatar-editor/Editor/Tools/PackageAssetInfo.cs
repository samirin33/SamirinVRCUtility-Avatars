using System;
using UnityEngine;

[Serializable]
public class PackageAssetInfo
{
    public string name;
    public string version;
    public string author;
    public string description;

    public UrlInfo[] urls;
    public ReleaseInfo[] releases;

    [Serializable]
    public class UrlInfo
    {
        public string urlDescription;
        public string url;
    }

    [Serializable]
    public class ReleaseInfo
    {
        public string version;
        public string releaseDate;
        public string[] releaseNotes;
    }
}
