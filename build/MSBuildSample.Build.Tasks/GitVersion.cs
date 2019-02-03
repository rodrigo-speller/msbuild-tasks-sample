using LibGit2Sharp;
using Microsoft.Build.Framework;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MSBuildSample.Build.Tasks
{
    public class GitVersion
        : ITask
    {
        private const RegexOptions StrictPatternOptions =
            RegexOptions.Compiled
            | RegexOptions.CultureInvariant
            | RegexOptions.ExplicitCapture
            | RegexOptions.Singleline;

        internal static readonly Regex BranchPattern = new Regex(
            @"(^|/)(hotfix|release)/(?<version>\d+(\.\d+){0,3})(-(?<suffix>[^/]*))?$",
            StrictPatternOptions
        );

        internal static readonly Regex TagPattern = new Regex(
            @"(^|/)(?<version>\d+(\.\d+){0,3})(-(?<suffix>[^/]*))?$",
            StrictPatternOptions
        );

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public string Configuration { get; set; }

        [Output]
        public string Version { get; set; }

        [Output]
        public string Identifier { get; set; }

        [Output]
        public bool IsDirty { get; set; }

        public bool Execute()
        {
            var path = Directory.GetCurrentDirectory();
            path = Repository.Discover(path);

            if (path == null)
            {
                BuildEngine.LogWarningEvent("No Git repository found.");
                return false;
            }

            var repository = new Repository(path);

            BuildEngine.LogMessageEvent($"Git repository: {path}", MessageImportance.High);

            var versionInfo = repository.GetVersion();

            if (versionInfo == null)
            {
                if (string.IsNullOrWhiteSpace(Configuration) || Configuration == "Release")
                {
                    if (BuildEngine.ContinueOnError)
                        BuildEngine.LogWarningEvent("No Git version found.");
                    else
                        BuildEngine.LogErrorEvent("No Git version found.");
                }
                else
                    BuildEngine.LogMessageEvent("No Git version found.", MessageImportance.High);
                
                return false;
            }

            Version = versionInfo.Version.ToString();
            Identifier = versionInfo.Identifier;
            IsDirty = versionInfo.IsDirty;

            return true;
        }
    }
}
