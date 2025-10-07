from fastapi import APIRouter, UploadFile, File
from ml_models.plant_disease import predict_disease

router = APIRouter()

@router.post("/predict")
async def predict(file: UploadFile = File(...)):
    image_bytes = await file.read()
    result = predict_disease(image_bytes)
    return {"prediction": result}
