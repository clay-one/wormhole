# COMMON PATHS
$buildFolder = (Get-Item -Path "./" -Verbose).FullName
$outputFolder = Join-Path $buildFolder "outputs"
$webHostFolder = Join-Path $buildFolder "../src/Wormhole.Api"

## CLEAR ######################################################################

Remove-Item $outputFolder -Force -Recurse -ErrorAction Ignore
New-Item -Path $outputFolder -ItemType Directory

## RESTORE NUGET PACKAGES #####################################################

Set-Location $webHostFolder
dotnet restore

## BUILD SOLUTION #############################################################
 
 'publish source from ' + (get-location) + '> '
dotnet publish --configuration Release --output (Join-Path $outputFolder "Host")

$hostOutputFolder = (Join-Path $outputFolder "Host")

Set-Location $outputFolder
