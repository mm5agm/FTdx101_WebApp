
4. **Configure serial port** in Settings (default: COM3,38400 baud)

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

### DailyOperation

- **Change Frequency:** Click any digit and use mouse wheel to tune
- **Change Band:** Click the band button (160m, 80m, etc.)
- **Change Mode:** Click mode button (LSB, USB, CW, etc.)
- **Switch Antenna:** Click ANT 1, 2,or 3
- **Monitor Signal:** Watch the analog S-Meter gauge

---

## 🔌 Integration with Other Software

This application is designed to work alongside:

- **WSJT-X** - Digital mode software
- **JTAlert** -Alert notification system
- **Log4OM** - Amateur radio logging software

> **Note:** I use these programs daily. The FT-dx101 WebControl app provides visual monitoring and quick access to controls I need, while WSJT-X handles digital mode operations and logging programs manage QSO records.

---

## 🎨 Features

### Interactive Frequency Display
- Large, easy-to-read 9-digit frequency display
- Click any digit to select it
- Use mouse wheel toincrement/decrement
- Touch-friendly for tablet operation
- Auto-updates from radio status

### Analog S-Meter Gauges
- Semicircular analog gauge design
- Real-time signal strength display
- Calibrated S0-S9 and S9+20/40/60 scales
- Green zone (S0-S9), Red zone (S9+)
- Custom positioned labels along arc

###Dual Receiver Control
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
- **.NET Runtime:** .NET 10.0 (included with self-contained installer)
- **Web Browser:** Chrome, Edge, Firefox, or Safari (latest versions)

### Hardware
- **Radio:** Yaesu FT-dx101MP or FT-dx101D
- **Connection:** USB cable (supplied with radio) or USB-to-Serial adapter
- **Computer:** Any modern PC capable of running .NET applications
- **SerialPort:** Physical or virtual COM port

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
- Single user connection (no multi-client support)
- No TX power control yet (coming soon)
- Basic error recovery (adequate for normal operation)

---

## 🤝 Contributing

I builtthis for my own use, but if you find it helpful and have suggestions:

1. **Open an Issue** - Describe the feature or problem
2. **Start a Discussion** - Share your ideas for improvements
3. **Submit a Pull Request** - If you've implemented something useful

I'm particularly interested in hearing from users who:
- Use similar software stacks (WSJT-X, JTAlert, Log4OM)
- Have visibility challenges with small radio controls
- Want to control their radio remotely within their home network

---

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Yaesu** - For the excellent FT-dx101 series transceivers
- **GitHub Copilot** - For assistance in development
- **canvas-gauges** - For the analog meter library (https://canvas-gauges.com/)
- **Amateur Radio Community** - For feedback and suggestions

---

## 📞Contact

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