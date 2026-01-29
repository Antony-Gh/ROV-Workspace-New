# ROV Workspace - Project Documentation

> Complete guide for setting up, building, running, and deploying the ROV control system.

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Structure](#project-structure)
4. [Installation](#installation)
5. [Environment Configuration](#environment-configuration)
6. [Running the Project](#running-the-project)
7. [Building for Production](#building-for-production)
8. [Deployment](#deployment)
9. [Troubleshooting](#troubleshooting)

---

## Overview

The ROV Workspace is a remotely operated vehicle control system consisting of three main components:

| Component | Technology | Purpose |
|-----------|------------|---------|
| **WPF GUI Control** | C#/.NET 4.7.2, WPF | Desktop control interface with camera feeds, joystick input, and telemetry |
| **STM32 Firmware** | C, FreeRTOS, STM32CubeIDE | Motor control, sensor reading (IMU, pressure), MAVLink communication |
| **Python Bridge** | Python 3.x | UART↔UDP bridge and camera streaming on Raspberry Pi |

### Communication Flow

```
[Windows PC]                    [Raspberry Pi]              [STM32F405RGT6]
   WPF GUI ───UDP/14550──────► bridge.py ───UART/115200──► MAVLink Parser
   Joystick ─UDP/14550───────►           │                        │
   Camera Viewers ◄──HTTP/5000─ stream.py                   Motor Control
                             ◄──HTTP/6000─                   Sensors (IMU/Pressure)
```

---

## Prerequisites

### 1. Windows Development Machine

| Requirement | Version | Download |
|-------------|---------|----------|
| Visual Studio 2022 | 17.0+ | [Download](https://visualstudio.microsoft.com/) |
| .NET Framework | 4.7.2 | Included with Windows 10/11 |
| Git | 2.30+ | [Download](https://git-scm.com/) |

**Visual Studio Workloads Required:**
- ✅ .NET Desktop Development
- ✅ Desktop development with C++

**Joystick Hardware:**
- Any DirectInput-compatible joystick (Xbox controller, flight stick, etc.)

---

### 2. Raspberry Pi (Bridge/Camera)

| Requirement | Version | Notes |
|-------------|---------|-------|
| Raspberry Pi | 4B or 5 | 2GB+ RAM recommended |
| Raspberry Pi OS | Bookworm (64-bit) | [Download](https://www.raspberrypi.com/software/) |
| Python | 3.9+ | Pre-installed on Raspberry Pi OS |
| USB Cameras | Any V4L2 compatible | Up to 3 cameras supported |

**Network Requirements:**
- Static IP configuration
- Same network as Windows PC

---

### 3. STM32 Development

| Requirement | Version | Download |
|-------------|---------|----------|
| STM32CubeIDE | 1.13+ | [Download](https://www.st.com/en/development-tools/stm32cubeide.html) |
| ST-Link Debugger | V2 or V3 | For flashing firmware |
| STM32CubeMX | 6.9+ | Included in CubeIDE |

**Hardware:**
- STM32F405RGT6 development board
- UART-to-USB converter (for Pi connection)
- ESCs (Electronic Speed Controllers) for thrusters

---

## Project Structure

```
ROV Workspace/
├── ROV GUI Control/              # Windows WPF Application
│   ├── ViewModels/               # MVVM ViewModels
│   │   ├── MainViewModel.cs      # Main application logic
│   │   ├── MAVLinkHandler.cs     # MAVLink UDP communication
│   │   ├── CAMStream.cs          # Camera UDP receiver
│   │   └── JOYStick.cs           # Joystick input handler
│   ├── View/                     # WPF User Controls
│   ├── MAVLink/                  # MAVLink protocol implementation
│   ├── ROV GUI Control.csproj    # Project file
│   └── packages.config           # NuGet dependencies
│
├── STM32F405RGT6/                # Embedded Firmware
│   ├── Core/
│   │   ├── Src/
│   │   │   ├── main.c            # Entry point
│   │   │   ├── motor_interface.c # PWM motor control
│   │   │   ├── PID.c             # PID controller
│   │   │   └── mavlink_messages.c
│   │   └── Inc/                  # Header files
│   ├── Drivers/                  # HAL drivers
│   └── STM32F405RGT6.ioc         # CubeMX configuration
│
├── bridge.py                     # UART ↔ UDP bidirectional bridge
├── stream.py                     # Flask MJPEG camera server
├── uart_to_udp.py               # Unidirectional UART → UDP
├── udp_to_uart.py               # Unidirectional UDP → UART
└── udp_streamer.py              # UDP raw camera streaming
```

---

## Installation

### Step 1: Clone the Repository

```powershell
cd "E:\Robotics Team\ROV"
# If using Git:
git clone <repository-url> "ROV Workspace"
```

---

### Step 2: Install WPF GUI Dependencies

1. **Open Solution in Visual Studio:**
   ```
   E:\Robotics Team\ROV\ROV Workspace\ROV GUI Control\ROV GUI Control.sln
   ```

2. **Restore NuGet Packages:**
   - Right-click solution → **Restore NuGet Packages**
   - Or from Package Manager Console:
     ```powershell
     Update-Package -reinstall
     ```

3. **Verify Package Installation:**
   
   | Package | Version | Purpose |
   |---------|---------|---------|
   | CommunityToolkit.Mvvm | 8.4.0 | MVVM framework |
   | HelixToolkit.Wpf | 2.27.0 | 3D visualization |
   | OxyPlot.Wpf | 2.2.0 | Charts/graphs |
   | SharpDX.DirectInput | 4.2.0 | Joystick input |
   | Core.Renci.SshNet | 2021.10.2 | SSH communication |
   | Newtonsoft.Json | 13.0.3 | JSON serialization |

---

### Step 3: Setup Raspberry Pi

1. **Enable SSH and connect:**
   ```bash
   ssh pi@<raspberry-pi-ip>
   ```

2. **Install Python dependencies:**
   ```bash
   sudo apt update
   sudo apt install -y python3-pip python3-opencv
   pip3 install flask pyserial
   ```

3. **Copy bridge scripts to Pi:**
   ```powershell
   # From Windows PowerShell
   scp bridge.py stream.py pi@<raspberry-pi-ip>:/home/rov/
   ```

4. **Configure static IP (optional but recommended):**
   ```bash
   sudo nano /etc/dhcpcd.conf
   ```
   Add:
   ```
   interface eth0
   static ip_address=192.168.0.100/24
   static routers=192.168.0.1
   ```

---

### Step 4: Setup STM32 Firmware

1. **Open in STM32CubeIDE:**
   ```
   File → Import → Existing Projects into Workspace
   Select: E:\Robotics Team\ROV\ROV Workspace\STM32F405RGT6
   ```

2. **Build the project:**
   - Press `Ctrl+B` or **Project → Build All**

3. **Flash to board:**
   - Connect ST-Link debugger
   - **Run → Debug** or **Run → Run**

---

## Environment Configuration

### WPF GUI Settings

Configuration is currently hardcoded in [MainViewModel.cs](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/ROV%20GUI%20Control/ViewModels/MainViewModel.cs). Modify these values before building:

```csharp
// MainViewModel.cs - Constructor
Host_IP = "192.168.0.100";    // Raspberry Pi IP
Cam1_Port = 5000;             // Camera 1 port
Cam2_Port = 6000;             // Camera 2 port
Cam3_Port = 7000;             // Camera 3 port
Joystick_Port = 14550;        // Joystick commands port
MAVLink_Port = 14550;         // MAVLink telemetry port
```

> ⚠️ **Note**: These should be moved to a configuration file in production.

---

### Raspberry Pi Configuration

Edit [bridge.py](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/bridge.py):
```python
uart_port = '/dev/serial0'      # Or '/dev/ttyUSB0' for USB adapter
baudrate = 115200
udp_target_ip = '192.168.0.132' # Windows PC IP
udp_target_port = 14550
```

Edit [stream.py](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/stream.py):
```python
camera = cv2.VideoCapture(0)    # Camera device index (0, 1, 2...)
port = 5000                     # HTTP server port
```

---

### STM32 Configuration

The [.ioc](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/STM32F405RGT6/STM32F405RGT6.ioc) file configures peripherals via STM32CubeMX:

| Peripheral | Configuration | Pins |
|------------|---------------|------|
| USART2 | 115200 baud, DMA | PA2 (TX), PA3 (RX) |
| TIM1 | 50Hz PWM (motors 0-3) | PA8-PA11 |
| TIM8 | 50Hz PWM (motors 4-5) | PC6-PC8 |
| I2C1 | 100kHz (MPU6050) | PB6 (SCL), PB7 (SDA) |
| SPI1 | 328kHz (pressure sensor) | PA5-PA7 |

---

## Running the Project

### 1. Start Raspberry Pi Services

```bash
# Terminal 1 - MAVLink Bridge
cd /home/rov
python3 bridge.py

# Terminal 2 - Camera Stream (per camera)
python3 stream.py  # Camera 1 on port 5000
# Modify port for additional cameras
```

**Or run as systemd services for auto-start:**
```bash
sudo nano /etc/systemd/system/rov-bridge.service
```
```ini
[Unit]
Description=ROV MAVLink Bridge
After=network.target

[Service]
ExecStart=/usr/bin/python3 /home/rov/bridge.py
WorkingDirectory=/home/rov
Restart=always
User=rov

[Install]
WantedBy=multi-user.target
```
```bash
sudo systemctl enable rov-bridge
sudo systemctl start rov-bridge
```

---

### 2. Power On STM32

1. Connect power to STM32 board
2. Wait 10 seconds for ESC calibration
3. Verify UART connection to Pi

---

### 3. Launch WPF GUI

**From Visual Studio:**
- Press `F5` (Debug) or `Ctrl+F5` (Run without debugging)

**From compiled executable:**
```
E:\Robotics Team\ROV\ROV Workspace\ROV GUI Control\bin\Debug\ROV GUI Control.exe
```

---

### 4. Connect to ROV

1. Click **Connect** button in GUI
2. Wait for connection confirmation
3. Click **Power** to enable system
4. Click **Enable** to arm thrusters
5. Use joystick for control

---

## Building for Production

### WPF GUI - Release Build

1. **Change configuration to Release:**
   - Build → Configuration Manager → **Release**

2. **Build solution:**
   ```powershell
   cd "E:\Robotics Team\ROV\ROV Workspace\ROV GUI Control"
   msbuild "ROV GUI Control.sln" /p:Configuration=Release /p:Platform="Any CPU"
   ```

3. **Output location:**
   ```
   ROV GUI Control\bin\Release\
   ```

4. **Required files for distribution:**
   - `ROV GUI Control.exe`
   - All `.dll` files
   - [ROV2025.obj](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/ROV%20GUI%20Control/ROV2025.obj) (3D model)
   - [NoSignal.png](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/ROV%20GUI%20Control/NoSignal.png), [Settings.png](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/ROV%20GUI%20Control/Settings.png)

---

### STM32 Firmware - Release Build

1. **Select Release configuration:**
   - Right-click project → Build Configurations → Set Active → Release

2. **Build and verify:**
   - Check `Debug\STM32F405RGT6.bin` output

---

## Deployment

### Deployment Checklist

- [ ] Configure static IPs on all devices
- [ ] Update hardcoded IPs in source code
- [ ] Test all camera connections
- [ ] Verify joystick is recognized
- [ ] Confirm ESC calibration completes
- [ ] Test basic movement controls
- [ ] Verify telemetry reception

### Network Diagram

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Windows PC     │     │  Raspberry Pi    │     │  STM32F405RGT6  │
│  192.168.0.132  │◄───►│  192.168.0.100   │◄───►│  (via UART)     │
│                 │ UDP │                  │UART │                 │
│  Port 14550     │     │  Port 14550      │     │  115200 baud    │
│  Port 5000-7000 │     │  Port 5000-7000  │     │                 │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

---

## Troubleshooting

### Issue: "Connect failed" in GUI

**Symptoms**: Connection button fails, "Connect to X faild!" message

**Causes & Solutions**:

| Cause | Solution |
|-------|----------|
| Wrong IP address | Verify Pi IP with `hostname -I` on Pi |
| Pi SSH not running | `sudo systemctl start ssh` |
| Firewall blocking | Disable Windows Firewall for private network |
| Network not connected | Verify both devices on same subnet |

---

### Issue: No camera feed displayed

**Symptoms**: Black screen or "No Signal" image

**Causes & Solutions**:

| Cause | Solution |
|-------|----------|
| Camera not connected | `ls /dev/video*` on Pi to verify |
| Wrong port | Check port matches [stream.py](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/stream.py) configuration |
| Stream not started | `python3 stream.py` manually first |
| Firewall | Allow inbound TCP 5000-7000 on Windows |

**Test camera manually:**
```bash
# On Raspberry Pi
python3 -c "import cv2; print(cv2.VideoCapture(0).read()[0])"
# Should print: True
```

---

### Issue: Joystick not recognized

**Symptoms**: MoveID stays at -1, no control response

**Causes & Solutions**:

| Cause | Solution |
|-------|----------|
| Joystick not connected | Connect before launching app |
| Wrong driver | Install joystick drivers |
| Not DirectInput compatible | Use XInput-compatible controller |

**Verify in Windows:**
1. Open `joy.cpl` (Game Controllers)
2. Verify joystick appears and calibrates

---

### Issue: Motors don't respond

**Symptoms**: Joystick moves but ROV doesn't move

**Causes & Solutions**:

| Cause | Solution |
|-------|----------|
| Not armed | Click Power → Enable in GUI |
| UDP not sending | **CRITICAL BUG** - see audit report |
| ESC not calibrated | Wait 10s after power-on |
| UART disconnected | Check serial connection Pi↔STM32 |

**Debug MAVLink traffic:**
```bash
# On Raspberry Pi
python3 -c "
import socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(('0.0.0.0', 14550))
while True:
    data, addr = sock.recvfrom(1024)
    print(f'Received {len(data)} bytes from {addr}')
"
```

---

### Issue: STM32 firmware doesn't run

**Symptoms**: No telemetry, no motor control

**Causes & Solutions**:

| Cause | Solution |
|-------|----------|
| Not programmed | Flash firmware via ST-Link |
| Init code not reached | **CRITICAL BUG** - move init before osKernelStart() |
| Wrong baud rate | Verify 115200 on both UART ends |

**Debug with SWD:**
1. Connect ST-Link
2. Open STM32CubeIDE
3. Run → Debug
4. Set breakpoint at [main()](file:///e:/Robotics%20Team/ROV/ROV%20Workspace/STM32F405RGT6/Core/Src/main.c#63-122) line 63

---

### Issue: Build fails - missing packages

**Symptoms**: NuGet restore errors

**Solution:**
```powershell
cd "E:\Robotics Team\ROV\ROV Workspace\ROV GUI Control"
nuget restore "ROV GUI Control.sln"
# Or in Visual Studio: Tools → NuGet Package Manager → Package Manager Console
Update-Package -reinstall
```

---

### Issue: Python ImportError on Raspberry Pi

**Symptoms**: `ModuleNotFoundError: No module named 'cv2'`

**Solution:**
```bash
sudo apt install python3-opencv
# For Flask:
pip3 install flask
# For serial:
pip3 install pyserial
```

---

## Support

For additional assistance:
1. Check the [Technical Audit Report](./technical_audit_report.md) for known issues
2. Review inline code comments
3. Contact the Assiut Robotics team

---

*Documentation version: 1.0 | Last updated: January 2026*
