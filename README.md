
![FTdx101 WebApp Screenshot](pictures/WebApp.png)

## Windows Smart App Control Example
![Smart App Control Screenshot](pictures/SmartAppControl.png)

# FTdx101 WebApp

## 📖 Why This Application Exists
I wrote this application because I can't see the FTdx101MP controls without using a magnifying glass. As a ham who uses WSJT-X, JTAlert, and Log4OM, there are many controls on the radio that I simply never touch. This web-based interface gives me a clean, large, easy-to-read control panel for the functions I actually use day-to-day.

I also use this application on my tablet, which provides a portable control panel in the shack. The large buttons and readable display work great on touchscreens, though the digit-by-digit frequency tuning feature (click digit + mouse wheel) hasn't been implemented for touch devices yet.

## Important - You need .NET10 to run this application. Download the x86 version from the official Microsoft website: https://dotnet.microsoft.com/en-us/download/dotnet/10.0 and install it before running.

> **Why x86?** I use WSJT-X V3.0.0 Improved Widescreen Plus and that is 32 bit. This app is built as x86 for better integration.

---

## ⚠️ Limitations

- **Windows Only:** Serial port requires Windows. (Linux users report success with Wine)
- **Unsigned App:** Windows Smart App Control may block it - allow through the warning screen (see screenshot above)
- **Touch Frequency Tuning:** Not yet implemented for touch devices
- **Single Radio:** Designed for one radio at a time
- **Not Implemented:** Per-band memory, filter controls, antenna change detection from radio

---

## 🚀 Features

### Radio Control
- **CAT Control:** Full control over frequency, mode, band, and antenna (ANT 1/2/3)
- **Radio Power On/Off:** Power the radio directly from the web interface
- **TX Button:** Toggle transmit from VFO header - updates automatically with WSJT-X
- **Dual VFO Support:** Independent control of VFO A and VFO B
- **Band Selection:** Quick access to all bands (160m - 4m, UK-centric)
- **Power & Gain:** Adjustable power (0-200W) and MIC Gain (switches to Data Out Gain in DATA modes) - settings persist across restarts

### Metering
- **S-Meter:** Real-time analog-style signal strength display
- **TX Meters:** Live Power, SWR, and ALC gauges during transmit
- **PA Monitoring:** IDD current, voltage (~48V), and temperature

### Integration
- **Built-in CAT Multiplexer:** The app is the only process that opens the serial port - no conflicts!
- **TCP Server (rigctld):** Log4OM, GridTracker, and Hamlib tools connect over TCP
- **WSJT-X UDP:** Full UDP integration with WSJT-X, JTAlert, and Log4OM
- **No Virtual COM Ports:** Eliminates need for com0com or VSPE
- **External App Launchers:** Configure up to 3 app buttons (e.g., WSJT-X, JTAlert, Log4OM)

### Real-Time Updates
Uses **Auto Information Mode (AI1;)** - the radio streams status changes to the app automatically. Only S-meters are polled. Result: instant, responsive UI.

> *Architecture credit: Martin G8MAB*

---

## 🔧 What's Next

If there's interest in this program, I'm open to suggestions for additional controls to add, such as:

- Touch-friendly frequency tuning for tablets
- Per-band memory (frequency, mode, antenna, power)
- Filter selection (width, shift)
- Noise blanker controls
- AGC settings
- Clarifier/RIT controls
- Split operation
- Memory management

Suggestions welcome! Open an issue or discussion.

---

## 🏗️ Technology Stack

| Component | Technology |
|-----------|------------|
| Backend | ASP.NET Core Razor Pages (.NET 10) |
| Frontend | Bootstrap 5, JavaScript, HTML5 Canvas |
| CAT | Serial port + FT-dx101 protocol |
| Gauges | Canvas-Gauges library |
| Platform | Windows (serial port requirement) |

---

## ⚙️ Configuration

Settings are stored in `%AppData%\MM5AGM\FTdx101 WebApp\radio_state.json`

Created automatically on first run. Example:
```json
{
  "FrequencyA": 21074100,
  "FrequencyB": 14074000,
  "BandA": "15m",
  "BandB": "20m",
  "ModeA": "USB",
  "ModeB": "USB",
  "AntennaA": "1",
  "AntennaB": "1",
  "PowerA": 10,
  "PowerB": 0
}
```
