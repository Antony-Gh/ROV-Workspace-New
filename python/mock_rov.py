import time
import socket
import struct
import math
from pymavlink import mavutil

# Configuration
# Listen for incoming UDP packets on this port (GUI sends to this port)
# The GUI behaves as a client connecting to "Host_IP".
# We act as the "Server" (ROV).
GUI_IP = "127.0.0.1" 
GUI_PORT = 14550

print(f"Starting Mock ROV...")
print(f"Please configure your GUI to connect to IP: 127.0.0.1 and Port: {GUI_PORT}")

# Create MAVLink connection
# 'udpout:127.0.0.1:14550' means we SEND to the GUI on port 14550.
# The GUI must be listening on port 14550.
# source_system=1 makes us appear as the vehicle.
master = mavutil.mavlink_connection('udpout:127.0.0.1:14550', source_system=1)

print("Waiting for heartbeat from GUI (or just starting transmission)...")

# Simulation State
roll = 0.0
pitch = 0.0
yaw = 0.0
depth = 0.0
heading = 0
speed = 0.0

start_time = time.time()

try:
    while True:
        # 1. Send Heartbeat (1Hz)
        master.mav.heartbeat_send(
            mavutil.mavlink.MAV_TYPE_SUBMARINE,
            mavutil.mavlink.MAV_AUTOPILOT_ARDUPILOTMEGA,
            mavutil.mavlink.MAV_MODE_FLAG_CUSTOM_MODE_ENABLED,
            0, 0
        )

        # 2. Update Simulation Data (Sine waves for movement)
        t = time.time() - start_time
        roll = math.sin(t) * 0.5        # +/- 0.5 radians (~30 deg)
        pitch = math.cos(t) * 0.3       # +/- 0.3 radians (~17 deg)
        yaw += 0.01                     # Slow rotation
        if yaw > 2*math.pi: yaw -= 2*math.pi
        
        depth = 5.0 + math.sin(t/2) * 2.0 # Depth between 3m and 7m
        heading = int(math.degrees(yaw)) % 360

        # 3. Send Attitude (30Hz approx)
        # time_boot_ms must fit in uint32 (max ~49 days)
        # We use time since start of script
        time_boot_ms = int((time.time() - start_time) * 1000) % 4294967295
        master.mav.attitude_send(
            time_boot_ms,
            roll, pitch, yaw,
            0, 0, 0
        )


        # 4. Send VFR_HUD (Depth/Heading)
        master.mav.vfr_hud_send(
            0,      # Airspeed
            speed,  # Groundspeed
            heading,
            0,      # Throttle
            depth,  # Alt
            0       # Climb
        )

        # 5. Check for incoming messages (Manual Control)
        try:
            msg = master.recv_match(blocking=False)
        except OSError as e:
            # Handle Windows UDP 'Connection Reset' (10054) if destination is closed
            if e.errno == 10054:
                msg = None
            else:
                raise e
        except Exception:
             msg = None

        if msg:
            if msg.get_type() == 'MANUAL_CONTROL':
                print(f"RX: Manual Control - x:{msg.x}, y:{msg.y}, z:{msg.z}, r:{msg.r}")
            elif msg.get_type() == 'HEARTBEAT':
                # Just ignore or print once
                pass
            else:
                print(f"RX: {msg.get_type()}")

        time.sleep(0.033) # ~30Hz loop

except KeyboardInterrupt:
    print("\nStopping Mock ROV.")
