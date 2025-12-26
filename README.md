# 🚧 FT-dx101 Web Control App - Work in Progress 🚧

![FT-dx101 Web Control Interface](pictures/webapp.png)

> **⚠️ ALPHA STAGE PROJECT** - This application is actively under development. Features may be incomplete, buggy, or subject to change. Use at your own risk!

---

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

### 🎨 Available Themes
- Modern Glass Palette (default)
- Ocean Blue
- Dark Operator
- LCD Green
- Purple Gradient

---

## 📋 Requirements

### Software
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows OS (for SerialPort support)
- Modern web browser (Chrome, Edge, Firefox, Safari)

### Hardware
- **Yaesu FT-dx101MP** (dual receiver) or **FT-dx101D** (single receiver)
- USB cable or USB-to-Serial adapter
- Proper CAT configuration on radio (19200 baud recommended)

---

## 🚀 Quick Start

### 1. Clone the Repository
