# create-release.ps1
# Creates a new GitHub release and triggers the build workflow.
# Usage: .\scripts\create-release.ps1 -Tag v0.8.0 -Notes "What's new in this release"

param(
    [Parameter(Mandatory=$true)]
    [string]$Tag,

    [Parameter(Mandatory=$false)]
    [string]$Notes = "Release $Tag"
)

# Ensure gh CLI is available
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Error "GitHub CLI (gh) is not installed. Download from https://cli.github.com/"
    exit 1
}

# Ensure we are on main and up to date
$branch = git rev-parse --abbrev-ref HEAD
if ($branch -ne "main") {
    Write-Error "You must be on the main branch to create a release. Currently on: $branch"
    exit 1
}

git fetch origin
$localHash  = git rev-parse HEAD
$remoteHash = git rev-parse origin/main
if ($localHash -ne $remoteHash) {
    Write-Error "Local main is not in sync with origin/main. Run: git pull origin main"
    exit 1
}

Write-Host "Creating release $Tag..." -ForegroundColor Cyan

# Create the GitHub release — this triggers release.yml automatically
gh release create $Tag `
    --title $Tag `
    --notes $Notes `
    --latest

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create release $Tag"
    exit 1
}

Write-Host ""
Write-Host "Release $Tag created." -ForegroundColor Green
Write-Host "The build workflow is now running on GitHub Actions." -ForegroundColor Green
Write-Host "Monitor progress at:" -ForegroundColor Cyan
gh browse --no-browser 2>$null
Write-Host "  https://github.com/mm5agm/FTdx101_WebApp/actions" -ForegroundColor Yellow
Write-Host "  https://github.com/mm5agm/FTdx101_WebApp/releases" -ForegroundColor Yellow
