param(
    [switch] $ReInstall,
    [switch] $Confirm
)

$module = "Set-RepoDir"
$docsDir = [Environment]::GetFolderPath("MyDocuments")
$outPath = "$docsDir\PowerShell\Modules\Set-RepoDir"
$binOutPath = "$outPath\bin"

if(($ReInstall) -and (!($Confirm))){
    $title    = 'Set-RepoDir re-install'
    $question = 'This process will close your powershell session, are you sure you want to proceed?'
    $choices  = '&Yes', '&No'

    $decision = $Host.UI.PromptForChoice($title, $question, $choices, 1)
    if ($decision -ne 0) {
        return
    }
}

if((!($ReInstall)) -and (Test-Path "$binOutPath/$module.dll")){
    Write-Host "You already have a copy of $module installed, we won't be able to automatically move the dll."
    $path = Get-Item out/bin/Posh.SetRepoDir.Module.dll
    Write-host "You can still manually perform the following move: `n$path -> $binOutPath\$module.dll"
}

./build.ps1

if (!(Test-Path $outPath)){
    New-Item -Type Directory $outPath | Out-Null
}

if (!(Test-Path $binOutPath)){
    New-Item -Type Directory $binOutPath | Out-Null
}

Push-Location $outPath
$manifestSplat = @{
    Path              = "$module.psd1"
    Author            = 'Michael Kelly'
    Company           = "None"
    NestedModules     = @("bin\$module.dll")
    RootModule        = "$module.psm1"
    FunctionsToExport = @('Set-RepoDir','Get-RepoDesc')
}
New-ModuleManifest @manifestSplat
Pop-Location

Set-Content -Value '' -Path "$outPath\$module.psm1"

if($ReInstall){
    $path = Get-Item out/bin/Posh.SetRepoDir.Module.dll
    $cmd = "(%systemroot%\System32\timeout.exe /t 2 /nobreak)>nul && copy $path $binOutPath\$module.dll && exit"
    Start-Process cmd -Argument ('/k "{0}"' -f $cmd) #-WindowStyle Hidden
    Get-Process pwsh | Stop-Process
}else{
    Copy-Item out/bin/Posh.SetRepoDir.Module.dll "$binOutPath\$module.dll" -Force -ErrorAction Ignore | Out-Null   
}


