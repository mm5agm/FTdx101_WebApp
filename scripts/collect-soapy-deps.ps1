# collect-soapy-deps.ps1
# Copies locally-built SoapySDR DLLs into the soapysdr-dist/ folder
# that installer.nsi expects.  Run once before creating a release locally,
# or whenever the SoapySDR build is updated.
#
# Usage:  .\scripts\collect-soapy-deps.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$bin  = Join-Path $root "soapysdr-dist\bin"
$mods = Join-Path $root "soapysdr-dist\lib\SoapySDR\modules0.8-3"

New-Item -ItemType Directory -Force -Path $bin  | Out-Null
New-Item -ItemType Directory -Force -Path $mods | Out-Null

$copies = @(
    @{ Src = "C:\build\SoapySDR\lib\SoapySDR.dll";             Dst = "$bin\SoapySDR.dll" },
    @{ Src = "C:\deps\airspy\x64\airspy.dll";                   Dst = "$bin\airspy.dll" },
    @{ Src = "C:\build\libhackrf\libhackrf\src\hackrf.dll";     Dst = "$bin\hackrf.dll" },
    @{ Src = "C:\deps\rtlsdr\librtlsdr.dll";                    Dst = "$bin\librtlsdr.dll" },
    @{ Src = "C:\deps\rtlsdr\libusb-1.0.dll";                   Dst = "$bin\libusb-1.0.dll" },
    @{ Src = "C:\deps\rtlsdr\libwinpthread-1.dll";              Dst = "$bin\libwinpthread-1.dll" },
    @{ Src = "C:\deps\airspy\x64\pthreadVC2.dll";               Dst = "$bin\pthreadVC2.dll" },
    @{ Src = "C:\build\pthreads4w\pthreadVC3.dll";              Dst = "$bin\pthreadVC3.dll" },
    @{ Src = "C:\build\SoapyAirspy\airspySupport.dll";          Dst = "$mods\airspySupport.dll" },
    @{ Src = "C:\build\SoapyHackRF\HackRFSupport.dll";          Dst = "$mods\HackRFSupport.dll" },
    @{ Src = "C:\build\SoapyRTLSDR\rtlsdrSupport.dll";          Dst = "$mods\rtlsdrSupport.dll" }
)

foreach ($c in $copies) {
    if (!(Test-Path $c.Src)) {
        Write-Warning "Missing: $($c.Src) - skipping"
        continue
    }
    Copy-Item $c.Src $c.Dst -Force
    Write-Host "Copied $($c.Src)" -ForegroundColor Green
}

Write-Host ""
Write-Host "soapysdr-dist/ is ready." -ForegroundColor Cyan
