# COMMON PATHS
$buildFolder = (Get-Item -Path "./" -Verbose).FullName
$outputFolder = Join-Path $buildFolder "outputs"
$workerFolder = Join-Path $buildFolder "../src/Wormhole.Worker"
$workerOutputFolder = Join-Path $outputFolder "Worker"

## CLEAR ######################################################################

Remove-Item $outputFolder -Force -Recurse -ErrorAction Ignore
New-Item -Path $outputFolder -ItemType Directory

## RESTORE NUGET PACKAGES #####################################################

Set-Location $workerFolder
dotnet restore

## BUILD SOLUTION #############################################################
 
 'publish source from ' + (get-location) + '> '
dotnet publish --configuration Release --output $workerOutputFolder


Set-Location $workerOutputFolder
