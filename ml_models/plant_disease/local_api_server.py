"""
Local API Server đơn giản để test RAG
Chạy server này trước khi sử dụng RAG
"""

from flask import Flask, request, jsonify
from transformers import pipeline
import torch

app = Flask(__name__)

# Khởi tạo model khi start server
print("Đang tải model...")
generator = pipeline(
    "text-generation",
    model="gpt2",
    tokenizer="gpt2",
    device=0 if torch.cuda.is_available() else -1,
    pad_token_id=50256
)
print("Model đã được tải!")

@app.route('/generate', methods=['POST'])
def generate_text():
    try:
        data = request.json
        prompt = data.get('prompt', '')
        max_length = data.get('max_length', 256)
        temperature = data.get('temperature', 0.7)
        
        # Generate text
        result = generator(
            prompt,
            max_new_tokens=max_length,
            temperature=temperature,
            do_sample=True,
            return_full_text=False
        )
        
        generated_text = result[0]['generated_text']
        
        return jsonify({
            'status': 'success',
            'generated_text': generated_text
        })
        
    except Exception as e:
        return jsonify({
            'status': 'error',
            'message': str(e)
        }), 500

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({'status': 'healthy'})

if __name__ == '__main__':
    print("Server đang chạy tại http://localhost:5000")
    print("Endpoint: POST /generate")
    print("Health check: GET /health")
    app.run(host='0.0.0.0', port=5000, debug=True)