using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Posh.SetRepoDir.Module
{
    [Cmdlet(VerbsCommon.Set, "RepoDir")]
    public class SetRepoDirCmdletCommand : PSCmdlet
    {
        private static Dictionary<string, string>? _validDirs;
        public static string? PrimaryRoot { get; private set; }
        public static Dictionary<string, string>? ValidDirs => _validDirs ??= GetValidDirs();

        [Parameter(Mandatory = false, Position = 0)]
        [ValidateSet(typeof(ValidFileNameGenerator))]
        public string? DirectoryName { get; set; }

        protected override void BeginProcessing()
        {
            WriteVerbose($"Valid dirs: {ValidDirs?.Keys}");
        }

        protected override void ProcessRecord()
        {
            if (ValidDirs is null ) return;

            var location = DirectoryName is not null ? ValidDirs[DirectoryName] : PrimaryRoot!;
            
            SessionState.Path.SetLocation(location);
        }

        private static Dictionary<string, string>? GetValidDirs()
        {
            var rootDirs = Environment.GetEnvironmentVariable("POSH_GitRootDirs")?.Split(' ');
            if (rootDirs is null)
                return null;

            PrimaryRoot = rootDirs.First();

            var ignoreDirs = Environment.GetEnvironmentVariable("POSH_GitIgnoreDirs")?.Split(' ') ??
                             Array.Empty<string>();

            return rootDirs
                .SelectMany(Directory.GetDirectories)
                .Select(directory => (name: directory.Split('\\').TakeLast(1).Single(), directory))
                .Where(d => !ignoreDirs.Contains(d.name))
                .ToDictionary(d => d.name, d => d.directory);
        }
    }

    public class ValidFileNameGenerator : IValidateSetValuesGenerator
    {
        public string[]? GetValidValues() 
            => SetRepoDirCmdletCommand.ValidDirs?.Keys.ToArray();
    }
}
