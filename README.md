# 🤖 ROV Workspace

<div align="center">

![ROV Control System](https://img.shields.io/badge/Platform-Windows%20%7C%20Raspberry%20Pi%20%7C%20STM32-blue)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple)
![License](https://img.shields.io/badge/License-MIT-green)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow)

**A complete Remotely Operated Vehicle (ROV) control system for underwater robotics**

[Features](#-features) • [Architecture](#-architecture) • [Installation](#-installation) • [Usage](#-usage) • [Documentation](#-documentation)

</div>

---

## 📖 Overview

ROV Workspace is a multi-component robotics control system developed by the **Assiut Robotics Team**. It provides a complete solution for controlling an underwater ROV including:

- 🎮 Desktop control interface with real-time camera feeds
- 🕹️ Joystick input for intuitive vehicle control
- 📡 MAVLink-based telemetry and command protocol
- 🔧 6-thruster motor control with PID stabilization
- 🌡️ Environmental sensing (IMU, pressure, temperature)

---

## ✨ Features

### 🆕 Recent Updates (Jan 2026)
- **GUI**: Fixed `System.FormatException` crashes with new `FloatConverter` for safe empty string binding.
- **GUI**: Resolved "Joystick not found!" errors and startup race conditions.
- **Firmware**: Enabled MAVLink UDP sending (previously commented out).
- **Firmware**: Fixed semaphore deadlocks using `WaitAsync`.
- **Firmware**: Corrected initialization order (moved before `osKernelStart`) to prevent hard faults.

### WPF GUI Control Application
- **Real-time Camera Feeds** - Support for up to 3 simultaneous camera streams
- **3D ROV Visualization** - Interactive 3D model showing vehicle orientation
- **Telemetry Dashboard** - Speed, depth, heading, roll, pitch, yaw
- **Environmental Monitoring** - Water/tube temperature and pressure graphs
- **Joystick Control** - DirectInput compatible controller support
- **Settings Panel** - Configurable connection parameters and PID tuning

### STM32 Firmware
- **FreeRTOS** - Real-time operating system for reliable control
- **6-Channel PWM** - Independent thruster control via TIM1/TIM8
- **MAVLink Protocol** - Standard drone communication protocol
- **Sensor Integration** - MPU6050 IMU and pressure sensors via I2C/SPI
- **PID Controller** - Attitude and depth stabilization

### Python Bridge (Raspberry Pi)
- **UART↔UDP Bridge** - Bidirectional communication between PC and STM32
- **MJPEG Streaming** - Low-latency camera feeds via Flask
- **Multi-camera Support** - Up to 3 USB cameras

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        WINDOWS PC                                   │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                 WPF GUI Control Application                 │    │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐    │    │
│  │  │ Camera   │ │ Joystick │ │ Telemetry│ │ 3D Model     │    │    │
│  │  │ Viewers  │ │ Handler  │ │ Display  │ │ Visualization│    │    │
│  │  └────┬─────┘ └────┬─────┘ └────┬─────┘ └──────────────┘    │    │
│  └───────┼────────────┼────────────┼───────────────────────────┘    │
│          │            │            │                                │
└──────────┼────────────┼────────────┼────────────────────────────────┘
           │ HTTP       │ UDP        │ UDP
           │ 5000-7000  │ 14550      │ 14550
           ▼            ▼            ▼
┌──────────────────────────────────────────────────────────────────────┐
│                        RASPBERRY PI                                  │
│  ┌──────────────┐                  ┌─────────────────────────────┐   │
│  │  stream.py   │                  │     bridge.py               │   │
│  │  (Flask)     │                  │   UART ↔ UDP Bridge         │   │
│  │  Camera MJPEG│                  │                             │   │
│  └──────────────┘                  └──────────────┬──────────────┘   │
│                                                   │ UART 115200      │
└───────────────────────────────────────────────────┼──────────────────┘
                                                    ▼
┌──────────────────────────────────────────────────────────────────────┐
│                        STM32F405RGT6                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌────────────┐   │
│  │   MAVLink   │  │   Motor     │  │    PID      │  │  Sensors   │   │
│  │   Parser    │──│  Interface  │──│ Controller  │──│ IMU/Press  │   │
│  │   (DMA)     │  │  (6x PWM)   │  │             │  │            │   │
│  └─────────────┘  └─────────────┘  └─────────────┘  └────────────┘   │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
ROV Workspace/
├── 📂 ROV GUI Control/           # Windows WPF Application
│   ├── ViewModels/               # MVVM ViewModels
│   ├── View/                     # WPF User Controls
│   ├── MAVLink/                  # MAVLink protocol
│   └── ROV GUI Control.sln       # Visual Studio solution
│
├── 📂 STM32F405RGT6/             # Embedded Firmware
│   ├── Core/Src/                 # Application source
│   ├── Drivers/                  # HAL drivers
│   └── STM32F405RGT6.ioc         # CubeMX configuration
│
├── 📂 python/                    # Python scripts
│   ├── bridge.py                 # UART↔UDP bridge
│   ├── stream.py                 # Camera streaming
│   └── udp_streamer.py           # Raw UDP streaming
│
├── 📂 documentations/            # Project documentation
│   ├── documentation.md          # Full setup guide
│   └── technical_audit_report.md # Code audit findings
│
├── 📂 docx/                      # Reference documents
├── 📂 images/                    # Reference images
└── 📂 excel/                     # Hardware specifications
```

---

## 🔧 Prerequisites

### Windows Development Machine
| Requirement          | Version        |
| -------------------- | -------------- |
| Visual Studio        | 2022 (17.0+)   |
| .NET Framework       | 4.7.2          |
| DirectInput Joystick | Any compatible |

### Raspberry Pi
| Requirement     | Version         |
| --------------- | --------------- |
| Raspberry Pi    | 4B/5 (2GB+ RAM) |
| Raspberry Pi OS | Bookworm 64-bit |
| Python          | 3.9+            |

### STM32 Development
| Requirement  | Version  |
| ------------ | -------- |
| STM32CubeIDE | 1.13+    |
| ST-Link      | V2 or V3 |

---

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/Antony-Gh/ROV-Workspace-New.git
cd ROV-Workspace-New
```

### 2. WPF GUI (Windows)

```powershell
# Open in Visual Studio
start "ROV GUI Control\ROV GUI Control.sln"

# Restore NuGet packages (automatic on build)
# Build: Ctrl+Shift+B
```

### 3. Python Bridge (Raspberry Pi)

```bash
# Install dependencies
sudo apt update
sudo apt install -y python3-pip python3-opencv
pip3 install flask pyserial

# Copy scripts
scp python/*.py pi@<raspberry-pi-ip>:/home/rov/
```

### 4. STM32 Firmware

1. Open STM32CubeIDE
2. Import project: `File → Import → Existing Projects`
3. Select `STM32F405RGT6` folder
4. Build: `Ctrl+B`
5. Flash via ST-Link

---

## 🎮 Usage

### 1. Start Services on Raspberry Pi

```bash
# Terminal 1 - MAVLink Bridge
python3 bridge.py

# Terminal 2 - Camera Stream
python3 stream.py
```

### 2. Power On STM32

- Connect power to the STM32 board
- Wait ~10 seconds for ESC calibration

### 3. Launch GUI

```powershell
# From Visual Studio: F5
# Or run executable:
.\ROV GUI Control\bin\Debug\ROV GUI Control.exe
```

### 4. Connect and Control

1. Click **Connect** → Wait for confirmation
2. Click **Power** → Enable system
3. Click **Enable** → Arm thrusters
4. Use joystick for control

---

## ⚙️ Configuration

### Network Settings

Edit in `MainViewModel.cs`:

```csharp
Host_IP = "192.168.0.100";     // Raspberry Pi IP
Cam1_Port = 5000;              // Camera ports
MAVLink_Port = 14550;          // MAVLink port
```

### Raspberry Pi Bridge

Edit in `bridge.py`:

```python
uart_port = '/dev/serial0'      # UART device
udp_target_ip = '192.168.0.132' # Windows PC IP
```

---

## 📚 Documentation

| Document                                                              | Description                     |
| --------------------------------------------------------------------- | ------------------------------- |
| [documentation.md](documentations/documentation.md)                   | Full installation & setup guide |
| [technical_audit_report.md](documentations/technical_audit_report.md) | Code audit & known issues       |

---

## 🔌 Hardware Pinout

### STM32F405RGT6

| Peripheral | Pins     | Function        |
| ---------- | -------- | --------------- |
| USART2     | PA2/PA3  | MAVLink UART    |
| TIM1 CH1-4 | PA8-PA11 | Motors 0-3 PWM  |
| TIM8 CH1-3 | PC6-PC8  | Motors 4-5 PWM  |
| I2C1       | PB6/PB7  | MPU6050 IMU     |
| SPI1       | PA5-PA7  | Pressure Sensor |

---

## 🛠️ Dependencies

### WPF GUI (NuGet Packages)

| Package               | Version   | Purpose            |
| --------------------- | --------- | ------------------ |
| CommunityToolkit.Mvvm | 8.4.0     | MVVM framework     |
| HelixToolkit.Wpf      | 2.27.0    | 3D visualization   |
| OxyPlot.Wpf           | 2.2.0     | Charts/graphs      |
| SharpDX.DirectInput   | 4.2.0     | Joystick input     |
| Renci.SshNet          | 2021.10.2 | SSH communication  |
| Newtonsoft.Json       | 13.0.3    | JSON serialization |

### Python

```
flask
pyserial
opencv-python
```

---

## 🐛 Known Issues

See [Technical Audit Report](documentations/technical_audit_report.md) for detailed findings.

| Issue                             | Severity | Status    |
| --------------------------------- | -------- | --------- |
| UDP Send commented out            | Critical | **Fixed** |
| Semaphore not acquired            | Critical | **Fixed** |
| Firmware init after osKernelStart | Critical | **Fixed** |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Team

**Assiut Robotics Team** - ROV Division

---

<div align="center">

Made with ❤️ for underwater exploration

</div>
