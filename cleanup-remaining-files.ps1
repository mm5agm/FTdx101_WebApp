<#
.SYNOPSIS
    Cleans up remaining FTdx101MP_WebApp files
.DESCRIPTION
    Renames .slnx, .csproj.user, and updates references inside them
#>

$oldName = "FTdx101MP_WebApp"
$newName = "FTdx101_WebApp"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Cleanup Remaining Files" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Update .slnx content
if (Test-Path "$oldName.slnx") {
    Write-Host "Updating $oldName.slnx content..." -ForegroundColor Cyan
    $content = Get-Content "$oldName.slnx" -Raw
    $content = $content -replace [regex]::Escape($oldName), $newName
    $content | Set-Content "$oldName.slnx" -NoNewline
    Write-Host "  ✓ Updated project reference inside .slnx`n" -ForegroundColor Green
}

# Step 2: Rename all remaining files
$filesRenamed = 0
Get-ChildItem -Path . -Filter "$oldName*" -File | ForEach-Object {
    $newFileName = $_.Name -replace [regex]::Escape($oldName), $newName
    Write-Host "Renaming: $($_.Name)" -ForegroundColor Yellow
    Write-Host "     To: $newFileName" -ForegroundColor Green
    Rename-Item -Path $_.FullName -NewName $newFileName
    $filesRenamed++
}

Write-Host "`n✓ Renamed $filesRenamed file(s)" -ForegroundColor Green

# Step 3: Update Git
Write-Host "`nUpdating Git..." -ForegroundColor Cyan
git add -A
git status --short | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }

$commitMessage = @"
refactor: Rename remaining user and solution files

- Renamed FTdx101MP_WebApp.slnx to FTdx101_WebApp.slnx
- Renamed FTdx101MP_WebApp.csproj.user to FTdx101_WebApp.csproj.user
- Updated project references inside .slnx file
"@

git commit -m $commitMessage

Write-Host "✓ Git commit created`n" -ForegroundColor Green

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Ready to push: " -ForegroundColor Yellow -NoNewline
Write-Host "git push origin main" -ForegroundColor White
Write-Host ""
