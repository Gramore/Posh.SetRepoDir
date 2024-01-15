using System.Linq;
using System.Management.Automation;

namespace Posh.SetRepoDir.Module
{
    [Cmdlet(VerbsCommon.Set, "RepoDir")]
    public class SetRepoDirCmdlet : PSCmdlet
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

            var location = DirectoryName is not null ? validDirs[DirectoryName] : root;
            
            SessionState.Path.SetLocation(location);
        }
    }
}
