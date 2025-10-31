from fastapi import APIRouter, Query, HTTPException
import requests
import os
from pathlib import Path
from dotenv import load_dotenv
from datetime import datetime, timedelta


# Load API key từ file .env
# load_dotenv()
# API_KEY = os.getenv("OPENWEATHER_API_KEY")
# print("Loaded API_KEY:", API_KEY)
# Xác định đường dẫn tuyệt đối tới file .env trong thư mục app/
# Trỏ tới file OpenWeather_API.env
env_path = Path(__file__).resolve().parents[1] / "OpenWeather_API.env"
print(f"Loading env file from: {env_path}")

load_dotenv(dotenv_path=env_path)

API_KEY = os.getenv("OPENWEATHER_API_KEY")
print("Loaded API_KEY:", API_KEY)

router = APIRouter()

@router.get("/weather")
def get_weather(city: str = Query("Ho Chi Minh", description="Tên thành phố, ví dụ: Hanoi")):
    
    url = f"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={API_KEY}&units=metric&lang=vi"
    
    response = requests.get(url)
    if response.status_code != 200:
        raise HTTPException(status_code=404, detail="Không tìm thấy dữ liệu thời tiết")

    data = response.json()

        # Tính giờ địa phương từ timestamp và timezone
    timestamp = data["dt"]
    timezone_offset = data["timezone"]
    local_time = datetime.utcfromtimestamp(timestamp + timezone_offset)

    # Trích xuất thông tin cần thiết
    result = {
        "city": data["name"],
        "temperature": data["main"]["temp"],
        "description": data["weather"][0]["description"].capitalize(),
        "humidity": data["main"]["humidity"],
        "icon": data["weather"][0]["icon"],
        "wind_speed": data["wind"]["speed"],
        "local_time": local_time.strftime("%Y-%m-%d %H:%M:%S")

    }
    return result

