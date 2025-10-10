from flask_cors import CORS
from flask import Flask, request, jsonify
import speech_recognition as sr
import cv2
import numpy as np
import base64
import torch
from io import BytesIO
from PIL import Image

app = Flask(__name__)
CORS(app, resources={"/detect_objects": {"origins": "http://localhost:8000"}, "/transcribe_audio": {"origins": "http://localhost:8000"}})
recognizer = sr.Recognizer()

# Load YOLOv5 model (pre-trained)
model = torch.hub.load('ultralytics/yolov5', 'yolov5s', pretrained=True)
if torch.cuda.is_available():
    print("Using GPU for object detection")
    model = model.cuda()
else:
    print("Using CPU for object detection")

# Simulated LLM for chat (replace with actual LLM API if available)
def simulate_llm(transcription):
    return f"You said: {transcription}. How can I assist you further?"

def getCORS():
    response = jsonify({})
    response.headers['Access-Control-Allow-Origin'] = 'http://localhost:8000'
    response.headers['Access-Control-Allow-Methods'] = 'POST, OPTIONS'
    response.headers['Access-Control-Allow-Headers'] = 'Content-Type'
    return response

@app.route('/transcribe_audio', methods=['POST'])
def transcribe_audio():
    if request.method == 'OPTIONS':
        return getCORS()

    data = request.json
    audio_b64 = data['audio']
    audio_data = np.frombuffer(base64.b64decode(audio_b64), dtype=np.int16)
    print("Received audio for transcription")

    # Convert audio data to WAV format
    import scipy.io.wavfile as wavfile
    wav_io = BytesIO()
    wavfile.write(wav_io, 44100, audio_data)
    wav_io.seek(0)
    
    # Transcribe audio
    with sr.AudioFile(wav_io) as source:
        audio = recognizer.record(source)
        try:
            transcription = recognizer.recognize_google(audio)
            print("Tx: ", transcription)            
            reply = simulate_llm(transcription)
            return jsonify({'reply': reply})
        except sr.UnknownValueError:
            return jsonify({'reply': 'Could not understand audio'})
        except sr.RequestError:
            return jsonify({'reply': 'Transcription service error'})

@app.route('/detect_objects', methods=['POST'])
def detect_objects():
    if request.method == 'OPTIONS':
        return getCORS()
            
    data = request.json
    img_data = base64.b64decode(data['image'].split(',')[1])
    img = Image.open(BytesIO(img_data))
    img = np.array(img)
    img = cv2.cvtColor(img, cv2.COLOR_RGB2BGR)

    # Perform object detection
    if torch.cuda.is_available():
        results = model(img)
    else:
        results = model(img)
    objects = []
    for det in results.xyxy[0]:
        x1, y1, x2, y2, conf, cls = det
        if conf > 0.5:  # Confidence threshold
            objects.append({
                'label': model.names[int(cls)],
                'x': int(x1),
                'y': int(y1),
                'width': int(x2 - x1),
                'height': int(y2 - y1)
            })
    return jsonify({'objects': objects})

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)