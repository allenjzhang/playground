from flask import Flask, request, jsonify
from flask_cors import CORS
import cv2
import numpy as np
import base64
import torch
from io import BytesIO
from PIL import Image
from faster_whisper import WhisperModel
import os
import tempfile
import av
import time

OBOJECT_DETECTION_CONFIDENCE_THRESHOLD = 0.5
class Colors:
    RED = '\033[91m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    BLUE = '\033[94m'
    MAGENTA = '\033[95m'
    CYAN = '\033[96m'
    WHITE = '\033[97m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'
    END = '\033[0m'

app = Flask(__name__)
CORS(app, resources={"/detect_objects": {"origins": "http://localhost:8000"}, "/transcribe_audio": {"origins": "http://localhost:8000"}})

# Load YOLOv5 model
model = torch.hub.load('ultralytics/yolov5', 'yolov5s', pretrained=True)
if torch.cuda.is_available():
    print(f"{Colors.GREEN}Using GPU for object detection{Colors.END}")
    model = model.cuda()
else:
    print(f"{Colors.RED}Using CPU for object detection{Colors.END}")

# Load faster-whisper model (use "tiny" for low resources, "base" for better accuracy)
whisper_model = WhisperModel("tiny", device="cpu", compute_type="int8")  # Change to device="cuda" for GPU

def getCORS():
    response = jsonify({})
    response.headers['Access-Control-Allow-Origin'] = 'http://localhost:8000'
    response.headers['Access-Control-Allow-Methods'] = 'POST, OPTIONS'
    response.headers['Access-Control-Allow-Headers'] = 'Content-Type'
    return response

def simulate_llm(transcription):
    return f"You said: {transcription}. How can I assist you further?"

@app.route('/transcribe_audio', methods=['POST', 'OPTIONS'])
def transcribe_audio():
    if request.method == 'OPTIONS':
        return getCORS()

    if 'audio' not in request.files:
        return {'error': 'No audio file part'}, 400

    try:
        audio_file = request.files['audio']
        print('Audio data size:', audio_file.content_length)

        # Save audio to temporary file
        with tempfile.NamedTemporaryFile(delete=False, suffix='.webm') as temp_file:
            temp_audio_path = temp_file.name
            audio_file.save(temp_file)
        print('Temporary file created:', temp_audio_path, 'Size:', os.path.getsize(temp_audio_path))

        # # Save a debug copy
        # debug_dir = '/home/jetson/audio_debug'
        # os.makedirs(debug_dir, exist_ok=True)
        # debug_path = os.path.join(debug_dir, f'audio_{int(time.time())}.webm')
        # audio_file.stream.seek(0)
        # audio_file.save(debug_path)
        # print('Debug audio saved:', debug_path)

        # # Validate WebM file with PyAV
        # try:
        #     with av.open(temp_audio_path) as container:
        #         if not container.streams.audio:
        #             raise ValueError("No audio stream found in WebM file")
        #         if container.format.name not in ['matroska', 'webm']:
        #             raise ValueError(f"Invalid container format: {container.format.name}")
        #         print('WebM file validated: Audio stream found, Format:', container.format.name)
        # except Exception as e:
        #     os.unlink(temp_audio_path)
        #     raise ValueError(f"Invalid WebM file: {str(e)}")

        # Transcribe with faster-whisper
        segments, info = whisper_model.transcribe(temp_audio_path, beam_size=5, language="en")
        transcription = " ".join(segment.text.strip() for segment in segments).strip()
        print('Transcription:', transcription)
        
        # Clean up temp file
        # os.unlink(temp_audio_path)
        
        print('Transcription:', transcription)
        reply = simulate_llm(transcription)
        return jsonify({'reply': reply})
    except Exception as e:
        print('Error processing audio:', str(e))
        return jsonify({'reply': f'Error processing audio: {str(e)}'}), 500

@app.route('/detect_objects', methods=['POST', 'OPTIONS'])
def detect_objects():
    if request.method == 'OPTIONS':
        return getCORS()

    data = request.json
    print('Received image data:', len(data['image']))
    img_data = base64.b64decode(data['image'].split(',')[1])
    img = Image.open(BytesIO(img_data))
    img = np.array(img)
    img = cv2.cvtColor(img, cv2.COLOR_RGB2BGR)
    results = model(img)
    print('Detection results:', results.xyxy[0])
    objects = []
    for det in results.xyxy[0]:
        x1, y1, x2, y2, conf, cls = det
        if conf > OBOJECT_DETECTION_CONFIDENCE_THRESHOLD: # Confidence threshold
            objects.append({
                'label': model.names[int(cls)],
                'x': int(x1), 'y': int(y1),
                'width': int(x2 - x1), 'height': int(y2 - y1)
            })
    response = jsonify({'objects': objects})
    response.headers['Access-Control-Allow-Origin'] = 'http://localhost:8000'
    return response

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)