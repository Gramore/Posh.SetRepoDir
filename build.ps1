$outPath = "out/bin"
if (!(Test-Path $outPath)){
    New-Item -Type Directory $outPath | Out-Null
}
dotnet publish -p:Configuration=Release -o $outPath -noLogo -t:Clean