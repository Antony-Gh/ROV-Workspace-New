# STM32F405RGT6 - STM32CubeIDE Setup & Simulation Guide

## Table of Contents
1. [Prerequisites](#1-prerequisites)
2. [Opening the Project](#2-opening-the-project)
3. [STM32CubeMX Configuration](#3-stm32cubemx-configuration)
4. [Building the Project](#4-building-the-project)
5. [Fixing Critical Code Issues](#5-fixing-critical-code-issues)
6. [Simulation Options](#6-simulation-options)
7. [Debugging with ST-Link](#7-debugging-with-st-link)

---

## 1. Prerequisites

### Required Software

| Software | Version | Download |
|----------|---------|----------|
| STM32CubeIDE | 1.13.0+ | [Download](https://www.st.com/en/development-tools/stm32cubeide.html) |
| STM32CubeMX | 6.9.0+ | Included in CubeIDE |
| ST-Link Drivers | Latest | [Download](https://www.st.com/en/development-tools/stsw-link009.html) |

### Optional for Simulation

| Software | Purpose |
|----------|---------|
| Proteus | Full circuit simulation with STM32 |
| QEMU (STM32) | Open-source ARM emulation |
| Renode | Multi-node embedded simulation |

---

## 2. Opening the Project

### Step 1: Launch STM32CubeIDE

```
Windows: Start Menu → STM32CubeIDE
```

### Step 2: Import Existing Project

1. Go to **File → Import**
2. Select **General → Existing Projects into Workspace**
3. Click **Next**
4. **Browse** to: `E:\Robotics_Team\ROV\ROV Workspace\STM32F405RGT6`
5. Ensure the project is checked
6. Click **Finish**

### Step 3: Verify Project Structure

You should see in Project Explorer:
```
STM32F405RGT6
├── Core/
│   ├── Inc/              # Header files
│   └── Src/              # Source files
│       ├── main.c
│       ├── motor_interface.c
│       ├── PID.c
│       ├── mavlink_messages.c
│       └── ...
├── Drivers/              # HAL & CMSIS
├── Middlewares/          # FreeRTOS
└── STM32F405RGT6.ioc     # CubeMX config
```

---

## 3. STM32CubeMX Configuration

### Opening the .ioc File

1. Double-click `STM32F405RGT6.ioc` in Project Explorer
2. STM32CubeMX will open inside the IDE

### Current Peripheral Configuration

| Peripheral | Configuration | Pins |
|------------|---------------|------|
| **USART2** | 115200 baud, 8N1, DMA | PA2 (TX), PA3 (RX) |
| **TIM1** | 50Hz PWM (Motors 0-3) | PA8, PA9, PA10, PA11 |
| **TIM8** | 50Hz PWM (Motors 4-5) | PC6, PC7, PC8 |
| **TIM2** | PWM for pressure sensor | PA0 |
| **TIM3** | 10ms timer interrupt | Internal |
| **I2C1** | 100kHz (MPU6050) | PB6 (SCL), PB7 (SDA) |
| **SPI1** | 328kHz (Pressure) | PA5, PA6, PA7 |
| **FreeRTOS** | CMSIS_V2 | System tick |

### Modifying Configuration

1. Open `.ioc` file
2. Make changes in the GUI
3. Click **Generate Code** (gear icon) or `Alt+K`
4. Code is regenerated between `USER CODE BEGIN` and `USER CODE END` blocks

### Key Settings to Verify

**Clock Configuration (RCC):**
- System Clock: 84 MHz (from HSI + PLL)
- APB1: 42 MHz
- APB2: 42 MHz

**DMA Settings:**
- USART2_RX: DMA1 Stream5, Circular mode
- USART2_TX: DMA1 Stream6, Normal mode

---

## 4. Building the Project

### Build Steps

1. Select project in Project Explorer
2. **Project → Build Project** or `Ctrl+B`
3. Watch Console for output

### Expected Output

```
Build Finished. 0 errors, 0 warnings.
Memory Usage:
   FLASH: 45.2 KB / 1024 KB (4.4%)
   RAM:   12.8 KB / 128 KB (10.0%)
```

### Common Build Errors & Fixes

| Error | Solution |
|-------|----------|
| `undefined reference to 'xxx'` | Add missing source file to project |
| `multiple definition of 'xxx'` | Remove duplicate include or define |
| `HAL_xxx undeclared` | Include the correct HAL header |
| `FreeRTOS not found` | Enable Middleware in CubeMX |

---

## 5. Fixing Critical Code Issues

> ⚠️ **IMPORTANT**: The current code has bugs that prevent it from running!

### Issue 1: Unreachable Initialization Code

**File**: `Core/Src/main.c` (lines 76-95)

The initialization code is placed AFTER `osKernelStart()`, which never returns.

**Current (BROKEN):**
```c
osKernelInitialize();
defaultTaskHandle = osThreadNew(StartDefaultTask, NULL, &defaultTask_attributes);
osKernelStart();  // Never returns!

Motors_Init_Values();     // Never executed!
Motor_Set_timer(0, ...);  // Never executed!
// ... all init code unreachable
```

**Fixed:**
```c
osKernelInitialize();

// ===== MOVE ALL INIT CODE HERE =====
Motors_Init_Values();
Motor_Set_timer(0, &htim1, 1);
Motor_Set_timer(1, &htim1, 2);
Motor_Set_timer(2, &htim1, 3);
Motor_Set_timer(3, &htim1, 4);
Motor_Set_timer(4, &htim8, 1);
Motor_Set_timer(5, &htim8, 2);
Motor_Enable(0xFF);

Set_mavlink(&huart2, 1, 1);
Start_UART_DMA_Receive();
if(!MPU6050_Init()){}
Pressure_sensor_Setup(&hspi1, &htim2);
Pressure_sensor_Init();
System_Init();
// ===================================

defaultTaskHandle = osThreadNew(StartDefaultTask, NULL, &defaultTask_attributes);
osKernelStart();  // Now init code runs first!
```

### Issue 2: Empty Main Loop

**File**: `Core/Src/main.c` (lines 100-120)

All processing code is commented out.

**Current (BROKEN):**
```c
while (1)
{
    /* Everything is commented out */
}
```

**Fixed** - Uncomment the processing code:
```c
while (1)
{
    if(pressure_update_flag)
    {
        pressure_update_flag = false;
        Update_Pressuere();
    }
    if(imu_update_flag)
    {
        imu_update_flag = false;
        Update_Imu();
    }
    if (HAL_GetTick() - last_send_time >= 100)
    {
        last_send_time = HAL_GetTick();
        MavlinkTx_Process();
    }
}
```

Also uncomment the timer starts (lines 96-97):
```c
HAL_TIM_Base_Start_IT(&htim3);
HAL_TIM_Base_Start_IT(&htim6);  // Note: TIM6 needs to be configured in CubeMX
```

### Issue 3: Blocking ESC Calibration

**File**: `Core/Src/motor_interface.c` (line 47)

```c
HAL_Delay(10000);  // Blocks for 10 seconds - problematic with FreeRTOS
```

**Recommendation**: Move ESC calibration to a FreeRTOS task and use `osDelay()` instead.

---

## 6. Simulation Options

### Option A: Proteus Simulation (Recommended)

Proteus provides full circuit simulation with STM32 support.

**Setup Steps:**
1. Install **Proteus 8.13+** with STM32 library
2. Create new project
3. Add STM32F405RG component
4. Configure crystal: 8 MHz (or use internal HSI)
5. Load the `.hex` or `.elf` file from `Debug/`
6. Add virtual peripherals (LEDs, motors, UART terminal)

**Proteus Virtual Components:**
- Virtual Terminal (for UART)
- PWM measurement probes
- Logic analyzer

### Option B: QEMU STM32 (Free, Limited)

QEMU has limited STM32F4 support but works for basic testing.

```bash
# Install QEMU
choco install qemu

# Run (limited support)
qemu-system-arm -M stm32f405-discovery -kernel Debug/STM32F405RGT6.elf
```

### Option C: Renode (Free, Good Support)

Renode is an open-source embedded simulation framework.

**Installation:**
```bash
# Download from https://renode.io/
# Or via Chocolatey:
choco install renode
```

**Creating a simulation script** (`rov.resc`):
```
mach create
machine LoadPlatformDescription @platforms/boards/stm32f4_discovery.repl
sysbus LoadELF @Debug/STM32F405RGT6.elf
start
```

### Option D: STM32CubeIDE Debug Mode (Real Hardware)

This is the most accurate but requires actual hardware.

1. Connect ST-Link to STM32 board
2. **Run → Debug Configurations**
3. Create new **STM32 Debugging** configuration
4. Click **Debug**

---

## 7. Debugging with ST-Link

### Hardware Setup

```
ST-Link V2         STM32F405
┌────────┐        ┌─────────┐
│ SWDIO  ├────────┤ PA13    │
│ SWCLK  ├────────┤ PA14    │
│ GND    ├────────┤ GND     │
│ 3.3V   ├────────┤ VDD     │
└────────┘        └─────────┘
```

### Debug Configuration

1. **Run → Debug Configurations**
2. Double-click **STM32 C/C++ Application**
3. Set **Name**: `STM32F405RGT6 Debug`
4. **Main tab**:
   - C/C++ Application: `Debug/STM32F405RGT6.elf`
5. **Debugger tab**:
   - Debug probe: ST-LINK (ST-LINK GDB server)
   - Interface: SWD
6. Click **Apply** then **Debug**

### Useful Debug Features

| Feature | Shortcut | Use |
|---------|----------|-----|
| Step Over | F6 | Execute line |
| Step Into | F5 | Enter function |
| Resume | F8 | Continue execution |
| Breakpoint | Double-click line | Pause at line |
| Watch Variable | Right-click → Add Watch | Monitor value |

### Live Expressions

1. **Window → Show View → Live Expressions**
2. Add variables to monitor:
   - `motors[0].speed`
   - `thrusters_enabled`
   - `pressure_update_flag`

### SWV (Serial Wire Viewer) for Printf Debug

1. Enable ITM in debug configuration
2. Add to code:
```c
#include <stdio.h>
int _write(int file, char *ptr, int len) {
    for (int i = 0; i < len; i++) {
        ITM_SendChar(*ptr++);
    }
    return len;
}
```
3. Use `printf()` for debug output
4. View in **SWV ITM Data Console**

---

## Quick Start Checklist

- [ ] Install STM32CubeIDE
- [ ] Import project from `STM32F405RGT6` folder
- [ ] Fix `main.c` - move init before `osKernelStart()`
- [ ] Uncomment main loop processing code
- [ ] Build project (`Ctrl+B`)
- [ ] Choose simulation method (Proteus/Renode) or use real hardware
- [ ] For hardware: Connect ST-Link and debug

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Project won't import | Ensure `.project` and `.cproject` files exist |
| Build fails with HAL errors | Right-click project → Refresh, then rebuild |
| ST-Link not detected | Install/reinstall ST-Link drivers |
| Can't enter debug mode | Check SWD connections, ensure board powered |
| FreeRTOS tasks not running | Verify `osKernelStart()` is called last |

---

*Guide created for ROV Workspace - STM32F405RGT6 Firmware*
