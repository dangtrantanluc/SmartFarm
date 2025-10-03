from fastapi import FastAPI
from backend.app.routers import chatbot

app = FastAPI()

app.include_router(chatbot.router, prefix="/chat", tags=["chatbot"])


