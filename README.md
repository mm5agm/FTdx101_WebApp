
![FTdx101 WebApp Screenshot](pictures/DevelopScreen.png)

## Windows Smart App Control Example
![Smart App Control Screenshot](pictures/SmartAppControl.png)

# FTdx101 WebApp

## 📖 Why This Application Exists
I wrote this application because I can't see the FTdx101MP controls without using a magnifying glass. As a ham who uses WSJT-X, JTAlert, and Log4OM, there are many controls on the radio that I simply never touch. This web-based interface gives me a clean, large, easy-to-read control panel for the functions I actually use day-to-day.

I also use this application on my tablet, which provides a portable control panel in the shack. The large buttons and readable display work great on touchscreens, though the digit-by-digit frequency tuning feature (click digit + mouse wheel) hasn't been implemented for touch devices yet.

---

## 🌱 Why Sponsorship Matters

I’m retired and maintain this project on a limited income, funding all development tools personally. AI‑assisted coding has been invaluable for building features quickly, but it isn’t free — I’ve spent roughly **$150 USD** so far, and with only a **Copilot Pro** subscription I often hit my monthly token limit within a week. That slows down progress until the next cycle resets.

If this project has helped you, please consider sponsoring it. Even small contributions make a real difference and help keep the development tools running.

The project has seen **602 clones from 139 unique cloners** in the last two weeks, which shows there’s genuine interest. Community support helps me keep the momentum going.

---

## Important - You need .NET10 to run this application. Download the x64 version from the official Microsoft website: https://builds.dotnet.microsoft.com/dotnet/Runtime/10.0.5/dotnet-runtime-10.0.5-win-x64.exe and install it as Administratorbefore running.

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
- **Power & Gain:** Adjustable power (5-200W MP, 5 - 100W D ) and MIC Gain (switches to Data Out Gain in DATA modes) - settings persist across restarts
- **Mode Selection:** USB, LSB, CW-U, CW_L, RTTY-L, RTTY-U, DATA-L, DATA-U, DATA-FM, DATA-FM-N, PSK, AM, AM-N, FM, FM-N
- **Filter selection** (width only at present. Shift to de added)
### Metering
- **S-Meter:** Real-time analog-style signal strength display
- **TX Meters:** Live Power, SWR, and ALC gauges during transmit
- **PA Monitoring:** IDD current, and temperature

### Integration
- **Built-in CAT Multiplexer:** The app is the only process that opens the serial port - no conflicts!
- **TCP Server (rigctld):** Log4OM, GridTracker, and Hamlib tools connect over TCP
- **WSJT-X UDP:** Full UDP integration with WSJT-X, JTAlert, and Log4OM
- **No Virtual COM Ports:** Eliminates need for com0com or VSPE, but user can also use a virtual COM port if they prefer to communicate with other software that way.
- **External App Launchers:** Configure up to 3 app buttons (e.g., WSJT-X, JTAlert, Log4OM)

### Real-Time Updates from radio
Uses **Auto Information Mode (AI1;)** - the radio streams status changes to the app automatically. Only S-meters are polled. Result: instant, responsive UI.

> * credit: Martin G8MAB*

---

## 🔧 What's Next

If there's interest in this program, I'm open to suggestions for additional controls to add, such as:

- Touch-friendly frequency tuning for tablets
- Per-band memory (frequency, mode, antenna, power)
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
| CAT | Serial port + FTdx101 protocol |
| Gauges | Canvas-Gauges library |
| Platform | Windows (serial port requirement) |

---

## ⚙️ Configuration

Settings are stored in "C:\Users\your_user_name\AppData\Roaming\MM5AGM\FTdx101 WebApp\

