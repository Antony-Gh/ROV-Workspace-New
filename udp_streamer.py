import cv2
import socket
import numpy as np


# UDP settings
UDP_IP = "192.168.1.2"  # Replace with your PC's IP
UDP_PORT = 5000
MAX_DATAGRAM_SIZE = 65507  # Maximum UDP packet size

# Camera setup
cap = cv2.VideoCapture(0)  # 0 for default USB camera
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
cap.set(cv2.CAP_PROP_FPS, 20)

# Create UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

try:
   while True:
        ret, frame = cap.read()
        if not ret:
            break
            
        # Encode frame to JPEG
        _, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 80])
        
        # Split into chunks if larger than max UDP size
        data = buffer.tobytes()
        for i in range(0, len(data), MAX_DATAGRAM_SIZE):
            chunk = data[i:i+MAX_DATAGRAM_SIZE]
            sock.sendto(chunk, (UDP_IP, UDP_PORT))
            
except KeyboardInterrupt:
    print("Streaming stopped")

finally:
    cap.release()
    sock.close()
