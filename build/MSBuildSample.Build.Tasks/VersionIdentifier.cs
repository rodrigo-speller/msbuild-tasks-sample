using System;

namespace MSBuildSample.Build.Tasks
{
    internal class VersionIdentifier
    {
        public VersionIdentifier(Version version, string identifier, bool isDirty)
        {
            Version = version
                ?? throw new ArgumentNullException(nameof(version));
                
            Identifier = identifier
                ?? throw new ArgumentNullException(nameof(identifier));

            IsDirty = isDirty;
        }

        public Version Version { get; }
        public string Identifier { get; }
        public bool IsDirty { get; }
    }
}