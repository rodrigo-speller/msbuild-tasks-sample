using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Build.Framework;

namespace MSBuildSample.Build.Tasks
{
    internal static class Extensions
    {
        public static void LogMessageEvent(this IBuildEngine engine, string message)
            => engine.LogMessageEvent(new BuildMessageEventArgs(message, null, null, MessageImportance.Normal));

        public static void LogMessageEvent(this IBuildEngine engine, string message, MessageImportance importance)
            => engine.LogMessageEvent(new BuildMessageEventArgs(message, null, null, importance));

        public static void LogWarningEvent(this IBuildEngine engine, string message)
            => engine.LogWarningEvent(new BuildWarningEventArgs(null, null, null, 0, 0, 0, 0, message, null, null));

        public static void LogErrorEvent(this IBuildEngine engine, string message)
            => engine.LogErrorEvent(new BuildErrorEventArgs(null, null, null, 0, 0, 0, 0, message, null, null));

        public static VersionIdentifier GetVersion(this IRepository repository)
        {
            var head = repository.Head;

            if (head == null)
                return null;

            (System.Version Version, string Identifier)? version = null;
            if (head.GetType() == typeof(Branch))
                version = head.GetVersion();

            if (version == null)
            {
                var commit = head.Tip;

                version = repository.Tags
                    .Where(x => x.Target == commit)
                    .GetVersion();
            }

            if (version == null)
            {
                var commit = head.Tip;

                version = repository.Branches
                    .Where(x => x.Tip == commit)
                    .GetVersion();
            }

            if (version == null)
                return null;

            var status = repository.RetrieveStatus();
            return new VersionIdentifier(
                version.Value.Version,
                version.Value.Identifier,
                status.IsDirty
            );
        }

        public static (System.Version Version, string Identifier)? GetVersion(this Branch branch)
        {
            var name = branch.FriendlyName;
            var match = GitVersion.BranchPattern.Match(name);

            if (!match.Success)
                return null;

            var identifier = branch.Tip.Sha;

            var suffix = match.Groups["suffix"];
            if (suffix.Success && !string.IsNullOrEmpty(suffix.Value))
                identifier = $"{suffix.Value}-{identifier}";
            
            return (
                new System.Version(match.Groups["version"].Value),
                identifier
            );
        }

        public static (System.Version Version, string Identifier)? GetVersion(this Tag tag)
        {
            var name = tag.FriendlyName;
            var match = GitVersion.TagPattern.Match(name);

            if (!match.Success)
                return null;

            var identifier = tag.Target.Sha;

            var suffix = match.Groups["suffix"];
            if (suffix.Success && !string.IsNullOrEmpty(suffix.Value))
                identifier = $"{suffix.Value}-{identifier}";
            
            return (
                new System.Version(match.Groups["version"].Value),
                identifier
            );
        }

        public static (System.Version Version, string Identifier)? GetVersion(this IEnumerable<Tag> tags)
        {
            return tags
                .Select(x => x.GetVersion())
                .Where(x => x.HasValue)
                .OrderByDescending(x => x.Value.Version)
                .FirstOrDefault();
        }

        public static (System.Version Version, string Identifier)? GetVersion(this IEnumerable<Branch> branches)
        {
            return branches
                .Select(x => x.GetVersion())
                .Where(x => x.HasValue)
                .OrderByDescending(x => x.Value.Version)
                .FirstOrDefault();
        }
    }
}