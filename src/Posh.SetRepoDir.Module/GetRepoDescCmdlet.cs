using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Posh.SetRepoDir.Module;

[Cmdlet(VerbsCommon.Get, "RepoDesc")]
public class GetRepoDescCmdlet : PSCmdlet
{
    [Parameter(Mandatory = false, Position = 0)]
    [ValidateSet(typeof(ValidFileNameGenerator))]
    public string? DirectoryName { get; set; }

    protected override void ProcessRecord()
    {
        var validDirs = ValidFileNameGenerator.ValidDirs;
        var root= ValidFileNameGenerator.PrimaryRoot;
        if (!validDirs.Any())
        {
            WriteWarning($"No valid dirs found. Roots searched: {root ?? "none"}");
        };

        var dirs = DirectoryName is not null 
            // Will really just be a single entry but DX is nicer :)
            ? validDirs
                .Where(k => k.Key == DirectoryName)
                .ToDictionary(k => k.Key,v => v.Value) 
            : validDirs;

        foreach (var (name,path) in dirs)
        {
            Console.WriteLine($"Repo: {name}");
            var hash = 0;
            foreach (var l in GetDirDisplay(path).Take(20))
            {
                // Try and smart read just the first markdown heading section.
                if (l.StartsWith('#')) hash++;
                if (hash >= 2) break;
                Console.WriteLine(l);
            }
                
        }
    }

    private static readonly string[] _readmeNames = { "README.md", "readme.md", "README", "readme" };
    private IEnumerable<string> GetDirDisplay(string dir)
    {

        var readmePath = _readmeNames
            .Select(n => Path.Combine(dir, n))
            .FirstOrDefault(File.Exists);

        return readmePath is null ? new List<string>(){"[No README found]"} : File.ReadLines(readmePath);
    }
}