@app.route('/')
def serve_client():
    return app.send_static_file('index.html')