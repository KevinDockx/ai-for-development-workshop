param(
    [string]$Configuration = "Release",
    [string]$Output = "artifacts"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host "Restoring..."
dotnet restore (Join-Path $root "ClaimHub.sln")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Publishing web project..."
$web = Join-Path $root "src/ClaimHub.Web/ClaimHub.Web.csproj"
dotnet publish $web -c $Configuration -o (Join-Path $root "$Output/web") --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Running tests..."
dotnet test (Join-Path $root "ClaimHub.sln") -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Build finished: $Output"
