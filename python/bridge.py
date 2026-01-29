#!/usr/bin/env python3
import serial
import socket
import select
import time

# === Configuration ===
uart_port = '/dev/serial0'
baudrate = 115200
udp_target_ip = '192.168.0.132'  # Your laptop IP
udp_target_port = 14550
udp_listen_ip = '0.0.0.0'
udp_listen_port = 14550

# === Setup UART ===
ser = serial.Serial(uart_port, baudrate=baudrate, timeout=0)

# === Setup UDP socket ===
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((udp_listen_ip, udp_listen_port))
sock.setblocking(False)

print(f"[+] Bridging UART <-> UDP ({udp_target_ip}:{udp_target_port})")

try:
    while True:
        # === Forward UART -> UDP ===
        uart_data = ser.read(ser.in_waiting or 1)
        if uart_data:
            sock.sendto(uart_data, (udp_target_ip, udp_target_port))
            print(f"[UART -> UDP] {len(uart_data)} bytes")

        # === Forward UDP -> UART ===
        try:
            udp_data, addr = sock.recvfrom(1024)
            if udp_data:
                ser.write(udp_data)
                print(f"[UDP -> UART] {len(udp_data)} bytes from {addr}")
        except BlockingIOError:
            pass  # No data yet

        time.sleep(0.01)

except KeyboardInterrupt:
    print("\n[!] Stopping bridge...")

finally:
    ser.close()
    sock.close()
