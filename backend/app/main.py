from fastapi import FastAPI
from backend.app.routers import chatbot, plant_disease

app = FastAPI()

app.include_router(chatbot.router, prefix="/chat", tags=["chatbot"])

app.include_router(plant_disease.router, prefix="/plant", tags=["Plant Disease"])
