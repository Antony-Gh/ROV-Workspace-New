#!/usr/bin/env python3
import serial
import socket
import time

uart_port = '/dev/serial0'
baudrate = 115200
laptop_ip = '192.168.1.2'
laptop_port = 14550

ser = serial.Serial(uart_port, baudrate=baudrate, timeout=1)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print(f"Forwarding UART ({uart_port}) to UDP ({laptop_ip}:{laptop_port})")

try:
    while True:
        if ser.in_waiting:
            data = ser.read(ser.in_waiting)
            if data:
                sock.sendto(data, (laptop_ip, laptop_port))
        time.sleep(0.01)  # Prevent 100% CPU usage
except KeyboardInterrupt:
    print("\nStopping forwarder")
finally:
    ser.close()
