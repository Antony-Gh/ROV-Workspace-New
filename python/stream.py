import time
from flask import Flask, Response
import cv2
from datetime import datetime

app = Flask(__name__)
camera = cv2.VideoCapture(0)

def generate_frames():
    font_scale = 1
    text = "CAM 1"
    cam_pos = "Tube CAM"
    show_text = True
    last_toggle_time = time.time()
    blink_interval = 0.5 

    while True:
        success, frame = camera.read()
        if not success:
            break
        else:
            height, width, _ = frame.shape
         
            current_time = time.time()
            if current_time - last_toggle_time >= blink_interval:
                show_text = not show_text
                last_toggle_time = current_time

            if show_text:
                cv2.putText(frame, text, (width-60, 20), cv2.FONT_HERSHEY_SIMPLEX,
                            0.5, (255, 255, 255), 1, cv2.LINE_AA)

            timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            cv2.putText(frame, timestamp, (width-195, height - 20), cv2.FONT_HERSHEY_SIMPLEX,
                        0.5, (255, 255, 255), 1, cv2.LINE_AA)

            cv2.putText(frame, cam_pos, (20, height - 20), cv2.FONT_HERSHEY_SIMPLEX,
                        0.5, (255, 255, 255), 1, cv2.LINE_AA)

            ret, buffer = cv2.imencode('.jpg', frame)
            frame = buffer.tobytes()
            yield (b'--frame\r\n'
                   b'Content-Type: image/jpeg\r\n\r\n' + frame + b'\r\n')

@app.route('/video')
def video():
    return Response(generate_frames(),
                    mimetype='multipart/x-mixed-replace; boundary=frame')

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
