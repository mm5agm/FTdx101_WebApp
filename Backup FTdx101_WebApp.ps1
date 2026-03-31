# ---------------------------------------------
# FTdx101 WebApp Backup Script
# Creates a timestamped folder and copies all files except bin, obj, node_modules, publish, BenchmarkSuite1
# ---------------------------------------------
$timestamp = Get-Date
$formatted = $timestamp.ToString("dd MMMM yyyy     HH mm ss")

$sourceFolder = "C:\Users\colin\source\repos\FTdx101_WebApp"
$destRoot = "Z:\Code"
$destFolder = Join-Path $destRoot "FTdx101_WebApp $formatted"

Write-Host ""
Write-Host "Starting Backup From:" -ForegroundColor Yellow
Write-Host "  $sourceFolder"
Write-Host "To:"
Write-Host "  $destFolder"
Write-Host ""

# Check if destination folder already exists
if (Test-Path $destFolder) {
    Write-Host "Destination folder already exists:" -ForegroundColor Red
    Write-Host "  $destFolder"
    Write-Host "Backup aborted to avoid overwriting." -ForegroundColor Red
    Read-Host "Press ENTER to close"
    exit
}

# Create destination folder
New-Item -ItemType Directory -Path $destFolder -Force | Out-Null

# Excluded folders
$exclude = @("bin", "obj", "node_modules", "BenchmarkSuite1")

Get-ChildItem -Path $sourceFolder -Recurse -File | Where-Object {
    $relative = $_.FullName.Substring($sourceFolder.Length).TrimStart('\')
    $parts = $relative.Split('\')[0]
    $parts -notin $exclude
} | ForEach-Object {

    $relativePath = $_.FullName.Substring($sourceFolder.Length).TrimStart('\')
    $targetPath = Join-Path $destFolder $relativePath

    $targetDir = Split-Path $targetPath
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }

    Write-Host "Copying: $relativePath"
    Copy-Item $_.FullName -Destination $targetPath -Force
}

Write-Host ""
Write-Host "Backup Complete!" -ForegroundColor Green
Write-Host "Files copied from:"
Write-Host "  $sourceFolder"
Write-Host "To:"
Write-Host "  $destFolder"
Write-Host ""

Read-Host "Press ENTER to close"
