<#
.SYNOPSIS
    Renames FTdx101MP_WebApp to FTdx101_WebApp
.DESCRIPTION
    Renames project files, folders, namespaces, and updates all references.
    Supports both FT-dx101MP (dual receiver) and FT-dx101D (single receiver).
#>

$oldName = "FTdx101MP_WebApp"
$newName = "FTdx101_WebApp"
$projectRoot = Get-Location

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  FT-dx101 Project Rename Tool" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Old Name: $oldName" -ForegroundColor Yellow
Write-Host "New Name: $newName" -ForegroundColor Green
Write-Host "Location: $projectRoot`n" -ForegroundColor Gray

# Step 1: Update all code files
Write-Host "[1/6] Updating code files..." -ForegroundColor Cyan
$filesUpdated = 0

Get-ChildItem -Recurse -Include *.cs,*.cshtml,*.csproj,*.json,*.sln,*.md | 
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\|\\.git\\" } |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
        if ($content -and $content -match $oldName) {
            Write-Host "  ? $($_.Name)" -ForegroundColor Green
            $content -replace [regex]::Escape($oldName), $newName | Set-Content $_.FullName -NoNewline
            $filesUpdated++
        }
    }

Write-Host "  Updated $filesUpdated files`n" -ForegroundColor Green

# Step 2: Rename project file
Write-Host "[2/6] Renaming project file..." -ForegroundColor Cyan
if (Test-Path "$oldName.csproj") {
    Rename-Item "$oldName.csproj" "$newName.csproj"
    Write-Host "  ? $newName.csproj`n" -ForegroundColor Green
} else {
    Write-Host "  ? Project file not found (may already be renamed)`n" -ForegroundColor Yellow
}

# Step 3: Rename solution file
Write-Host "[3/6] Renaming solution file..." -ForegroundColor Cyan
if (Test-Path "$oldName.sln") {
    Rename-Item "$oldName.sln" "$newName.sln"
    Write-Host "  ? $newName.sln`n" -ForegroundColor Green
} else {
    Write-Host "  ? Solution file not found (may already be renamed)`n" -ForegroundColor Yellow
}

# Step 4: Move up and rename folder
Write-Host "[4/6] Renaming project folder..." -ForegroundColor Cyan
Set-Location ..
$parentDir = Get-Location
if (Test-Path $oldName) {
    Rename-Item $oldName $newName
    Write-Host "  ? Renamed to: $parentDir\$newName`n" -ForegroundColor Green
    Set-Location $newName
} else {
    Write-Host "  ? Already at correct name`n" -ForegroundColor Yellow
    Set-Location $newName
}

# Step 5: Update UI text to support both models
Write-Host "[5/6] Updating UI for dual model support..." -ForegroundColor Cyan
$indexPath = "Pages\Index.cshtml"
if (Test-Path $indexPath) {
    $content = Get-Content $indexPath -Raw
    $content = $content -replace "FT-dx101MP Control", "FT-dx101 Series Control"
    $content = $content -replace "FT-dx101MP Dual Receiver Control", "FT-dx101 Series Web Control"
    $content = $content -replace "Monitor and control both receivers simultaneously", "Supports FT-dx101MP (dual receiver) and FT-dx101D (single receiver)"
    $content = $content -replace "Connected to FT-dx101MP", "Connected to FT-dx101"
    $content | Set-Content $indexPath -NoNewline
    Write-Host "  ? Updated Index.cshtml for both models`n" -ForegroundColor Green
} else {
    Write-Host "  ? Index.cshtml not found`n" -ForegroundColor Yellow
}

# Step 6: Update Git
Write-Host "[6/6] Updating Git repository..." -ForegroundColor Cyan
try {
    git add -A
    git status --short | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    
    $commitMsg = @"
refactor: Rename project to FTdx101_WebApp

- Supports both FT-dx101MP (dual receiver) and FT-dx101D (single receiver)
- Updated namespaces from FTdx101MP_WebApp to FTdx101_WebApp
- Updated UI text to reflect broader model support
- Both radios use identical CAT command set
"@
    
    git commit -m $commitMsg
    Write-Host "`n  ? Git commit created`n" -ForegroundColor Green
    
    Write-Host "  Ready to push? Run: " -ForegroundColor Yellow -NoNewline
    Write-Host "git push origin main`n" -ForegroundColor White
} catch {
    Write-Host "  ? Git update skipped (not in a git repo or no changes)`n" -ForegroundColor Yellow
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ? Rename Complete!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Project renamed to: " -ForegroundColor White -NoNewline
Write-Host "$newName`n" -ForegroundColor Green

Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Reopen Visual Studio" -ForegroundColor White
Write-Host "  2. Open: $parentDir\$newName\$newName.sln" -ForegroundColor White
Write-Host "  3. Build and test the project" -ForegroundColor White
Write-Host "  4. Push to GitHub: git push origin main`n" -ForegroundColor White

Write-Host "Optional: Rename GitHub repository" -ForegroundColor Cyan
Write-Host "  • Go to: https://github.com/mm5agm/FTdx101MP_WebApp/settings" -ForegroundColor White
Write-Host "  • Change name to: FTdx101_WebApp" -ForegroundColor White
Write-Host "  • Update remote: git remote set-url origin https://github.com/mm5agm/FTdx101_WebApp`n" -ForegroundColor White

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")