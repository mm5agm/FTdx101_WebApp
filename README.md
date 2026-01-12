# FT-dx101 Web Control Application

<img width="1326" height="1054" alt="WebApp" src="https://github.com/user-attachments/assets/052fe288-02f1-4461-b015-c94c63111c3f" />

![FT-dx101 Web Control Interface](pictures/webapp.png)

---

## 📖 Why This Application Exists

I wrote this application because **I can't see the FT-dx101MP controls without using a magnifying glass**. As a ham who uses **WSJT-X** along with **JTAlert** and **Log4OM**, there are many controls on the radio that I simply never touch. This web-based interface gives me a clean, large, easy-to-read control panel for the functions I actually use day-to-day.

### 🔌 Serial Port Multiplexing Solution

I needed a way to share the radio's serial port between this web app and other software like WSJT-X. After investigating various options:

- **OmniRig** - Couldn't find a complete, working version available for download
- **Com0Com** - Had reliability issues on my system
- **Com0Com + Com2TCP** - Complex setup with potential stability problems

**Solution:** I built a **CAT multiplexer** directly into this application. It allows multiple programs to share access to the radio's serial port simultaneously, without needing third-party virtual COM port utilities.

### 🎯 Current Capabilities

The application currently **fulfills my personal needs** for daily operation:

✅ **Frequency Control** - Large, readable frequency display with interactive tuning  
✅ **Band Selection** - Quick access to all amateur bands (160m - 4m)  
✅ **Mode Selection** - LSB, USB, CW, FM, AM, DATA-USB, RTTY-USB, C4FM  
✅ **Antenna Switching** - Select between ANT 1, 2, or 3  
✅ **S-Meter Display** - Analog gauge showing signal strength with proper calibration  
✅ **Dual Receiver Support** - Independent control of both VFO A and VFO B  
✅ **Built-in CAT Multiplexer** - Share serial port with other applications  

### 🔧 What's Missing (For Now)

The only control I still need to add is **power output adjustment**. Once I have an installation program built and tested, that will likely be the next feature.

### 🤝 Open to Suggestions

**If there's interest in this program, I'm open to suggestions** for additional controls to add, such as:
- Power output control (coming soon)
- Filter selection (width, shift)
- Noise blanker controls
- AGC settings
- Clarifier/RIT controls
- Split operation
- Memory management
- Other features you'd find useful

Feel free to open an issue or discussion with your ideas!

---

## 🏗️ Technology Stack

- **Framework:** ASP.NET Core Razor Pages (.NET 10)
- **Frontend:** Bootstrap 5, JavaScript, HTML5 Canvas
- **CAT Control:** Serial Port communication via FT-dx101 CAT protocol
- **Gauges:** Canvas-Gauges library for analog S-Meter display
- **Multiplexer:** Built-in CAT command multiplexer for port sharing

---

## 📦 Installation

> **⚠️ Installation Program Coming Soon!**  
> I am currently building an installation program for easy deployment. Once complete, I'll publish full installation instructions here.

### Manual Installation (For Now)

If you want to try it before the installer is ready:

1. **Install .NET 10 Runtime** (required if not using self-contained deployment)
   - Download from: https://dotnet.microsoft.com/download/dotnet/10.0

