# Build and create installer for FT-dx101 Web Control

Write-Host "🔨 Building FT-dx101 Web Control..." -ForegroundColor Cyan

# Step 1: Clean previous builds
Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean

# Step 2: Publish self-contained app
Write-Host "📦 Publishing self-contained application..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false

# Step 3: Check if Inno Setup is installed
$innoSetup = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $innoSetup) {
    Write-Host "🎨 Creating installer with Inno Setup..." -ForegroundColor Yellow
    & $innoSetup installer.iss
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Installer created successfully!" -ForegroundColor Green
        Write-Host "📍 Location: installer\FTdx101WebControl-Setup-v0.8.exe" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Installer creation failed!" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  Inno Setup not found. Install from https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    Write-Host "📦 Published files available in: bin\Release\net10.0\win-x64\publish\" -ForegroundColor Cyan
}

Write-Host "`n✨ Build complete! 73 de MM5AGM 📻" -ForegroundColor Green