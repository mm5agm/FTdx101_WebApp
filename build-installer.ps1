# =============================================================================
# build-installer.ps1  -  FTdx101 WebApp installer build script
# =============================================================================
# Run from the solution root directory:
#   .\build-installer.ps1
#
# Requires NSIS installed (default path below).  Download from:
#   https://nsis.sourceforge.io
# =============================================================================

$Version    = "0.5.4"
$PublishDir = "publish"

# Support both local install path and Chocolatey/CI path
$NsisMake = @(
    "C:\Program Files (x86)\NSIS\makensis.exe",
    "C:\ProgramData\chocolatey\bin\makensis.exe",
    (Get-Command makensis.exe -ErrorAction SilentlyContinue)?.Source
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $NsisMake) {
    Write-Host "ERROR: makensis.exe not found. Install NSIS from https://nsis.sourceforge.io" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== FTdx101 WebApp - Build Installer v$Version ===" -ForegroundColor Cyan
Write-Host ""

# ---------------------------------------------------------------------------
# Step 1 - Publish (x86, framework-dependent)
# ---------------------------------------------------------------------------
Write-Host "Step 1: Publishing app (x86, framework-dependent)..." -ForegroundColor Yellow

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
    Write-Host "  Cleaned existing publish folder" -ForegroundColor Gray
}

dotnet publish FTdx101_WebApp.csproj `
    --configuration Release `
    --runtime win-x86 `
    --self-contained false `
    --output $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: dotnet publish failed (exit code $LASTEXITCODE)" -ForegroundColor Red
    exit 1
}

Write-Host "  Publish OK" -ForegroundColor Green

# ---------------------------------------------------------------------------
# Step 2 - Remove files not needed / not safe to ship
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Step 2: Removing dev/debug files..." -ForegroundColor Yellow

$itemsToRemove = @(
    "$PublishDir\*.pdb",                 # Debug symbols
    "$PublishDir\libman.json",           # Library manager config (dev only)
    "$PublishDir\web.config",            # IIS config (not used by WinExe)
    "$PublishDir\radio_state.json",      # User's saved radio state - never ship
    "$PublishDir\appsettings.user.json"  # User's saved COM port / settings - never ship
)

foreach ($pattern in $itemsToRemove) {
    $matches = Resolve-Path $pattern -ErrorAction SilentlyContinue
    if ($matches) {
        foreach ($item in $matches) {
            Remove-Item $item -Force
            Write-Host "  Removed: $(Split-Path $item -Leaf)" -ForegroundColor Gray
        }
    }
}

# ---------------------------------------------------------------------------
# Step 3 - Build NSIS installer
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Step 3: Building NSIS installer..." -ForegroundColor Yellow
Write-Host "  Using: $NsisMake" -ForegroundColor Gray

if (-not (Test-Path $NsisMake)) {
    Write-Host ""
    Write-Host "ERROR: NSIS not found at:" -ForegroundColor Red
    Write-Host "  $NsisMake" -ForegroundColor Red
    Write-Host "Download from https://nsis.sourceforge.io" -ForegroundColor Yellow
    exit 1
}

& $NsisMake installer.nsi

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: NSIS build failed (exit code $LASTEXITCODE)" -ForegroundColor Red
    exit 1
}

# Normalise the output filename - NSIS may have written a versioned name
$found = Get-ChildItem -Filter "*Setup*.exe" | Select-Object -First 1
if (-not $found) {
    Write-Host "ERROR: No *Setup*.exe found after NSIS ran" -ForegroundColor Red
    exit 1
}
if ($found.Name -ne "FTdx101_WebApp_Setup.exe") {
    Rename-Item $found.FullName "FTdx101_WebApp_Setup.exe"
    Write-Host "  Renamed $($found.Name) -> FTdx101_WebApp_Setup.exe" -ForegroundColor Gray
}

# ---------------------------------------------------------------------------
# Done
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host "  Installer: FTdx101_WebApp_Setup.exe" -ForegroundColor Green
Write-Host ""
