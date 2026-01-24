# FTdx101 Web Control Application

![FTdx101 Web Control Interface](pictures/WebAppMeters.png)

---

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

## 🌐 TCP Communication with Logging/Contest Software

**Current architecture:**  
The web app now acts as a TCP server (rigctld-compatible), and external applications (WSJT-X, Log4OM, JTAlert, etc.) connect to it over TCP.  
**Only the web app opens the radio's serial port.**  
This eliminates the need for serial port multiplexers or virtual COM port utilities. All CAT communication is managed by the web app, and other software interacts with the radio through the TCP interface.

---

## 🆕 Auto Information Mode (`AI1;`)

This application now leverages the FTdx101's **Auto Information mode** by sending the `AI1;` CAT command. When enabled, the radio automatically streams status updates to the application, eliminating the need for constant polling and providing a more responsive user experience.

**How it works:**
- On connection, the app sends `AI1;` to the radio.
- The radio pushes real-time status messages, which are processed and reflected in the UI.
- This reduces latency and improves the experience for live frequency and S-meter updates.

**Credit:**  
Special thanks to **Martin Bradford G8MAB** for suggesting the use of Auto Information mode (`AI1;`).

---

## 🔧 What’s Missing / Roadmap

- Power output adjustment (coming soon)
- Touch-friendly digit-by-digit frequency tuning
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

---

## 📦 Installation

> **⚠️ Installation Program Coming Soon!**  
> A full installer is in development. For now, follow the manual steps below.

### Manual Installation

1. **Install .NET 10 Runtime**  
   [Download .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)

2. **Clone this repository**

3. **Configure your serial port and radio settings**  
   Edit `appsettings.user.json` to match your radio's COM port and baud rate.

4. **Run the application**  
   Then open your browser to [http://localhost:8080](http://localhost:8080).

---

## 🖥️ Usage

- Use the web interface to control frequency, band, mode, and antenna.
- Connect WSJT-X, Log4OM, JTAlert, or other logging/contest software to the app’s TCP server (rigctld-compatible).
- All radio state changes are reflected in real time thanks to Auto Information mode.

---

## 📝 License

This project is open source and available under the MIT License.

---

## 🙏 Acknowledgements

- **Martin Bradford G8MAB** for suggesting Auto Information mode (`AI1;`).
- The ham radio and open source communities for feedback and support.

---
