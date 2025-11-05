import firebase_admin
from firebase_admin import credentials, messaging, db
from apscheduler.schedulers.background import BackgroundScheduler
from datetime import datetime
import pytz

if not firebase_admin._apps:
    cred = credentials.Certificate("backend/serviceAccountKey.json")
    firebase_admin.initialize_app(cred)

scheduler = BackgroundScheduler(timezone='Asia/Ho_Chi_Minh')

def send_alarm_notification(userId, message, fcm_token):
    try:
        msg = messaging.Message(
            token=fcm_token,
            notification=messaging.Notification(
                title="Đã đến giờ làm việc",
                body=message
            )
        )
        response = messaging.send(msg)
        print(f"🔔 Đã gửi thông báo: {response} về thiết bị có device token là: {fcm_token}")
    except  messaging.UnregisteredError:
        print(f"⚠️ Token không còn hợp lệ cho user {userId}, xóa token này khỏi database.")
        # TODO: Xóa token đó khỏi Realtime Database hoặc Firestore
    except Exception as e:
        print(f"🔥 Lỗi khác khi gửi FCM: {e}")

def check_notifications():
    now = datetime.now(pytz.timezone('Asia/Ho_Chi_Minh')).strftime("%H:%M")
    ref = db.reference("notifications")
    notifications = ref.get() or {}

    print("Notifications:", notifications)
    for user_id, user_notifications in notifications.items():
        for notif_id, notif in user_notifications.items():
            notif_time_full = notif.get("timestamp")
            message = notif.get("message", "")
            fcm_token = get_fcm_token(user_id)

            print("Giờ hiện tại:", now)
            print("Giờ thông báo:", notif_time_full)
            print("UserID:", user_id)
            print("FCM:", fcm_token)

            # Chuyển timestamp Firebase thành định dạng HH:MM
            if notif_time_full:
                notif_time = datetime.fromisoformat(notif_time_full).strftime("%H:%M")

                if notif_time == now and fcm_token:
                    send_alarm_notification(user_id, message, fcm_token)
            
def get_fcm_token(user_id):
    user_ref = db.reference(f"users/{user_id}")
    user_data = user_ref.get()

    if user_data and "fcm_token" in user_data:
        return user_data["fcm_token"]
    else:
        return None

def start_scheduler():
    # Tránh thêm nhiều job trùng khi reload
    if not scheduler.get_jobs():
        scheduler.add_job(check_notifications, "interval", minutes=1)
        scheduler.start()
        print("✅ Scheduler thông báo đã khởi động!")

def stop_scheduler():
    if scheduler.running:
        scheduler.shutdown()
        print("🛑 Scheduler đã dừng!")