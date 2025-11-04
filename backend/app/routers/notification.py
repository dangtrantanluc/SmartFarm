from fastapi import APIRouter
from pydantic import BaseModel
from datetime import datetime
from backend.app.services.firebase_service import save_notification, get_notifications, delete_notification

router = APIRouter()
class Notification(BaseModel):
    userId: str
    message: str
    timestamp: datetime

@router.get("/get/{userId}")
def get_notification_by_id(userId : str):
    return get_notifications(userId)

@router.post("/save")
async def save(request: Notification):
    save_notification(request.userId, request.message, request.timestamp)

@router.delete("/remove/{userId}/{noteKey}")
async def remove(userId: str, noteKey: str):
    delete_notification(userId, noteKey)    