$module = "Set-RepoDir"

./build.ps1

$docsDir = [Environment]::GetFolderPath("MyDocuments")
$outPath = "$docsDir\PowerShell\Modules\Set-RepoDir"
if (!(Test-Path $outPath)){
    New-Item -Type Directory $outPath | Out-Null
}

$binOutPath = "$outPath/bin"
if (!(Test-Path $binOutPath)){
    New-Item -Type Directory $binOutPath | Out-Null
}

Copy-Item out/bin/Posh.SetRepoDir.Module.dll "$binOutPath/$module.dll" -Force
Set-Content -Value '' -Path "$outPath/$module.psm1"

Push-Location $outPath
$manifestSplat = @{
    Path              = "$module.psd1"
    Author            = 'Michael Kelly'
    Company           = "None"
    NestedModules     = @("bin/$module.dll")
    RootModule        = "$module.psm1"
    FunctionsToExport = @('Set-RepoDir')
}
New-ModuleManifest @manifestSplat
Pop-Location

