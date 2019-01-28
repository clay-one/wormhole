# COMMON PATHS
$buildFolder = (Get-Item -Path "./" -Verbose).FullName
$slnFolder = Join-Path $buildFolder "../"
$outputFolder = Join-Path $buildFolder "outputs"
$webHostFolder = Join-Path $slnFolder "src/Wormhole.Api"

## CLEAR ######################################################################

Remove-Item $outputFolder -Force -Recurse -ErrorAction Ignore
New-Item -Path $outputFolder -ItemType Directory

## RESTORE NUGET PACKAGES #####################################################

Set-Location $slnFolder
dotnet restore

## BUILD SOLUTION #############################################################
dotnet publish --configuration Release --output (Join-Path $outputFolder "Host")

$hostOutputFolder = (Join-Path $outputFolder "Host")

Set-Location $outputFolder
