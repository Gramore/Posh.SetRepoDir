using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Posh.SetRepoDir.Module;

public class ValidFileNameGenerator : IValidateSetValuesGenerator
{
    public static readonly string? PrimaryRoot;
    public static readonly Dictionary<string, string> ValidDirs;
    
    static ValidFileNameGenerator()
    {
        var rootDirs = Environment.GetEnvironmentVariable("POSH_GitRootDirs")?.Split(' ');
        if (rootDirs is null || !rootDirs.Any())
        {
            ValidDirs = new Dictionary<string, string>();
            return;
        }

        PrimaryRoot = rootDirs.First();

        var ignoreDirs = Environment.GetEnvironmentVariable("POSH_GitIgnoreDirs")?.Split(' ') ??
                         Array.Empty<string>();
                             
        var aliases = Environment.GetEnvironmentVariable("POSH_GitAliases")?.Split(' ') ??
                      Array.Empty<string>();
        
        var manualRepos = Environment.GetEnvironmentVariable("POSH_GitManualRepos")?.Split(' ') ??
                         Array.Empty<string>();
            
        var resolvedAliases = PopulateAliases(aliases);
            
        ValidDirs = GetValidDirs(rootDirs,ignoreDirs, manualRepos, resolvedAliases);
    }
    
    public string[]? GetValidValues() 
        => ValidDirs?.Keys.ToArray();
    
    private static Dictionary<string, string> GetValidDirs(
        IEnumerable<string> rootDirs,
        string[] ignoreDirs,
        string[] manualRepos, 
        List<Alias> resolvedAliases)
    {
        var validDirs =  rootDirs
            .SelectMany(Directory.GetDirectories)
            .Select(directory => (name: directory.Split('\\').TakeLast(1).Single(), directory))
            .Where(d => !ignoreDirs.Contains(d.name))
            .ToDictionary(d => d.name, d => d.directory);
        
        foreach (var repo in manualRepos.Select(path =>
                 {
                     var name = new DirectoryInfo(path).Name;;
                     return (name, path);
                 }))
            _ = validDirs.TryAdd(repo.name, repo.path);

        var aliases = resolvedAliases
            .Where(a => validDirs.ContainsKey(a.Source))
            .Select(a => (alias: a,ignoreDir: validDirs[a.Source]));

        foreach (var (a, dir) in aliases)
        {
            if (a.Overwrite)
                validDirs.Remove(a.Source);
            _ = validDirs.TryAdd(a.Name, dir);
        }

        return validDirs;
    }
        
    private static List<Alias> PopulateAliases(IEnumerable<string> aliases) 
        => aliases
            .Select(a =>
            {
                var parts = a.Trim().Split(":");
                return parts switch
                {
                    [{ } p1, { } p2] =>  new Alias(p1,p2,false),
                    [{ } p1, { } p2, {} p3] =>  new Alias(p1,p2,p3 is "overwrite"),
                    _ => new Alias(string.Empty,string.Empty,false)

                };
            })
            .ToList();

    private record struct Alias(string Source,string Name, bool Overwrite);
}


