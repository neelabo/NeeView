#pragma warning disable CA1822

using System;

namespace NeeView
{
    [WordNodeMember]
    public class EnvironmentAccessor
    {
        [WordNodeMember]
        public string NeeViewPath => Environment.AssemblyLocation;

        [WordNodeMember]
        public string UserSettingFilePath => SaveData.UserSettingFilePath;

        [WordNodeMember]
        public string PackageType => Environment.PackageType;

        [WordNodeMember]
        public string ReleaseType => Environment.ReleaseType;

        [WordNodeMember]
        public string Version => Environment.DisplayVersionShort;

        [WordNodeMember]
        public string ProductVersion => Environment.ProductVersion;

        [WordNodeMember]
        public string Revision => Environment.Revision;

        [WordNodeMember]
        public bool SelfContained => Environment.SelfContained;

        [WordNodeMember]
        public string OSVersion => Environment.OSVersion;

        [WordNodeMember]
        public string UserAgent => Environment.UserAgent;


        #region Obsolete

        [WordNodeMember]
        [Obsolete("no used"), Alternative(null, 45, ScriptErrorLevel.Warning)]
        public string DateVersion => "";

        #endregion Obsolete
    }
}
