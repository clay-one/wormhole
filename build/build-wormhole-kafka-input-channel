# COMMON PATHS
$buildFolder = (Get-Item -Path "./" -Verbose).FullName
$outputFolder = Join-Path $buildFolder "outputs"
$hostFolder = Join-Path $buildFolder "../src/Wormhole.InputChannels.Kafka.Consumer"
$hostOutputFolder = Join-Path $outputFolder "Host"

## CLEAR ######################################################################

Remove-Item $outputFolder -Force -Recurse -ErrorAction Ignore
New-Item -Path $outputFolder -ItemType Directory

## RESTORE NUGET PACKAGES #####################################################

Set-Location $hostFolder
dotnet restore

## BUILD SOLUTION #############################################################
 
 'publish source from ' + (get-location) + '> '
dotnet publish --configuration Release --output $hostOutputFolder


Set-Location $hostOutputFolder
