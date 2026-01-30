
import socket
import threading
import time
import serial
import cv2
import math
import struct
import sys
import os

# Configuration
SERIAL_PORT = '/dev/ttyS0' # Adjust as needed (e.g. /dev/ttyACM0 or /dev/ttyAMA0)
BAUD_RATE = 115200
SURFACE_IP = '192.168.2.1' # Adjust to Surface PC IP
MAVLINK_PORT = 14550
CAM1_PORT = 5000
CAM2_PORT = 5001
CHUNK_SIZE = 60000

# Global flags
running = True

def mavlink_bridge():
    """Handles bidirectional MAVLink communication between Serial and UDP."""
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_socket.bind(('0.0.0.0', MAVLINK_PORT))
    udp_socket.settimeout(1.0)
    
    ser = None
    
    while running:
        try:
            if ser is None or not ser.is_open:
                print(f"Connecting to serial {SERIAL_PORT}...")
                ser = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=1)
                print("Serial connected.")
            
            # Serial -> UDP
            if ser.in_waiting > 0:
                data = ser.read(ser.in_waiting)
                udp_socket.sendto(data, (SURFACE_IP, MAVLINK_PORT))
                
            # UDP -> Serial
            try:
                data, addr = udp_socket.recvfrom(4096)
                ser.write(data)
            except socket.timeout:
                pass
                
        except serial.SerialException as e:
            print(f"Serial Error: {e}")
            ser = None
            time.sleep(2)
        except Exception as e:
            print(f"Bridge Error: {e}")
            time.sleep(1)

def camera_stream(camera_id, port):
    """Captures video, compresses to JPEG, fragments, and sends via UDP."""
    cap = cv2.VideoCapture(camera_id)
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    # Set lower resolution/FPS for performance if needed
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
    
    packet_sequence = 0
    
    print(f"Starting Camera {camera_id} on port {port}")
    
    while running:
        try:
            ret, frame = cap.read()
            if not ret:
                time.sleep(0.1)
                continue
                
            # Compress to JPEG
            encoded, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 70])
            data = buffer.tobytes()
            
            # Fragment if larger than CHUNK_SIZE
            if len(data) > CHUNK_SIZE:
                chunks = math.ceil(len(data) / CHUNK_SIZE)
                for i in range(chunks):
                    start = i * CHUNK_SIZE
                    end = min((i + 1) * CHUNK_SIZE, len(data))
                    chunk_data = data[start:end]
                    
                    # Protocol: Byte 0 = Flag
                    # 1=Start, 0=Middle, 2=End
                    if i == 0:
                        flag = 1 
                    elif i == chunks - 1:
                        flag = 2
                    else:
                        flag = 0
                        
                    packet = struct.pack('B', flag) + chunk_data
                    udp_socket.sendto(packet, (SURFACE_IP, port))
            else:
                # Single packet (Flag 3)
                packet = struct.pack('B', 3) + data
                udp_socket.sendto(packet, (SURFACE_IP, port))
                
            time.sleep(0.033) # Limit to ~30 FPS
            
        except Exception as e:
            print(f"Camera {camera_id} Error: {e}")
            time.sleep(1)
            # Try to reconnect camera
            cap.release()
            time.sleep(1)
            cap = cv2.VideoCapture(camera_id)

    cap.release()

if __name__ == "__main__":
    t_mav = threading.Thread(target=mavlink_bridge)
    t_cam1 = threading.Thread(target=camera_stream, args=(0, CAM1_PORT))
    # t_cam2 = threading.Thread(target=camera_stream, args=(2, CAM2_PORT)) # Uncomment for 2nd camera
    
    t_mav.start()
    t_cam1.start()
    # t_cam2.start()
    
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        running = False
        t_mav.join()
        t_cam1.join()
        print("Exiting...")