2. **Download the latest release** from the [Releases page](https://github.com/mm5agm/FTdx101_WebApp/releases)

3. **Extract and run** the application:
4. **Configure serial port** in Settings (default: COM3, 38400 baud)

5. **Access the interface** at: http://localhost:5000

---

## 🚀 Quick Start Guide

### First Time Setup

1. **Connect your FT-dx101** via USB cable
2. **Configure radio CAT settings:**
- Menu → CAT Settings → CAT Rate: 38400 bps
- CAT Mode: RTS/CTS or OFF
3. **Launch the web application**
4. **Go to Settings page** and configure your COM port
5. **Click Connect** and start operating!

### Daily Operation

- **Change Frequency:** Click any digit and use mouse wheel to tune
- **Change Band:** Click the band button (160m, 80m, etc.)
- **Change Mode:** Click mode button (LSB, USB, CW, etc.)
- **Switch Antenna:** Click ANT 1, 2, or 3
- **Monitor Signal:** Watch the analog S-Meter gauge

---

## 🔌 Integration with Other Software

This application is designed to work alongside:

- **WSJT-X** - Digital mode software
- **JTAlert** - Alert notification system
- **Log4OM** - Amateur radio logging software

> **Note:** I use these programs daily. The FT-dx101 WebControl app provides visual monitoring and quick access to controls I need, while WSJT-X handles digital mode operations and logging programs manage QSO records.The built-in CAT multiplexer allows all these programs to share the radio's serial port without conflicts.

---

## 🎨 Features

### Interactive Frequency Display
- Large, easy-to-read 9-digit frequency display
- Click any digit to select it
- Use mouse wheel to increment/decrement
- Touch-friendly for tablet operation
- Auto-updates from radio status

### Analog S-Meter Gauges
- Semicircular analog gauge design
- Real-time signal strength display
- Calibrated S0-S9 and S9+20/40/60 scales
- Green zone (S0-S9), Red zone (S9+)
- Custom positioned labels along arc

### Dual Receiver Control
- Independent control of both VFOs
- Color-coded panels (Blue = Receiver A, Green = Receiver B)
- Simultaneous monitoring and control
- Individual antenna selection per receiver

### Clean, Modern Interface
- Bootstrap 5 responsive design
- Glass-morphism UI effects
- Mobile and tablet friendly
- High contrast for visibility
- Large, clickable buttons

---

## 📋 System Requirements

### Software
- **Operating System:** Windows 10/11 (64-bit)
- **.NET Runtime:** .NET10.0 (included with self-contained installer)
- **Web Browser:** Chrome, Edge, Firefox, or Safari (latest versions)

### Hardware
- **Radio:** Yaesu FT-dx101MP or FT-dx101D
- **Connection:** USB cable (supplied with radio) or USB-to-Serial adapter
- **Computer:** Any modern PC capable of running .NET applications
- **Serial Port:** Physical or virtual COM port

### Radio Configuration
- **CAT Rate:** 38400 baud (recommended)
- **CAT Mode:** RTS/CTS or OFF
- **CAT Protocol:** Standard FT-dx101 CAT commands

---

## 🛠️ Development Status

### Current Version
The application is in active development and **fully functional for daily use**. It handles the core functions I need for operating with digital modes and logging software.

### Roadmap

**Next Up:**
- [ ] Power output control slider
- [ ] Installation program (in progress)
- [ ] Installation documentation

**Considering (based on user feedback):**
- [ ] Filter width/shift controls
- [ ] Noise blanker controls
- [ ] AGC settings
- [ ] Clarifier/RIT controls
- [ ] Split operation
- [ ] Memory channel management

### Known Limitations
- Windows only (due to SerialPort library)
- Single user connection (nomulti-client support)
- No TX power control yet (coming soon)
- Basic error recovery (adequate for normal operation)

---

## 🤝 Contributing

I built this for my own use, but if you find it helpful and have suggestions:

1. **Open an Issue** - Describe the feature or problem
2. **Start a Discussion** - Share your ideas for improvements
3. **Submit a Pull Request** - If you've implemented something useful

I'm particularly interested in hearing from users who:
- Use similar software stacks (WSJT-X, JTAlert, Log4OM)
- Have visibility challenges with small radio controls
- Want to control their radio remotely within their home network

---

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](FILE) for details.

---

## 🙏 Acknowledgments

- **Yaesu** - For the excellent FT-dx101 series transceivers
- **GitHub Copilot** - For assistance in development
- **canvas-gauges** - For the analog meter library (https://canvas-gauges.com/)
- **Amateur Radio Community** - For feedback and suggestions

---

## 📞 Contact

**Callsign:** MM5AGM  
**GitHub:** [mm5agm](https://github.com/mm5agm)  
**Project:** [FTdx101_WebApp](https://github.com/mm5agm/FTdx101_WebApp)

---

## 📸 Screenshots

![Main Control Panel](pictures/webapp.png)
*Dual receiver control with analog S-meters*

---

**73 de MM5AGM** 📻

> *"Making amateur radio accessible, one line of code at a time."*