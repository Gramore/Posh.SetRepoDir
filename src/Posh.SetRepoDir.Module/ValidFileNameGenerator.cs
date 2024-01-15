using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Posh.SetRepoDir.Module;

public class ValidFileNameGenerator : IValidateSetValuesGenerator
{
    private static readonly Dictionary<string, string> _aliases;
    public static readonly string? PrimaryRoot;
    public static readonly Dictionary<string, string> ValidDirs;
    
    static ValidFileNameGenerator()
    {
        var rootDirs = Environment.GetEnvironmentVariable("POSH_GitRootDirs")?.Split(' ');
        if (rootDirs is null || !rootDirs.Any())
        {
            ValidDirs = new Dictionary<string, string>();
            _aliases = new Dictionary<string, string>();
            return;
        }

        PrimaryRoot = rootDirs.First();

        var ignoreDirs = Environment.GetEnvironmentVariable("POSH_GitIgnoreDirs")?.Split(' ') ??
                         Array.Empty<string>();
                             
        var aliases = Environment.GetEnvironmentVariable("POSH_GitAliases")?.Split(' ') ??
                      Array.Empty<string>();
            
        _aliases = PopulateAliases(aliases);
            
        ValidDirs = GetValidDirs(rootDirs,ignoreDirs);
    }
    
    public string[]? GetValidValues() 
        => ValidDirs?.Keys.ToArray();
    
    private static Dictionary<string, string> GetValidDirs(IEnumerable<string> rootDirs, string[] ignoreDirs)
    {
        var validDirs =  rootDirs
            .SelectMany(Directory.GetDirectories)
            .Select(directory => (name: directory.Split('\\').TakeLast(1).Single(), directory))
            .Where(d => !ignoreDirs.Contains(d.name))
            .ToDictionary(d => d.name, d => d.directory);
            
        var aliases = _aliases.Where(a => validDirs.ContainsKey(a.Key))
            .Select(a => (ignoreDir: validDirs[a.Key], alias: a))
            .ToDictionary(k => k.alias.Value, k => k.ignoreDir);

        foreach (var (key, value) in aliases)
            _ = validDirs.TryAdd(key, value);

        return validDirs;
    }
        
    private static Dictionary<string, string> PopulateAliases(IEnumerable<string> aliases)
    {
        return aliases
            .Select(ToPair)
            .ToDictionary(d => d.Key, d => d.Value);
    }
    
    private static (string Key, string Value) ToPair(string alias)
    {
        var parts = alias.Trim().Split(":");
        return parts.Length != 2 ? (string.Empty, string.Empty) : (parts[0], parts[1]);
    }
}


