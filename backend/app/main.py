from fastapi import FastAPI
from backend.app.routers import chatbot, plant_disease, weather

app = FastAPI()

app.include_router(chatbot.router, prefix="/chat", tags=["chatbot"])

app.include_router(plant_disease.router, prefix="/plant", tags=["Plant Disease"])

app.include_router(weather.router, prefix="/weather", tags=["Weather"])
