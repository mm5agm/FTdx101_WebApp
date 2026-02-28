Setting up Network Settings for WSJT-X, JTAlert and Log4OM

WSJT-X In the "Reporting" setting
Tick "Enable PSK Reporting Spotting"
UDP Server : 239.255.0.1
UDP Server port number :  2237
Outgoing Interfaces loopback_0
Multicast TTL 1
Tick Accept UDP requests, Notify on accepted UDP request, and Accepted UDP request restores window
I prefer Dark Mode and this can be toggled On/Off in the "View" menu on the main screen. Use Dark Style.
JTAlert - There's nothing to set up
JTAlert does not expose any WSJT‑X UDP settings at all in the current versions.  
There is no port field, no IP field, and no network configuration screen.

This is because:
**JTAlert no longer receives WSJT‑X data over UDP.
It reads WSJT‑X’s log file instead.**

WSJT‑X writes decodes → JTAlert reads the log file → JTAlert sends ADIF to Log4OM

Log4OM
Step‑by‑step: Configure Log4OM for JTAlert
1) Open the Log4OM Settings
In Log4OM:

Settings → Program Configuration → Software Integration → Connections

This is where Log4OM listens for incoming messages.

2) Add a new inbound UDP connection
Click Add and configure:

Type: JT_Message

Port: 2236 (recommended; JTAlert defaults to this)

Enabled: ✔