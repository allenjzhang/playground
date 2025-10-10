from faster_whisper import WhisperModel

# Path to your test audio file
audio_path = "/tmp/tmpqrebj3w2.webm"

# Load the faster-whisper model (change device to "cuda" if you want to use GPU)
model = WhisperModel("tiny", device="cpu", compute_type="int8")

# Transcribe the audio file
segments, info = model.transcribe(audio_path, beam_size=5, language="en")

# Print the transcription results
print("Transcription segments:")
for segment in segments:
    print(f"[{segment.start:.2f}s - {segment.end:.2f}s]: {segment.text.strip()}")

# Optionally, print the full transcription
full_transcription = " ".join(segment.text.strip() for segment in segments).strip()
print("\nFull transcription:")
print(full_transcription)