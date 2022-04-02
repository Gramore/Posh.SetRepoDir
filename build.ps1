$outPath = "out\bin"
if (!(Test-Path $outPath)){
    New-Item -Type Directory $outPath | Out-Null
}
$Error = $null
dotnet publish -p:Configuration=Release -o $outPath -noLogo -t:Clean | Out-Null
if($null -ne $Error){
    Write-Error "Build failed"
    Write-Error $Error
    exit 1
}