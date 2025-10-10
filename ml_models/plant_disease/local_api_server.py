"""
Local API Server đơn giản để test RAG
Chạy server này trước khi sử dụng RAG
"""

from flask import Flask, request, jsonify
from transformers import pipeline
import torch
import json
from model_plant_disease import load_model, predict_image_bytes
import asyncio
from concurrent.futures import ThreadPoolExecutor

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

# MODEL_PATH = "ml_models/resnet50_plant_disease.pth"
# CLASS_NAMES_PATH = "ml_models/data/plant_disease_names.txt"  # mỗi dòng một class label
# DISEASE_JSON = "ml_models/data/disease_guide.json"

# # Load disease guide
# with open(DISEASE_JSON, "r", encoding="utf-8") as f:
#     DISEASE_GUIDE = json.load(f)

# # Load class names
# with open(CLASS_NAMES_PATH, "r", encoding="utf-8") as f:
#     CLASS_NAMES = [r.strip() for r in f.readlines()]

# NUM_CLASSES = len(CLASS_NAMES)

# Load model plant disease(synchronous — tại startup)
model, class_names, device = load_model(MODEL_PATH, num_classes=NUM_CLASSES, class_names_path=CLASS_NAMES_PATH)

# Tạo ThreadPoolExecutor để chạy dự đoán song song
executor = ThreadPoolExecutor(max_workers=4)

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

# def predict_sync(file_bytes, top_k=3):
#     """Hàm chạy dự đoán đồng bộ trong thread executor."""
#     results = predict_image_bytes(model, device, file_bytes, CLASS_NAMES, top_k)
#     top = results[0]
#     predicted_label = top["label"]
#     confidence = top["score"]
#     guide = DISEASE_GUIDE.get(predicted_label, None)

#     return {
#         "predicted": predicted_label,
#         "confidence": confidence,
#         "alternatives": results,
#         "guide": guide
#     }

# @app.route("/predict", methods=["POST"])
# def predict():
#     # Kiểm tra có file không
#     if "file" not in request.files:
#         return jsonify({"error": "No file provided"}), 400

#     file = request.files["file"]

#     # Kiểm tra đúng loại file ảnh
#     if not file.content_type.startswith("image/"):
#         return jsonify({"error": "File must be an image"}), 400

#     # Đọc nội dung ảnh
#     file_bytes = file.read()

#     # Có thể lấy top_k từ query param, mặc định = 3
#     top_k = int(request.form.get("top_k", 3))

#     # Dự đoán trong thread riêng (để không block Flask main thread)
#     loop = asyncio.new_event_loop()
#     asyncio.set_event_loop(loop)
#     future = loop.run_in_executor(executor, predict_sync, file_bytes, top_k)
#     result = loop.run_until_complete(future)

#     return jsonify(result)


if __name__ == '__main__':
    print("Server đang chạy tại http://localhost:5000")
    print("Endpoint: POST /generate")
    print("Health check: GET /health")
    app.run(host='0.0.0.0', port=5000, debug=True)