$buildFolder = (Get-Item -Path "./" -Verbose).FullName
Write-Host "build folder path:"
Write-Host $buildFolder
$testProject = Join-Path $buildFolder "tests/Wormhole.Tests/Wormhole.Tests.csproj"
Set-Location $buildFolder
dotnet test $testProject