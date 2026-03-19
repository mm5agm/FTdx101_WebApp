# ---------------------------------------------
# FTdx101 WebApp Recovery Script
# Restores the project from a backup folder
# ---------------------------------------------

# Ask for the backup folder path
$backupFolder = Read-Host "Enter the full path to your backup folder"

# Destination (your normal project location)
$destFolder = "C:\Users\colin\source\repos\FTdx101_WebApp"

Write-Host ""
Write-Host "Starting Recovery From:" -ForegroundColor Yellow
Write-Host "  $backupFolder"
Write-Host "To:"
Write-Host "  $destFolder"
Write-Host ""

# Create destination folder if missing
if (!(Test-Path $destFolder)) {
    Write-Host "Creating destination folder..."
    New-Item -ItemType Directory -Path $destFolder -Force | Out-Null
}

# Copy files one by one and show filenames
Get-ChildItem -Path $backupFolder -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($backupFolder.Length).TrimStart('\')
    $targetPath = Join-Path $destFolder $relativePath

    $targetDir = Split-Path $targetPath
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }

    Write-Host "Restoring: $relativePath"
    Copy-Item $_.FullName -Destination $targetPath -Force
}

Write-Host ""
Write-Host "Source files restored." -ForegroundColor Green
Write-Host ""

# Optional: restore .NET dependencies
if (Test-Path "$destFolder\*.csproj") {
    Write-Host "Running dotnet restore..."
    dotnet restore $destFolder
}

# Optional: restore npm dependencies if package.json exists
if (Test-Path "$destFolder\package.json") {
    Write-Host "Running npm install..."
    npm install --prefix $destFolder
}

Write-Host ""
Write-Host "Recovery Complete!" -ForegroundColor Green
Write-Host "Project restored to:"
Write-Host "  $destFolder"
Write-Host ""

Read-Host "Press ENTER to close"
