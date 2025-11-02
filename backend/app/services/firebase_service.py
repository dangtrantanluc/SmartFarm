import firebase_admin
from firebase_admin import credentials, db
from datetime import datetime

# --- Khởi tạo Firebase ---
if not firebase_admin._apps:
    cred = credentials.Certificate("backend/serviceAccountKey.json")
    firebase_admin.initialize_app(cred, {
        'databaseURL': 'https://smartfarm-d4d95-default-rtdb.firebaseio.com/'
    })

# --- Hàm lưu thông báo ---
def save_notification(user_id: str, message: str, timestamp: datetime):
    ref = db.reference(f'notifications/{user_id}')
    
    new_notification = {
        "userId": user_id,
        "message": message,
        "timestamp": timestamp.isoformat() if hasattr(timestamp, 'isoformat') else str(timestamp)
    }
    
    # Thêm thông báo mới (tự sinh key)
    ref.push(new_notification)
    print("✅ Đã lưu thông báo thành công!")

def get_notifications(user_id: str):
    try:
        ref = db.reference(f'notifications/{user_id}')
        notifications = ref.get()

        if not notifications:
            return {"status": "empty", "notifications": []}

        # Chuyển từ dict sang list để dễ xử lý phía client
        result = [
            {
                "id": key,
                **value
            }
            for key, value in notifications.items()
        ]

        # Sắp xếp thông báo mới nhất lên đầu
        result.sort(key=lambda x: x["timestamp"], reverse=True)

        return {"status": "success", "count": len(result), "notifications": result}

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    
# 2️⃣ Hàm xóa thông báo
def delete_notification(user_id: str, note_key: str):
    try:
        ref = db.reference(f"notifications/{user_id}/{note_key}")
        ref.delete()
        print(f"✅ Xóa thông báo {note_key} của user {user_id} thành công!")
    except Exception as e:
        print(f"❌ Lỗi khi xóa thông báo: {e}")
