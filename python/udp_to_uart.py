#!/usr/bin/env python3
import socket
import serial

# === UART configuration ===
uart_port = '/dev/serial0'  # Or use '/dev/ttyUSB0' if USB UART
baudrate = 115200
ser = serial.Serial(uart_port, baudrate=baudrate, timeout=0.1)

# === UDP configuration ===
udp_ip = '0.0.0.0'  # Listen on all interfaces
udp_port = 14550
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((udp_ip, udp_port))

print(f"Listening on UDP {udp_ip}:{udp_port} and forwarding to UART {uart_port} @ {baudrate}bps")

try:
    while True:
        data, addr = sock.recvfrom(1024)
        if data:
            ser.write(data)
            print(f"Forwarded {len(data)} bytes from {addr} to UART")

except KeyboardInterrupt:
    print("Stopping...")
finally:
    ser.close()
    sock.close()
