# FTdx101 Web Control Application

![FT-dx101 Web Control Interface](pictures/WebApp.png)

---

Now with a groups.io page: **ft101dx-webapp**

## 📖 Why This Application Exists

I wrote this application because **I can't see the FTdx101MP controls without using a magnifying glass**. As a ham who uses **WSJT-X**, **JTAlert**, and **Log4OM**, there are many controls on the radio that I simply never touch. This web-based interface gives me a clean, large, easy-to-read control panel for the functions I actually use day-to-day.

**I also use this application on my tablet**, which provides a portable control panel in the shack. The large buttons and readable display work great on touchscreens, though the digit-by-digit frequency tuning feature (click digit + mouse wheel) hasn't been implemented for touch devices yet.

---

## 🚀 Key Features

- **Large, Accessible UI:** Clean, readable controls for frequency, band, mode, and antenna selection.
- **Dual Receiver Support:** Independent control and display for VFO A and VFO B.
- **Live S-Meter and Power Display:** Real-time analog-style meters.
- **Auto Information Mode (`AI1;`):** The app enables the radio’s Auto Information mode, so the radio streams status updates (frequency, mode, S-meter, etc.) automatically to the web app for low-latency, real-time updates.
- **TCP Integration:** The app acts as a TCP server (rigctld-compatible), allowing external applications (WSJT-X, Log4OM, JTAlert, etc.) to connect over TCP. The web app is the only process that opens the radio's serial port.
- **No Virtual COM Ports Needed:** Eliminates the need for third-party serial port sharing utilities.
- **Tablet and Touch Friendly:** Optimized for use on tablets and touch devices.

---

### 🏗️ Architecture: Reactive State with Intelligent Polling

**Special thanks to Martin G8MAB** for suggesting a hybrid reactive architecture that dramatically improved performance and responsiveness!

**The Problem:** Early versions polled the radio constantly for all parameters, creating serial port congestion and slow UI updates.

**The Solution:** 
- **Reactive State Service** - Radio changes stream in real-time via Auto-Information (AI) mode
- **Smart Polling** - Only S-meters are polled (3x per second) since AI mode doesn't provide them
- **Instant Updates** - Frequency, mode, and antenna changes propagate immediately
- **No Queue Backup** - Minimal serial commands = fast, responsive UI

This architecture change in **v0.9.0** transformed the app from sluggish polling to buttery-smooth real-time control!

### 🎯 Current Capabilities

**Current architecture:**  
The web app now acts as a TCP server (rigctld-compatible), and external applications (WSJT-X, Log4OM, JTAlert, etc.) connect to it over TCP.  
**Only the web app opens the radio's serial port.**  
This eliminates the need for serial port multiplexers or virtual COM port utilities. All CAT communication is managed by the web app, and other software interacts with the radio through the TCP interface.

✅ **Frequency Control** - Large, readable frequency display with interactive tuning  
✅ **Band Selection** - Quick access to all amateur bands (160m - 4m)  
✅ **Mode Selection** - LSB, USB, CW, FM, AM, DATA-USB, RTTY-USB, C4FM  
✅ **Antenna Switching** - Select between ANT 1, 2, or 3  
✅ **Power Control** - Adjustable power output (0-200W for FT-dx101MP)  
✅ **S-Meter Display** - Real-time analog gauges showing signal strength  
✅ **Dual Receiver Support** - Independent control of both VFO A and VFO B  
✅ **Built-in CAT Multiplexer** - Share serial port with other applications  
✅ **Reactive Architecture** - Instant response to radio changes  
✅ **Tablet Compatible** - Works on tablets and touch devices  

### 🔧 What's Next

**If there's interest in this program, I'm open to suggestions** for additional controls to add, such as:
- Touch-friendly frequency tuning for tablets
- Per-band memory (frequency, mode, antenna, power)
- Filter selection (width, shift)
- Noise blanker controls
- AGC settings
- Clarifier/RIT controls
- Split operation
- Memory management

