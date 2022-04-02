# Set-RepoDir
A small utility to allow quick navigation to repo directories

# Installation
Add the following to your powershell profile with relevant values:
```posh
#Set-RepoDir config
$Env:POSH_GitRootDirs = @("C:\YOUR_REPO_PATH")
$Env:POSH_GitIgnoreDirs = @()
```

Run the following:
```posh
Set-ExecutionPolicy Bypass -Scope Process -Force;.\build.ps1
```

Please note that the install script will not work if you have previously installed the module as your powershell session will keep a lock on the DLL