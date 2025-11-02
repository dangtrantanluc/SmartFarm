import pandas as pd

print("Đang đọc dữ liệu từ file JSON...")
data = pd.read_json(r'D:\DangTranTanLuc\Chatbot\ml_models\data\data.json')
print(data.head())
