from fastapi import APIRouter, UploadFile, File,HTTPException
from pydantic import BaseModel
import asyncio
from concurrent.futures import ThreadPoolExecutor
import json
# from ml_models.plant_disease import predict_disease
from ml_models.plant_disease.model_plant_disease import load_model, predict_image_bytes

# --- Cấu hình file ---
MODEL_PATH = "ml_models/resnet50_plant_disease.pth"
CLASS_NAMES_PATH = "ml_models/data/plant_disease_names.txt"  # mỗi dòng một class label
DISEASE_JSON = "ml_models/data/disease_guide.json"

# --- Load dữ liệu hỗ trợ ---
with open(DISEASE_JSON, "r", encoding="utf-8") as f:
    DISEASE_GUIDE = json.load(f)

with open(CLASS_NAMES_PATH, "r", encoding="utf-8") as f:
    CLASS_NAMES = [r.strip() for r in f.readlines()]

NUM_CLASSES = len(CLASS_NAMES)
model, class_names, device = load_model(MODEL_PATH, num_classes=NUM_CLASSES, class_names_path=CLASS_NAMES_PATH)

router = APIRouter()


# --- Thread pool để tránh block event loop ---
executor = ThreadPoolExecutor(max_workers=2)

# --- Kiểu dữ liệu trả về ---
class PredictResponse(BaseModel):
    predicted: str
    confidence: float
    alternatives: list = []
    guide: dict | None = None


@router.get("/health")
def health():
    return {"status": "ok", "device": str(device)}


@router.post("/predict", response_model=PredictResponse)
async def predict(file: UploadFile = File(...), top_k: int = 3):
    if file.content_type.split("/")[0] != "image":
        raise HTTPException(status_code=400, detail="File must be an image.")

    contents = await file.read()
    loop = asyncio.get_running_loop()

    # Chạy hàm predict trong threadpool
    results = await loop.run_in_executor(executor, predict_image_bytes, model, device, contents, CLASS_NAMES, top_k)

    top = results[0]
    predicted_label = top["label"]
    confidence = top["score"]
    guide = DISEASE_GUIDE.get(predicted_label, None)

    return {
        "predicted": predicted_label,
        "confidence": confidence,
        "alternatives": results,
        "guide": guide
    }