**Suggestions are welcome!**  
Open an issue or discussion with your ideas.

---

## 🏗️ Technology Stack

- **Backend:** ASP.NET Core Razor Pages (.NET 10)
- **Frontend:** Bootstrap 5, JavaScript, HTML5 Canvas
- **CAT Control:** Serial Port communication via FTdx101 CAT protocol
- **TCP Server:** rigctld-compatible interface for external apps
- **Gauges:** Canvas-Gauges library for analog S-Meter display
- **Architecture:** Reactive state with Auto-Information (AI) mode streaming
- **Multiplexer:** Built-in CAT command multiplexer for port sharing
- **Platform:** Windows only (serial port implementation requires Windows)

---

## 📥 Downloads

- **[FTdx101_WebApp-v0.9.0-win-x64.zip](https://github.com/mm5agm/FTdx101_WebApp/releases/download/v0.9.0/FTdx101_WebApp-v0.9.0-win-x64.zip)** - Windows x64 (self-contained, no .NET runtime required) ✅ **Recommended**

**⚠️ Note:** Linux builds are currently not functional due to serial port implementation issues in .NET 10. This is a known limitation and may be addressed in future versions.

---

## 🚀 Installation (Windows Only)

### Windows (Self-Contained) - Recommended
1. Download `FTdx101_WebApp-v0.9.0-win-x64.zip`
2. Extract to a folder of your choice
3. Edit `appsettings.user.json` to configure:
   - Your COM port (e.g., "COM3")
   - Radio model ("FTdx101MP" or "FTdx101D")
   - Baud rate (default: 38400)
4. Run `FTdx101_WebApp.exe`
5. Open browser to `http://localhost:8080`
6. **For tablet access:** Use `http://[your-pc-ip]:8080` from your tablet

### Example appsettings.user.json

```json
{
  "Connection": {
    "Port": "COM3",
    "Model": "FTdx101MP",
    "BaudRate": 38400
  }
}
```

---

## ✨ What's New in v0.9.0 - "First Reactive Version"

**Major architectural overhaul thanks to Martin G8MAB's suggestion!**

- ✅ **Reactive state architecture** - Real-time updates via Auto-Information (AI) mode
- ✅ **Smart S-meter polling** - Only polls what AI mode doesn't provide
- ✅ **Instant frequency/mode changes** - No more waiting for poll cycles
- ✅ **Smooth UI updates** - 2x per second refresh rate
- ✅ **Zero-dropout validation** - Defensive parsing prevents invalid readings
- ✅ **Proper startup state** - Loads actual radio state on launch
- ✅ **Automatic band detection** - Calculates band from frequency
- ✅ **Queue management** - Command multiplexer prevents serial port congestion

**Performance improvements:**
- Reduced serial port traffic by ~80%
- UI responsiveness improved from 2+ seconds to <500ms
- S-meters update smoothly with no dropouts
- Frequencies and modes change instantly

---

## 🤝 Acknowledgments

**Martin G8MAB** - For the excellent suggestion to move from continuous polling to a reactive architecture with AI mode streaming. This change dramatically improved the app's performance and responsiveness!

---

## 💡 Known Limitations

- **Windows Only** - Serial port implementation currently requires Windows due to .NET 10 SerialPort limitations
- **Touch Frequency Tuning** - Digit-by-digit frequency tuning (mouse wheel) not yet implemented for touch devices
- **Single Radio** - Designed for controlling one radio at a time

---

## 📞 Support & Community

- **GitHub Issues:** [Report bugs or request features](https://github.com/mm5agm/FTdx101_WebApp/issues)
- **Groups.io:** Join the discussion at **ft101dx-webapp**
- **License:** MIT (see LICENSE file)

---

## 📸 Screenshots

![Web Interface](pictures/WebApp.png)
*Main control interface showing dual receivers with analog S-meters and reactive state updates*

---

**73 de MM5AGM** 📻
