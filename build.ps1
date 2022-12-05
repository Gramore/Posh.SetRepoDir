$outPath = "out\bin"
if (!(Test-Path $outPath)){
    New-Item -Type Directory $outPath | Out-Null
}
$Error = $null
dotnet clean -c Release -noLogo | Out-Null
dotnet publish -c Release -o $outPath -noLogo | Out-Null
if($null -ne $Error){
    Write-Error "Build failed"
    Write-Error $Error
    exit 1
}