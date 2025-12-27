# 🚧 FT-dx101 Web Control App - Work in Progress 🚧
<img width="1326" height="1054" alt="WebApp" src="https://github.com/user-attachments/assets/052fe288-02f1-4461-b015-c94c63111c3f" />

![FT-dx101 Web Control Interface](pictures/webapp.png)

> **⚠️ ALPHA STAGE PROJECT** - This application is actively under development. Features may be incomplete, buggy, or subject to change. Use at your own risk!

---

I couldn't use OmniRig because the full installaion package doesn't appear to be available anymore.

I tried Com0Com and Hub4Com but had trouble getting them to work reliably on my system.

It was suggested using Com0Com with Com2TCP to create a virtual COM port that can be accessed over TCP/IP but there can be problems with that so I'm going to try and write a CAT Multiplexer with my good friend github copilot

## 📻 What is This?

A **modern, web-based control interface** for the **Yaesu FT-dx101 Series** amateur radio transceivers (FT-dx101MP and FT-dx101D). Control your radio through your web browser with a sleek, glass-morphism UI design!

### 🎯 What It Does

This ASP.NET Core Razor Pages application connects to your FT-dx101 radio via serial CAT (Computer Aided Transceiver) interface and provides:

- **📡 Dual VFO Control** - Monitor and control both Main (A) and Sub (B) receivers
- **🔢 Interactive Frequency Tuning** - Click any digit and use mouse wheel or touch swipe to tune
- **📊 Real-Time S-Meter** - Live signal strength display for both receivers
- **🎚️ Mode Selection** - Quick switching between LSB, USB, CW, FM, AM, DATA, RTTY, and C4FM
- **📶 Antenna Switching** - Select between ANT 1, 2, or 3 for each receiver
- **🔌 Connection Management** - Easy connect/disconnect with proper COM port handling
- **⚙️ Configurable Settings** - Configure serial port, baud rate, and server settings
- **🌐 Browser-Based** - Access from any device on your local network
- **🎨 Modern UI** - Beautiful glass-morphism design with multiple color themes

---

## 🏗️ Current Status

### ✅ Working Features
- Serial CAT communication with FT-dx101MP/D
- Real-time frequency and mode display for both VFOs
- S-meter readings from both receivers
- Interactive frequency tuning (digit selection + mouse wheel/swipe)
- Mode selection (LSB, USB, CW, FM, AM, DATA, RTTY, C4FM)
- Antenna selection (ANT 1/2/3 for Main and Sub)
- Auto-polling with error recovery
- Settings page for serial port configuration
- Proper connection/disconnection handling
- Mobile-responsive design

### 🚧 Known Issues / TODO
- [ ] Advanced features (split, clarifier, filters) not yet implemented
- [ ] TX indicators need testing
- [ ] Memory channel management not implemented
- [ ] Band stacking register access not implemented
- [ ] No logging or contest mode features
- [ ] Limited error handling in some scenarios
- [ ] Settings validation could be improved
- [ ] No multi-user support (single connection only)
- [ ] No connection to other programs like WSJT-X. I need to get familiar with Omnirig I think
### 🎨 Available Themes
- Modern Glass Palette (default)
- Maybe later I'll add more themes:


---

## 📋 Requirements

### Software
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows OS (for SerialPort support)
- Modern web browser (Chrome, Edge, Firefox, Safari)

### Hardware
- **Yaesu FT-dx101MP** (dual receiver) or **FT-dx101D** (single receiver)
- USB cable or USB-to-Serial adapter
- Proper CAT configuration on radio (38400 baud recommended)

---

## 🚀 Quick Start

### 1. Clone the Repository

````````
### 📝 TODO List

- [ ] Implement advanced features (split, clarifier, filters)
- [ ] Test and improve TX indicators
- [ ] Add memory channel management
- [ ] Add band stacking register access
- [ ] Add logging and contest mode features
- [ ] Improve error handling in all services
- [ ] Enhance settings validation
- [ ] Add multi-user support
- [ ] Expand documentation for setup and troubleshooting
- [ ] Add more UI themes
- [ ] Document JTAlert and WSJT-X integration steps
- [ ] Add automated tests for CAT multiplexer and client logic
- [ ] Add proper signal strength and power meters to the UI
- [ ] Persist and restore last used antenna and other key settings on startup
- [ ] Remove the MODE bar from the UI (the mode button is sufficient)
- [ ] Check WSJT-X works with the built-in rigctld server
- [ ] Check Log4OM works with the CAT multiplexer and rigctld server
- [ ] Check JTAlert works with WSJT-X and receives correct radio status

---
