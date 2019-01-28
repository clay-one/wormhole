$buildFolder = (Get-Item -Path "./" -Verbose).FullName
$slnFolder = Join-Path $buildFolder "../"
Write-Host "build folder path:"
Write-Host $buildFolder
$testProject = Join-Path $slnFolder "tests/Wormhole.Tests/Wormhole.Tests.csproj"
Set-Location $slnFolder
dotnet test $testProject
