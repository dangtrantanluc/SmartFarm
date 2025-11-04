from fastapi import FastAPI
from contextlib import asynccontextmanager
from backend.app.routers import chatbot, plant_disease, weather, notification
from backend.app.services import notification_service

@asynccontextmanager
async def lifespan(app: FastAPI):
    # ---- Khởi động khi server bật ----
    notification_service.start_scheduler()
    print("🚀 Scheduler đã khởi động khi app start!")

    # yield = giữ app chạy
    yield

    # ---- Dọn dẹp khi server tắt ----
    notification_service.stop_scheduler()
    print("🛑 Scheduler đã dừng khi app shutdown!")
app = FastAPI(lifespan=lifespan)

app.include_router(chatbot.router, prefix="/chat", tags=["chatbot"])

app.include_router(plant_disease.router, prefix="/plant", tags=["Plant Disease"])

app.include_router(weather.router, prefix="/weather", tags=["Weather"])

app.include_router(notification.router, prefix="/notifications", tags=["Notifications"])