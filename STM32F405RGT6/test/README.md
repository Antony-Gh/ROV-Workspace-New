# ROV Firmware Testing Guide

## Quick Start - Unit Tests

### Prerequisites
- CMake 3.16+
- GCC or MSVC compiler

### Build and Run Tests

```powershell
# Navigate to test directory
cd "e:\Robotics_Team\ROV\ROV Workspace\STM32F405RGT6\test"

# Create build directory
mkdir build
cd build

# Configure with CMake
cmake ..

# Build
cmake --build .

# Run tests
.\Debug\test_rov.exe   # Windows MSVC
# OR
.\test_rov.exe         # Windows MinGW / Linux
```

### Expected Output
```
===========================================
     ROV Firmware Unit Tests
===========================================

=== Mapping Tests ===
Running test_build_allocation_matrix...
  PASS
Running test_cholesky_solve_simple...
  PASS
...

=== PID Tests ===
Running test_clampf_in_range...
  PASS
...

========================================
Tests: 14 | Passed: 14 | Failed: 0
========================================
```

---

## Renode Emulation (Optional)

### Prerequisites
- [Renode](https://renode.io/) installed
- Built firmware (`Debug/STM32F405RGT6.elf`)

### Run Emulation

```powershell
# From project root
cd "e:\Robotics_Team\ROV\ROV Workspace\STM32F405RGT6"

# Start Renode
renode test/renode/run.resc
```

In Renode console:
```
(monitor) start    # Start emulation
(monitor) pause    # Pause emulation
(monitor) quit     # Exit
```

### Limitations
- Renode may not perfectly emulate all STM32F4 peripherals
- I2C sensor responses need mock implementations
- PWM output is simulated but motors won't respond

---

## Test Files Structure

```
test/
├── CMakeLists.txt          # Build configuration
├── mocks/
│   ├── stm32f4xx.h         # Mock HAL types
│   └── cmsis_os.h          # Mock FreeRTOS
├── unit/
│   ├── test_framework.h    # Test macros
│   ├── test_main.c         # Test runner
│   ├── test_mapping.c      # Allocation tests
│   └── test_pid.c          # PID tests
└── renode/
    ├── stm32f405_rov.repl  # Platform description
    └── run.resc            # Run script
```

---

## Adding New Tests

1. Create `test/unit/test_yourmodule.c`
2. Include `test_framework.h`
3. Write tests using `TEST()`, `ASSERT()`, `ASSERT_NEAR()`
4. Create `run_yourmodule_tests()` function
5. Call it from `test_main.c`
6. Add source file to `CMakeLists.txt`
